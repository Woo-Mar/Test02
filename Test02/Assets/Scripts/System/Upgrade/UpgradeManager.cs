using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// using UnityEngine.UIElements;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [System.Serializable]
    public class UpgradeData
    {
        public string name;
        public int cost;
        public string description;
        public Sprite icon;
        public bool isUnlocked;
    }

    [Header("UI引用")]
    public GameObject upgradePanel;
    public Button openButton;
    public Button closeButton;
    public Transform leftColumn;  // 背景升级容器
    public Transform rightColumn; // 设施升级容器
    public GameObject upgradeItemPrefab;

    [Header("场景引用")]
    public SpriteRenderer background;
    public GameObject exhibit1;
    public GameObject exhibit2;
    public SpriteRenderer coffeeMachineSR;
    public SpriteRenderer tableSR;        // 场景中桌子的 SpriteRenderer


    [Header("资源配置")]
    public Sprite[] backgroundSprites; // 3张背景图
    public Sprite[] tableSprites;         // 3张不同等级的桌子图片
    public Sprite upgradedMachineSprite; // 升级后的咖啡机图
    public List<UpgradeData> bgUpgrades; // 对应左列
    public List<UpgradeData> facilityUpgrades; // 对应右列

    // 升级效果数值
    public float tipMultiplier = 1.0f;      // 小费倍率
    public float patienceBonus = 0f;        // 耐心加成
    public float priceMultiplier = 1.0f;    // 价格倍率

    private List<UpgradeItemUI> allUIItems = new List<UpgradeItemUI>();

    void Awake() { Instance = this; }

    void Start()
    {
        openButton.onClick.AddListener(OpenUpgradePanel);
        closeButton.onClick.AddListener(MenuManager.Instance.CloseMenu);


        // 默认背景1已解锁
        if (bgUpgrades.Count > 0) bgUpgrades[0].isUnlocked = true;

        InitUI();
    }

    public void OpenUpgradePanel()
    {
        upgradePanel.SetActive(true);
    }

    //public void CloseUpgradePanel()
    //{
    //    upgradePanel.SetActive(false);
    //}

    void InitUI()
    {
        // 清理并创建左列（背景）
        for (int i = 0; i < bgUpgrades.Count; i++)
        {
            CreateItem(leftColumn, bgUpgrades[i], i);
        }
        // 清理并创建右列（设施）
        for (int i = 0; i < facilityUpgrades.Count; i++)
        {
            CreateItem(rightColumn, facilityUpgrades[i], i + 100); // 偏移ID区分
        }
    }

    void CreateItem(Transform parent, UpgradeData data, int index)
    {
        GameObject go = Instantiate(upgradeItemPrefab, parent);
        var ui = go.GetComponent<UpgradeItemUI>();
        ui.Setup(index, data.name, data.cost, data.description, data.icon, data.isUnlocked, this);
        allUIItems.Add(ui);
    }

    // 【修改2】增加第三个参数接收点击的对应 UI
    // 【修改1】将返回值改为 bool，用于判断升级是否真正成功应用
    public void TryUpgrade(int id, int cost, UpgradeItemUI clickedItemUI)
    {
        // 先检查是否满足金币条件
        if (GameManager.Instance.money < cost)
        {
            EventManager.Instance.TriggerGameLog("金币不足，无法升级！", LogType.Warning);
            return;
        }

        // 尝试应用效果，如果内部条件不满足会返回 false
        if (ApplyUpgradeEffect(id))
        {
            // 只有真正成功了，才扣钱和更新UI
            if (GameManager.Instance.SpendMoney(cost, "升级消费"))
            {
                clickedItemUI.SetUnlocked();
                EventManager.Instance.TriggerGameLog("升级成功！");
            }
        }
    }

    // 【修改2】改为 bool 类型
    bool ApplyUpgradeEffect(int id)
    {
        if (id < 100) // 背景类
        {
            bgUpgrades[id].isUnlocked = true;
            if (id < backgroundSprites.Length) background.sprite = backgroundSprites[id];
            if (tableSR != null && id < tableSprites.Length) tableSR.sprite = tableSprites[id];
            tipMultiplier = 1.0f + (id * 0.1f);
            return true;
        }
        else // 设施类
        {
            int index = id - 100;
            if (index == 0) // 咖啡机升级
            {
                // 检查：背景必须达到第3级 (index 2)
                if (bgUpgrades.Count >= 3 && bgUpgrades[2].isUnlocked)
                {
                    facilityUpgrades[index].isUnlocked = true;

                    // 确保这里能找到咖啡机
                    CoffeeMachine machine = FindObjectOfType<CoffeeMachine>();
                    if (machine != null)
                    {
                        machine.isAutomatic = true;
                        // 如果咖啡机里有手动研磨次数，记得重置它（可选）
                    }

                    if (coffeeMachineSR != null) coffeeMachineSR.sprite = upgradedMachineSprite;
                    return true; // 真正成功
                }
                else
                {
                    EventManager.Instance.TriggerGameLog("升级失败：需要先将店铺背景升级至第三级！", LogType.Warning);
                    return false; // 条件不满足，升级失败
                }
            }

            if (index == 1) // 道路升级
            {
                facilityUpgrades[index].isUnlocked = true;
                PurchaseManager pm = FindObjectOfType<PurchaseManager>();
                if (pm != null) pm.deliveryTime = 10f;
                return true;
            }

            if (index == 2)
            {
                facilityUpgrades[index].isUnlocked = true;
                if (exhibit2 != null) exhibit2.SetActive(true);
                patienceBonus += 10f;
                return true;
            }

            return false;
        }
    }
}
