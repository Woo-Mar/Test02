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

    // 【新增】保存所有生成的 UI 项，方便直接刷新
    private List<AchievementItemUI> spawnedUIItems = new List<AchievementItemUI>();

    void Awake() { Instance = this; }

    void Start()
    {
        achievementPanel.SetActive(false);
        openButton.onClick.AddListener(OpenAchievementPanel); // 修改为调用新方法
        closeButton.onClick.AddListener(() => achievementPanel.SetActive(false));

        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnOrderCompleted += OnOrderCompleted;
            EventManager.Instance.OnMoneyEarned += OnMoneyEarned;
            // ... 其他事件
        }

        // 初始生成一次 UI 列表
        InitializeUI();
    }

    // 【修改】点击打开按钮时，先刷新数据再显示
    void OpenAchievementPanel()
    {
        RefreshAllUI();
        achievementPanel.SetActive(true);
    }

    // 【修改】只在初始化时生成物体
    void InitializeUI()
    {
        foreach (Transform child in contentContainer) Destroy(child.gameObject);
        spawnedUIItems.Clear();

        foreach (var ach in achievements)
        {
            GameObject go = Instantiate(itemPrefab, contentContainer);
            AchievementItemUI ui = go.GetComponent<AchievementItemUI>();
            ui.Setup(ach, this);
            spawnedUIItems.Add(ui); // 存入列表
        }
    }

    // 【新增】刷新所有已存在的 UI 文字，而不销毁物体
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
        var ach = achievements.Find(a => a.id == id);
        if (ach != null && !ach.isClaimed) // 即使达成了也可以继续更新直到领取
        {
            ach.currentProgress += addValue;
            CheckReached(ach);

            // 【关键】进度变了，立即刷新 UI 显示
            RefreshAllUI();
        }
    }

    public void SetProgress(string id, int value)
    {
        var ach = achievements.Find(a => a.id == id);
        if (ach != null && !ach.isClaimed)
        {
            ach.currentProgress = value;
            CheckReached(ach);

            // 【关键】进度变了，立即刷新 UI 显示
            RefreshAllUI();
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
            RefreshAllUI(); // 领取后刷新按钮状态
        }
    }

    // 监听事件
    void OnOrderCompleted(Customer customer, int reward)
    {
        // 示例：每完成一个订单，某个总数成就+1
        UpdateProgress("total_orders", 1);

        // 检查是否包含无花果茶
        foreach (var type in customer.orders)
        {
            if (type == Coffee.CoffeeType.FigOnly) UpdateProgress("fig_master", 1);
        }
    }

    void OnMoneyEarned(int amount, string source)
    {
        // 累计金币成就
        UpdateProgress("money_collector", amount);
    }
}
