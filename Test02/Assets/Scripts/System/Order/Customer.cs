// Customer.cs - 修改后的版本（仅开心/生气表情）
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    public enum CustomerType
    {
        Normal,
        VIP,
        Impatient
    }

    private Color maxPatienceColor;

    [Header("音效")]
    public AudioClip happySound;   // 正确交付时的开心音效
    public AudioClip angrySound;   // 生气时的音效

    [Header("顾客表情外观")]
    public Sprite happyNormalSprite;
    public Sprite angryNormalSprite;

    public Sprite happyVipSprite;
    public Sprite angryVipSprite;

    public Sprite happyImpatientSprite;
    public Sprite angryImpatientSprite;

    [Header("Settings")]
    public float patience = 40f;        // 总耐心时间（秒）
    public int baseReward = 10;         // 基础奖励金币

    [Header("UI")]
    public Slider patienceSlider;       // 耐心条UI
    public Image orderIcon;             // 订单图标
    public TextMeshProUGUI remainingOrdersText;  // 显示剩余杯数的UI文字，比如 "×2"

    [Header("订单图标精灵")]
    public Sprite hotCoffeeOrderSprite;
    public Sprite icedCoffeeOrderSprite;
    public Sprite latteOrderSprite;
    public Sprite strawberryLatteOrderSprite;
    public Sprite carambolaAmericanoOrderSprite;
    public Sprite figTeaOrderSprite;

    // 订单相关（非Inspector）
    public CustomerType customerType { get; private set; }
    public List<Coffee.CoffeeType> orders = new List<Coffee.CoffeeType>();
    private List<bool> ordersCompleted = new List<bool>();
    public int completedCount { get; private set; }
    public int totalOrders => orders.Count;
    public int remainingOrders => totalOrders - completedCount;

    private float rewardMultiplier = 1f;        // VIP奖励倍率
    private SpriteRenderer spriteRenderer;       // 角色本体渲染器
    private float currentPatience;                // 当前剩余耐心
    private bool isWaiting = true;                // 是否在等待状态
    private bool isServed = false;                // 是否已被服务
    private bool isLeaving = false;                // 是否正在离开

    [HideInInspector]
    public Transform spawnPoint;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentPatience = patience;

        UpdateCustomerSprite(true);


        // 默认一个订单（会被 InitializeOrders 覆盖）
        if (orders.Count == 0)
        {
            orders = new List<Coffee.CoffeeType> { Coffee.CoffeeType.HotCoffee };
            ordersCompleted = new List<bool> { false };
        }
        // 读取 Fill 的颜色
        if (patienceSlider != null)
        {
            Image fillImage = patienceSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                maxPatienceColor = fillImage.color;
            }
        }

        InitializeUI();
        UpdateRemainingText();
        UpdateOrderIcon();
    }

    void UpdateCustomerSprite(bool isHappy)
    {
        if (spriteRenderer == null) return;

        switch (customerType)
        {
            case CustomerType.VIP:
                spriteRenderer.sprite = isHappy ? happyVipSprite : angryVipSprite;
                break;

            case CustomerType.Impatient:
                spriteRenderer.sprite = isHappy ? happyImpatientSprite : angryImpatientSprite;
                break;

            default:
                spriteRenderer.sprite = isHappy ? happyNormalSprite : angryNormalSprite;
                break;
        }
    }



    /// <summary>
    /// 随机生成咖啡类型
    /// </summary>
    private Coffee.CoffeeType GetRandomCoffeeType()
    {
        float randomValue = Random.value;
        if (randomValue < 0.2f) return Coffee.CoffeeType.HotCoffee;
        else if (randomValue < 0.4f) return Coffee.CoffeeType.IcedCoffee;
        else if (randomValue < 0.6f) return Coffee.CoffeeType.Latte;
        else if (randomValue < 0.75f) return Coffee.CoffeeType.StrawberryLatte;
        else if (randomValue < 0.9f) return Coffee.CoffeeType.CarambolaAmericano;
        else return Coffee.CoffeeType.FigOnly;
    }

    void InitializeUI()
    {
        if (patienceSlider != null)
        {
            patienceSlider.maxValue = patience;
            patienceSlider.value = currentPatience;
        }
    }

    /// <summary>
    /// 由订单管理器调用，初始化顾客的订单和类型
    /// </summary>
    public void InitializeOrders(List<Coffee.CoffeeType> orderList, CustomerType type)
    {
        // 确保 spriteRenderer 已赋值
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Debug.Log($"InitializeOrders 被调用，type={type}");

        orders = new List<Coffee.CoffeeType>(orderList);
        ordersCompleted = new List<bool>(new bool[orders.Count]);
        completedCount = 0;
        customerType = type;

        switch (type)
        {
            case CustomerType.VIP:
                rewardMultiplier = 1.5f;
                break;
            case CustomerType.Impatient:
                rewardMultiplier = 1f;
                patience *= 0.6f;
                currentPatience = patience; // 同步当前耐心值
                break;
            default:
                rewardMultiplier = 1f;
                break;
        }

        // 设置顾客本体外观
        if (spriteRenderer != null)
        {
            switch (type)
            {
                case CustomerType.VIP:
                    spriteRenderer.sprite = happyVipSprite ?? happyNormalSprite;
                    Debug.Log($"VIP顾客设置精灵: {(happyVipSprite != null ? happyVipSprite.name : "null")}，最终使用: {spriteRenderer.sprite.name}");
                    break;
                case CustomerType.Impatient:
                    spriteRenderer.sprite = happyImpatientSprite ?? happyNormalSprite;
                    Debug.Log($"Impatient顾客设置精灵: {(happyImpatientSprite != null ? happyImpatientSprite.name : "null")}，最终使用: {spriteRenderer.sprite.name}");
                    break;
                default:
                    spriteRenderer.sprite = happyNormalSprite;
                    Debug.Log($"普通顾客设置精灵: {happyNormalSprite.name}");
                    break;
            }
        }
        else
        {
            Debug.LogError("spriteRenderer 仍然为 null！请检查顾客预制体上是否有 SpriteRenderer 组件。");
        }

        // 更新UI
        UpdateRemainingText();
        UpdateOrderIcon();

        if (patienceSlider != null)
        {
            patienceSlider.maxValue = patience;
            patienceSlider.value = currentPatience;
        }

        UpdateCustomerSprite(true);


        if (UpgradeManager.Instance != null)
        {
            patience += UpgradeManager.Instance.patienceBonus;
            currentPatience = patience;
        }
    }

    private void UpdateOrderIcon()
    {
        if (orderIcon == null) return;

        for (int i = 0; i < orders.Count; i++)
        {
            if (!ordersCompleted[i])
            {
                SetOrderIcon(orders[i]);
                return;
            }
        }
        orderIcon.gameObject.SetActive(false);
    }

    private void UpdateRemainingText()
    {
        if (remainingOrdersText != null)
        {
            if (remainingOrders > 0)
            {
                remainingOrdersText.text = $"×{remainingOrders}";
                remainingOrdersText.gameObject.SetActive(true);
            }
            else
            {
                remainingOrdersText.gameObject.SetActive(false);
            }
        }
    }

    private void SetOrderIcon(Coffee.CoffeeType coffeeType)
    {
        if (orderIcon == null) return;
        switch (coffeeType)
        {
            case Coffee.CoffeeType.HotCoffee:
                orderIcon.sprite = hotCoffeeOrderSprite; break;
            case Coffee.CoffeeType.IcedCoffee:
                orderIcon.sprite = icedCoffeeOrderSprite; break;
            case Coffee.CoffeeType.Latte:
                orderIcon.sprite = latteOrderSprite; break;
            case Coffee.CoffeeType.StrawberryLatte:
                orderIcon.sprite = strawberryLatteOrderSprite; break;
            case Coffee.CoffeeType.CarambolaAmericano:
                orderIcon.sprite = carambolaAmericanoOrderSprite; break;
            case Coffee.CoffeeType.FigOnly:
                orderIcon.sprite = figTeaOrderSprite; break;
        }
    }

    void Update()
    {
        if (isWaiting && !isServed && !isLeaving)
        {
            currentPatience -= Time.deltaTime;
            if (patienceSlider != null)
                patienceSlider.value = currentPatience;

            UpdatePatienceColor();

            if (currentPatience <= 0)
                LeaveAngry();
        }
    }

    void UpdatePatienceColor()
    {
        if (patienceSlider != null)
        {
            Image fillImage = patienceSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float patienceRatio = currentPatience / patience;
                if (patienceRatio > 0.5f)
                {
                    float t = (patienceRatio - 0.5f) * 2f;
                    fillImage.color = Color.Lerp(Color.yellow, maxPatienceColor, t);
                }
                else
                {
                    float t = patienceRatio * 2f;
                    fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
                }
            }
        }
    }

    public void TryServeCoffee(Cup cup)
    {
        if (isServed || !isWaiting || isLeaving) return;

        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCoffee == null)
        {
            Debug.Log("无法获取咖啡数据");
            return;
        }
        Coffee coffeeData = coffeeMachine.currentCoffee;

        if (!cup.hasCoffee && !cup.hasFig) return;

        int matchedIndex = -1;
        for (int i = 0; i < orders.Count; i++)
        {
            if (!ordersCompleted[i] && coffeeData.type == orders[i])
            {
                matchedIndex = i;
                break;
            }
        }

        if (matchedIndex >= 0)
        {
            // 正确交付
            if (orders[matchedIndex] == Coffee.CoffeeType.FigOnly)
            {
                float restore = patience * 0.3f;
                currentPatience = Mathf.Min(patience, currentPatience + restore);
                EventManager.Instance.TriggerGameLog($"无花果茶恢复了耐心 +{restore:F1}");
            }

            ordersCompleted[matchedIndex] = true;
            completedCount++;
            UpdateRemainingText();
            EventManager.Instance.TriggerGameLog($"交付了一杯 {coffeeData.type}，剩余 {remainingOrders} 杯");
            UpdateOrderIcon();
            cup.OnServed();

            if (completedCount == totalOrders)
            {
                int totalReward = CalculateTotalReward();
                CompleteAllOrders(totalReward);
            }
            else
            {
                coffeeMachine.ResetCurrentCoffee();
            }
        }
        else
        {
            OrderIncorrect(cup);
        }
    }

    private int CalculateTotalReward()
    {
        int total = 0;
        foreach (var type in orders)
        {
            int basePrice = DataManager.Instance.GetCoffeePrice(type);
            float multiplier = rewardMultiplier;
            if (UpgradeManager.Instance != null) multiplier *= UpgradeManager.Instance.tipMultiplier;
            total += Mathf.RoundToInt(basePrice * multiplier);
        }
        return total;
    }

    private void CompleteAllOrders(int totalReward)
    {
        isWaiting = false;
        isServed = true;
        isLeaving = true;

        // 隐藏所有 UI
        if (orderIcon != null) orderIcon.gameObject.SetActive(false);
        if (patienceSlider != null) patienceSlider.gameObject.SetActive(false);
        if (remainingOrdersText != null) remainingOrdersText.gameObject.SetActive(false);

        // 保持开心表情（或切换为开心，已经是开心则不变）
        UpdateCustomerSprite(true);
        CoffeeOrderManager.Instance.CompleteOrder(this, totalReward);

        // 播放开心音效
        if (happySound != null)
        {
            AudioSource.PlayClipAtPoint(happySound, transform.position);
        }
    }

    void OrderIncorrect(Cup cup)
    {
        // 显示生气表情
        UpdateCustomerSprite(false);

        // 减少耐心
        currentPatience -= 10f;
        if (patienceSlider != null)
            patienceSlider.value = currentPatience;

        EventManager.Instance.TriggerGameLog("订单错误！顾客不满意！", LogType.Warning);
        EventManager.Instance.TriggerOrderIncorrect(this);

        ResetCupForReuse(cup);
        cup.ReturnToCoffeeMachine();

        if (currentPatience <= 0)
        {
            LeaveAngry();
        }

        // 播放生气音效
        if (angrySound != null)
        {
            AudioSource.PlayClipAtPoint(angrySound, transform.position);
        }
    }

    private void ResetCupForReuse(Cup cup)
    {
        if (cup == null) return;
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine != null)
            coffeeMachine.ResetCurrentCoffee();
    }

    void LeaveAngry()
    {
        if (isLeaving) return;
        isLeaving = true;

        // 生气表情
        UpdateCustomerSprite(false);


        EventManager.Instance.TriggerGameLog("顾客生气地离开了...");
        EventManager.Instance.TriggerCustomerLeftAngry(this);

        CoffeeOrderManager.Instance.CustomerLeftAngry(this);
        // 播放生气音效
        if (angrySound != null)
        {
            AudioSource.PlayClipAtPoint(angrySound, transform.position);
        }
    }

    void Leave()
    {
        isWaiting = false;
        StartCoroutine(LeaveAnimation());
    }

    IEnumerator LeaveAnimation()
    {
        float duration = 1f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position += Vector3.right * Time.deltaTime * 2f;
            Color color = originalColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;
            yield return null;
        }
    }
}