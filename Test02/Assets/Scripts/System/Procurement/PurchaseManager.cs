using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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
    public Button openButton;
    public Button closeButton;
    public Transform contentContainer;
    public GameObject itemPrefab;

    [Header("滚动文案设置")]
    public TMP_Text marqueeText;
    public RectTransform marqueeContainer;
    public float scrollSpeed = 50f;

    [Header("采购配置")]
    public List<PurchaseConfig> configs;

    // 销售追踪
    private Dictionary<Coffee.CoffeeType, int> salesHistory = new Dictionary<Coffee.CoffeeType, int>();
    private List<float> saleTimestamps = new List<float>();
    private List<Coffee.CoffeeType> saleTypes = new List<Coffee.CoffeeType>();

    void Start()
    {
        purchasePanel.SetActive(false);
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);

        // 订阅销售事件
        if (EventManager.Instance != null)
            EventManager.Instance.OnOrderCompleted += RecordSale;

        InitList();
    }

    void Update()
    {
        if (purchasePanel.activeSelf && marqueeText != null)
        {
            ScrollMarquee();
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

    public void OpenPanel()
    {
        UpdateMarketTrend();
        purchasePanel.SetActive(true);
    }

    public void ClosePanel() => purchasePanel.SetActive(false);

    public void TryPurchase(string id, int cost, int amount)
    {
        if (GameManager.Instance.SpendMoney(cost, "采购原料:" + id))
        {
            IngredientSystem.Instance.AddIngredient(id, amount);
            EventManager.Instance.TriggerGameLog($"采购成功：增加了 {amount} {id}");
        }
        else
        {
            EventManager.Instance.TriggerGameLog("金币不足，无法采购！", LogType.Warning);
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

        // 动态调价逻辑：热门饮品售价临时上涨20%
        // 注意：这里需要修改DataManager中的价格或在结算时计算
        string drinkName = GetDrinkChineseName(topDrink);
        marqueeText.text = $"【市场热点】{drinkName} 近期走红（售出{count}杯），建议零售价已上调！补货需及时！";

        // 实际效果：直接影响DataManager里的单价
        // DataManager.Instance.UpdatePrice(topDrink, ...); // 如果DataManager支持的话
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
