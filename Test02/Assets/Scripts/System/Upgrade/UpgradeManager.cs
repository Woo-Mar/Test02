using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public Image backgroundImage;
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
        upgradePanel.SetActive(false);
        openButton.onClick.AddListener(() => upgradePanel.SetActive(true));
        closeButton.onClick.AddListener(() => upgradePanel.SetActive(false));

        // 默认背景1已解锁
        if (bgUpgrades.Count > 0) bgUpgrades[0].isUnlocked = true;

        InitUI();
    }

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
    public void TryUpgrade(int id, int cost, UpgradeItemUI clickedItemUI)
    {
        // 如果这里之前的字符串是乱码，建议修正回正常的提示文字
        if (GameManager.Instance.SpendMoney(cost, "升级消费"))
        {
            // 1. 设置数据以及应用游戏效果
            ApplyUpgradeEffect(id);

            // 2. 更新刚才被点击的这个 UI 表现，它的按键将完全被禁用并变成 "已解锁"
            clickedItemUI.SetUnlocked();

            // 3. 触发日志反馈
            EventManager.Instance.TriggerGameLog("升级成功！");

            // 你原本这里有一段重新开关UI面板的暴力刷新代码，现在有了 clickedItemUI.SetUnlocked(); 这一步属于精准刷新，不再需要开关面板了。
        }
    }

    void ApplyUpgradeEffect(int id)
    {
        if (id < 100) // 背景类
        {
            bgUpgrades[id].isUnlocked = true;
            // 1、切换背景图
            if (id < backgroundSprites.Length)
            {
                backgroundImage.sprite = backgroundSprites[id];
            }
            // 2、切换桌子图
            if (tableSR != null && id < tableSprites.Length)
            {
                tableSR.sprite = tableSprites[id];
            }
            tipMultiplier = 1.0f + (id * 0.1f); // 背景2加10%，背景3加20%
        }
        else // 设施类
        {
            int index = id - 100;
            facilityUpgrades[index].isUnlocked = true;
            if (index == 0)
            {
                coffeeMachineSR.sprite = upgradedMachineSprite;
                priceMultiplier = 1.2f;
            }
            if (index == 1) { exhibit1.SetActive(true); patienceBonus += 10f; }
            if (index == 2) { exhibit2.SetActive(true); patienceBonus += 10f; }
            
        }
    }
}
