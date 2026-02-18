// UpgradeManager.cs - 升级系统框架
using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [System.Serializable]
    public class Upgrade
    {
        public string id;
        public string displayName;
        public string description;
        public int level = 0;
        public int maxLevel = 5;
        public int baseCost = 100;
        public float costMultiplier = 1.5f;
        public Sprite icon;
        public Action<int> applyUpgrade; // 应用升级效果，参数为当前等级
    }

    [Header("咖啡机升级")]
    public Upgrade grindSpeedUpgrade;
    public Upgrade brewSpeedUpgrade;
    public Upgrade coffeeQualityUpgrade;

    [Header("原料升级")]
    public Upgrade ingredientEfficiencyUpgrade;
    public Upgrade ingredientCapacityUpgrade;

    [Header("顾客升级")]
    public Upgrade customerPatienceUpgrade;
    public Upgrade customerRewardUpgrade;

    [Header("员工升级")]
    public Upgrade autoBrewUpgrade;
    public Upgrade autoServeUpgrade;

    private Dictionary<string, Upgrade> allUpgrades = new Dictionary<string, Upgrade>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeUpgrades()
    {
        // 咖啡机升级
        grindSpeedUpgrade = new Upgrade
        {
            id = "grind_speed",
            displayName = "研磨速度",
            description = "提高咖啡研磨速度",
            applyUpgrade = (level) => {
                // 这里可以修改咖啡机的研磨时间
                Debug.Log($"研磨速度升级到 {level} 级");
            }
        };

        // 初始化所有升级
        allUpgrades[grindSpeedUpgrade.id] = grindSpeedUpgrade;

        // 订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCoffeeGrinded += OnCoffeeGrinded;
            EventManager.Instance.OnOrderCompleted += OnOrderCompleted;
        }
    }

    void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCoffeeGrinded -= OnCoffeeGrinded;
            EventManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        }
    }

    public bool PurchaseUpgrade(string upgradeId)
    {
        if (!allUpgrades.ContainsKey(upgradeId))
            return false;

        Upgrade upgrade = allUpgrades[upgradeId];

        if (upgrade.level >= upgrade.maxLevel)
            return false;

        int cost = CalculateUpgradeCost(upgrade);

        if (GameManager.Instance.SpendMoney(cost, $"升级:{upgrade.displayName}"))
        {
            upgrade.level++;
            upgrade.applyUpgrade?.Invoke(upgrade.level);
            return true;
        }

        return false;
    }

    public int CalculateUpgradeCost(Upgrade upgrade)
    {
        return Mathf.RoundToInt(upgrade.baseCost * Mathf.Pow(upgrade.costMultiplier, upgrade.level));
    }

    void OnCoffeeGrinded(string coffeeType)
    {
        // 根据研磨速度升级，可能有额外效果
        if (grindSpeedUpgrade.level > 0)
        {
            // 例如：研磨速度提升，减少等待时间
        }
    }

    void OnOrderCompleted(Customer customer, int totalReward)
    {
        if (customerRewardUpgrade.level > 0)
        {
            int bonus = Mathf.RoundToInt(totalReward * 0.1f * customerRewardUpgrade.level);
            if (bonus > 0)
            {
                GameManager.Instance.AddMoney(bonus, "升级奖励");
            }
        }
    }
}