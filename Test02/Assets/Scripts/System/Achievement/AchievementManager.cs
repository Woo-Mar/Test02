using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("UI引用")]
    public GameObject achievementPanel;
    public Button openButton;
    public Button closeButton;
    public Transform contentContainer;
    public GameObject itemPrefab;

    [Header("成就配置")]
    public List<Achievement> achievements = new List<Achievement>();

    private List<AchievementItemUI> spawnedUIItems = new List<AchievementItemUI>();

    void Awake() { Instance = this; }

    void Start()
    {
        achievementPanel.SetActive(false);
        openButton.onClick.AddListener(OpenAchievementPanel);
        closeButton.onClick.AddListener(() => achievementPanel.SetActive(false));

        // 确保事件订阅
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnOrderCompleted += OnOrderCompleted;
            EventManager.Instance.OnMoneyEarned += OnMoneyEarned;
            Debug.Log("[成就系统] 已成功订阅事件");
        }

        InitializeUI();
    }

    void OpenAchievementPanel()
    {
        RefreshAllUI();
        achievementPanel.SetActive(true);
    }

    void InitializeUI()
    {
        // 清理旧物体
        foreach (Transform child in contentContainer) Destroy(child.gameObject);
        spawnedUIItems.Clear();

        foreach (var ach in achievements)
        {
            GameObject go = Instantiate(itemPrefab, contentContainer);
            AchievementItemUI ui = go.GetComponent<AchievementItemUI>();
            if (ui != null)
            {
                ui.Setup(ach, this);
                spawnedUIItems.Add(ui);
            }
        }
        Debug.Log($"[成就系统] UI初始化完成，共生成 {spawnedUIItems.Count} 个项");
    }

    public void RefreshAllUI()
    {
        foreach (var ui in spawnedUIItems)
        {
            if (ui != null) ui.RefreshUI();
        }
    }

    // --- 进度更新逻辑 ---
    public void UpdateProgress(string id, int addValue)
    {
        // 查找成就
        var ach = achievements.Find(a => a.id == id);
        if (ach != null)
        {
            if (!ach.isClaimed)
            {
                ach.currentProgress += addValue;
                CheckReached(ach);
                RefreshAllUI(); // 进度改变立即刷新
                Debug.Log($"[成就系统] 成就 {id} 进度更新: {ach.currentProgress}/{ach.goalValue}");
            }
        }
        else
        {
            // 如果你在控制台看到这条报错，说明你在Inspector里填的ID和代码里的不一致！
            Debug.LogWarning($"[成就系统] 警告：尝试更新未知的成就ID: {id}");
        }
    }

    void CheckReached(Achievement ach)
    {
        if (ach.currentProgress >= ach.goalValue)
        {
            ach.currentProgress = ach.goalValue;
            if (!ach.isReached)
            {
                ach.isReached = true;
                EventManager.Instance.TriggerGameLog($"★成就达成：{ach.name}！");
            }
        }
    }

    public void ClaimReward(string id)
    {
        var ach = achievements.Find(a => a.id == id);
        if (ach != null && ach.isReached && !ach.isClaimed)
        {
            ach.isClaimed = true;
            GameManager.Instance.AddMoney(ach.rewardMoney, "成就奖励");
            RefreshAllUI();
        }
    }

    void OnOrderCompleted(Customer customer, int reward)
    {
        Debug.Log("[成就系统] 监听到订单完成，正在更新进度...");
        UpdateProgress("total_orders", 1);

        foreach (var type in customer.orders)
        {
            if (type == Coffee.CoffeeType.FigOnly)
                UpdateProgress("fig_master", 1);
        }
    }

    void OnMoneyEarned(int amount, string source)
    {
        UpdateProgress("money_collector", amount);
    }
}
