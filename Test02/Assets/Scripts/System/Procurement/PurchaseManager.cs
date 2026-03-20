using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class PurchaseManager : MonoBehaviour
{
    [System.Serializable]
    public class PurchaseConfig
    {
        public string id;
        public string displayName;
        public int packAmount; // 一份的数量
        public int baseBuyPrice; // 购买价格
    }

    [Header("UI面板")]
    public GameObject purchasePanel;
    //public Button openButton;
    public Button closeButton;
    public Transform contentContainer;
    public GameObject itemPrefab;

    [Header("滚动文案设置")]
    public TMP_Text marqueeText;
    public RectTransform marqueeContainer;
    public float scrollSpeed = 100f;

    [Header("采购配置")]
    public List<PurchaseConfig> configs;

    [Header("配送时间")]
    public float deliveryTime = 30f; // 初始30秒

    private float deliveryMessageEndTime = 0f;
    // 在类中定义一个计时器
    private float nextMarketUpdate = 0f;

    // 销售追踪
    private Dictionary<Coffee.CoffeeType, int> salesHistory = new Dictionary<Coffee.CoffeeType, int>();
    private List<float> saleTimestamps = new List<float>();
    private List<Coffee.CoffeeType> saleTypes = new List<Coffee.CoffeeType>();

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(MenuManager.Instance.CloseMenu);

        // 订阅销售事件
        if (EventManager.Instance != null)
            EventManager.Instance.OnOrderCompleted += RecordSale;

        InitList();
    }

    void Update()
    {
        if (marqueeText != null)
        {
            ScrollMarquee();

            // 仅在到达时间点时更新市场热点，避免每一帧都做复杂的计算
            if (Time.time >= nextMarketUpdate)
            {
                UpdateMarketTrend();
                nextMarketUpdate = Time.time + 10f; // 每10秒更新一次
            }
        }
    }

    void InitList()
    {
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        foreach (var config in configs)
        {
            GameObject go = Instantiate(itemPrefab, contentContainer);
            var ui = go.GetComponent<PurchaseItemUI>();
            // 尝试从库存系统获取图标
            Sprite icon = IngredientSystem.Instance.GetIngredient(config.id)?.icon;
            ui.Setup(config.id, config.displayName, config.baseBuyPrice, config.packAmount, icon, this);
        }
    }


    public void TryPurchase(string id, int cost, int amount)
    {
        AudioManager.Instance.PlayButtonSound();
        // --- 1. 阶段锁定逻辑 ---
        if (ProgressGuideManager.Instance.guideStep == 1)
        {
            if (id != "fig" && id != "cup")
            {
                SetMarqueeText("由于交通不便，该物资暂时无法送达！");
                return;
            }
        }

        // --- 2. 购买逻辑 ---
        if (GameManager.Instance.SpendMoney(cost, "购买:" + id))
        {
            // 【新增逻辑】：检查道路是否已升级
            // 根据之前的代码，facilityUpgrades[1] 是道路升级
            bool isRoadUpgraded = false;
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.facilityUpgrades.Count > 1)
            {
                isRoadUpgraded = UpgradeManager.Instance.facilityUpgrades[1].isUnlocked;
            }
            deliveryMessageEndTime = Time.time + 5f;
            // 根据状态显示不同文字
            if (isRoadUpgraded)
            {
                SetMarqueeText($"订单已发出，道路已修好，预计{deliveryTime:F0}秒送达！");
            }
            else
            {
                SetMarqueeText($"订单已发出，由于路不好走，预计{deliveryTime:F0}秒送达！");
            }

            StartCoroutine(DeliveryProcess(id, amount));
        }
        else
        {
            EventManager.Instance.TriggerGameLog("金币不足，无法购买！", LogType.Warning);
        }
    }

    IEnumerator DeliveryProcess(string id, int amount)
    {
        yield return new WaitForSeconds(deliveryTime);
        IngredientSystem.Instance.AddIngredient(id, amount);
        EventManager.Instance.TriggerGameLog($"物资 {id} 已送达仓库！");
    }

    void SetMarqueeText(string msg)
    {
        if (marqueeText != null)
        {
            marqueeText.text = msg;
            Debug.Log($"[跑马灯] 文字已更新为: {msg}");
        }
        else
        {
            Debug.LogError("[跑马灯] 错误：marqueeText 引用丢失，请在 Inspector 中赋值！");
        }
    }

    // --- 市场动态逻辑 ---
    void RecordSale(Customer customer, int reward)
    {
        // 这里需要获取最后成交的咖啡类型，假设我们从CoffeeMachine获取当前类型
        // 简化处理：从订单中获取
        Coffee.CoffeeType type = customer.orders[0]; // 记录第一杯
        saleTimestamps.Add(Time.time);
        saleTypes.Add(type);
    }

    void UpdateMarketTrend()
    {
        // 【新增】如果当前时间还在配送文案的显示时间内，直接跳过，不更新市场热点
        if (Time.time < deliveryMessageEndTime) return;
        // 清理5分钟以前的数据 (300秒)
        float threshold = Time.time - 300f;
        while (saleTimestamps.Count > 0 && saleTimestamps[0] < threshold)
        {
            saleTimestamps.RemoveAt(0);
            saleTypes.RemoveAt(0);
        }

        if (saleTypes.Count == 0)
        {
            marqueeText.text = "当前市场平稳，暂无热门饮品推荐。";
            return;
        }

        // 统计最热门
        var topDrink = saleTypes.GroupBy(t => t)
                                .OrderByDescending(g => g.Count())
                                .First().Key;

        int count = saleTypes.Count(t => t == topDrink);

        string drinkName = GetDrinkChineseName(topDrink);
        marqueeText.text = $"【市场热点】{drinkName} 近期走红（售出{count}杯），补货需及时！";
    }

    void ScrollMarquee()
    {
        marqueeText.rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.unscaledDeltaTime;
        if (marqueeText.rectTransform.anchoredPosition.x < -marqueeText.preferredWidth - 200)
        {
            marqueeText.rectTransform.anchoredPosition = new Vector2(marqueeContainer.rect.width, 0);
        }
    }

    string GetDrinkChineseName(Coffee.CoffeeType type)
    {
        switch (type)
        {
            case Coffee.CoffeeType.HotCoffee: return "热美式";
            case Coffee.CoffeeType.IcedCoffee: return "冰美式";
            case Coffee.CoffeeType.Latte: return "拿铁";
            case Coffee.CoffeeType.StrawberryLatte: return "草莓拿铁";
            case Coffee.CoffeeType.CarambolaAmericano: return "杨桃美式";
            case Coffee.CoffeeType.FigOnly: return "无花果茶";
            default: return "特色饮品";
        }
    }
}
