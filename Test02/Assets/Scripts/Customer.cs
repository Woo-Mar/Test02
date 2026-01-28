// Customer.cs - 修改后的版本
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 顾客控制器 - 管理顾客行为、订单需求和耐心系统
/// 处理咖啡服务、满意度评价和离开动画
/// </summary>
public class Customer : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite waitingSprite;        // 等待状态精灵
    public Sprite happySprite;          // 满意状态精灵
    public Sprite angrySprite;          // 生气状态精灵

    [Header("Settings")]
    public float patience = 30f;        // 总耐心时间（秒）
    public int baseReward = 10;         // 基础奖励金币

    [Header("UI")]
    public Slider patienceSlider;       // 耐心条UI
    public Image orderIcon;             // 订单图标

    [Header("订单图标精灵")]
    public Sprite hotCoffeeOrderSprite;        // 热咖啡订单图标
    public Sprite icedCoffeeOrderSprite;       // 冰咖啡订单图标
    public Sprite latteOrderSprite;            // 拿铁订单图标
    public Sprite strawberryLatteOrderSprite;  // 草莓拿铁订单图标
    public Sprite carambolaAmericanoOrderSprite; // 杨桃美式订单图标
    public Sprite figTeaOrderSprite;           // 无花果干茶订单图标

    [Header("Order Type")]
    public Coffee.CoffeeType wantedCoffeeType = Coffee.CoffeeType.HotCoffee; // 顾客想要的咖啡类型

    private SpriteRenderer spriteRenderer; // 精灵渲染器
    private float currentPatience;        // 当前剩余耐心
    private bool isWaiting = true;       // 是否在等待状态
    private bool isServed = false;       // 是否已被服务

    [HideInInspector]
    public Transform spawnPoint;

    // 添加：离开状态标记，防止多次触发离开逻辑
    private bool isLeaving = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentPatience = patience; // 初始化耐心值

        // 随机生成顾客想要的咖啡类型（包括所有6种类型）
        wantedCoffeeType = GetRandomCoffeeType();

        // 设置顾客初始外观
        if (waitingSprite != null)
        {
            spriteRenderer.sprite = waitingSprite;
        }

        InitializeUI(); // 初始化UI显示
        // 记录生成点（调试用）
        if (spawnPoint != null)
        {
            Debug.Log($"顾客在 {spawnPoint.name} 生成");
        }
        else
        {
            Debug.LogWarning("顾客没有分配生成点！");
        }
    }

    /// <summary>
    /// 随机生成咖啡类型
    /// </summary>
    private Coffee.CoffeeType GetRandomCoffeeType()
    {
        // 6种咖啡类型的概率分布（可以根据需要调整）
        float randomValue = Random.value;

        if (randomValue < 0.2f)      // 20% 热咖啡
            return Coffee.CoffeeType.HotCoffee;
        else if (randomValue < 0.4f) // 20% 冰咖啡
            return Coffee.CoffeeType.IcedCoffee;
        else if (randomValue < 0.6f) // 20% 拿铁
            return Coffee.CoffeeType.Latte;
        else if (randomValue < 0.75f) // 15% 草莓拿铁
            return Coffee.CoffeeType.StrawberryLatte;
        else if (randomValue < 0.9f)  // 15% 杨桃美式
            return Coffee.CoffeeType.CarambolaAmericano;
        else                          // 10% 无花果干茶
            return Coffee.CoffeeType.FigOnly;
    }

    /// <summary>
    /// 初始化顾客UI显示
    /// </summary>
    void InitializeUI()
    {
        if (patienceSlider != null)
        {
            patienceSlider.maxValue = patience; // 设置耐心条最大值
            patienceSlider.value = currentPatience; // 设置当前值
        }

        if (orderIcon != null)
        {
            // 根据订单类型设置图标
            SetOrderIcon(wantedCoffeeType);
            orderIcon.gameObject.SetActive(true); // 显示订单图标

            Debug.Log($"新顾客到达！想要{wantedCoffeeType}");
        }
    }

    /// <summary>
    /// 根据咖啡类型设置订单图标
    /// </summary>
    private void SetOrderIcon(Coffee.CoffeeType coffeeType)
    {
        switch (coffeeType)
        {
            case Coffee.CoffeeType.HotCoffee:
                orderIcon.sprite = hotCoffeeOrderSprite;
                break;
            case Coffee.CoffeeType.IcedCoffee:
                orderIcon.sprite = icedCoffeeOrderSprite;
                break;
            case Coffee.CoffeeType.Latte:
                orderIcon.sprite = latteOrderSprite;
                break;
            case Coffee.CoffeeType.StrawberryLatte:
                orderIcon.sprite = strawberryLatteOrderSprite;
                break;
            case Coffee.CoffeeType.CarambolaAmericano:
                orderIcon.sprite = carambolaAmericanoOrderSprite;
                break;
            case Coffee.CoffeeType.FigOnly:
                orderIcon.sprite = figTeaOrderSprite;
                break;
            default:
                orderIcon.sprite = hotCoffeeOrderSprite;
                break;
        }
    }

    void Update()
    {
        if (isWaiting && !isServed && !isLeaving)
        {
            currentPatience -= Time.deltaTime;

            if (patienceSlider != null)
            {
                patienceSlider.value = currentPatience;
            }

            UpdatePatienceColor();

            // 耐心耗尽离开
            if (currentPatience <= 0)
            {
                LeaveAngry();
            }
        }
    }

    /// <summary>
    /// 根据耐心值更新耐心条颜色（绿色→黄色→红色）
    /// </summary>
    void UpdatePatienceColor()
    {
        if (patienceSlider != null)
        {
            Image fillImage = patienceSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float patienceRatio = currentPatience / patience; // 耐心比例

                if (patienceRatio > 0.5f)
                {
                    // 绿色到黄色渐变（高耐心）
                    float t = (patienceRatio - 0.5f) * 2f;
                    fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
                }
                else
                {
                    // 黄色到红色渐变（低耐心）
                    float t = patienceRatio * 2f;
                    fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
                }
            }
        }
    }

    /// <summary>
    /// 尝试服务咖啡（由杯子拖拽触发）
    /// </summary>
    /// <param name="cup">被服务的杯子</param>
    public void TryServeCoffee(Cup cup)
    {
        // 检查顾客状态
        if (isServed || !isWaiting) return;

        Debug.Log($"尝试服务顾客，顾客想要{wantedCoffeeType}");
        Debug.Log($"杯子状态 - hasCoffee: {cup.hasCoffee}, hasFig: {cup.hasFig}");

        // 检查杯子是否有饮品（包括无花果干茶）
        if (!cup.hasCoffee && !cup.hasFig)
        {
            Debug.Log("杯子是空的！");
            return;
        }

        // 获取咖啡数据
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCoffee == null)
        {
            Debug.Log("无法获取咖啡数据");
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;

        // 如果是无花果干茶，检查是否满足配方
        if (coffeeData.type == Coffee.CoffeeType.FigOnly)
        {
            ServeFigTea(cup, coffeeData);
            return;
        }

        // 检查咖啡订单
        bool orderCorrect = CheckCoffeeOrder(cup, coffeeData);

        if (orderCorrect)
        {
            ServeCoffee(cup, coffeeData);
        }
        else
        {
            OrderIncorrect(cup);
        }
    }

    /// <summary>
    /// 检查咖啡订单（扩展版本）
    /// </summary>
    private bool CheckCoffeeOrder(Cup cup, Coffee coffee)
    {
        // 首先检查是否有咖啡
        if (!coffee.hasBrewedCoffee) return false;

        // 检查顾客想要的咖啡类型是否与制作的一致
        if (coffee.type != wantedCoffeeType)
        {
            Debug.Log($"咖啡类型不匹配！顾客想要{wantedCoffeeType}，但制作的是{coffee.type}");
            return false;
        }

        // 根据咖啡类型检查原料
        switch (wantedCoffeeType)
        {
            case Coffee.CoffeeType.HotCoffee:
                // 热咖啡：只需要咖啡，不需要其他原料
                return !cup.hasIce && !cup.hasMilk && !cup.hasStrawberry && !cup.hasCarambola;

            case Coffee.CoffeeType.IcedCoffee:
                // 冰咖啡：需要咖啡和冰
                return cup.hasIce && !cup.hasMilk && !cup.hasStrawberry && !cup.hasCarambola;

            case Coffee.CoffeeType.Latte:
                // 拿铁：需要咖啡和牛奶
                return cup.hasMilk && !cup.hasIce && !cup.hasStrawberry && !cup.hasCarambola;

            case Coffee.CoffeeType.StrawberryLatte:
                // 草莓拿铁：需要咖啡、牛奶和草莓
                return cup.hasMilk && cup.hasStrawberry && !cup.hasIce && !cup.hasCarambola;

            case Coffee.CoffeeType.CarambolaAmericano:
                // 杨桃美式：需要咖啡、冰和杨桃
                return cup.hasIce && cup.hasCarambola && !cup.hasMilk && !cup.hasStrawberry;

            default:
                return false;
        }
    }

    /// <summary>
    /// 服务无花果干茶
    /// </summary>
    private void ServeFigTea(Cup cup, Coffee coffee)
    {
        if (!coffee.hasFig || coffee.hasBrewedCoffee || coffee.hasCoffeePowder)
        {
            Debug.Log("这不是纯无花果干茶！");
            OrderIncorrect(cup);
            return;
        }

        isWaiting = false;
        isServed = true;
        isLeaving = true;

        // 更新外观
        if (happySprite != null)
        {
            spriteRenderer.sprite = happySprite;
        }

        // 隐藏UI
        if (orderIcon != null)
        {
            orderIcon.gameObject.SetActive(false);
        }

        if (patienceSlider != null)
        {
            patienceSlider.gameObject.SetActive(false);
        }

        Debug.Log("顾客收到无花果干茶！");

        // 无花果干茶的特殊效果：恢复耐心
        float patienceRestore = patience * 0.3f; // 恢复30%的耐心
        currentPatience = Mathf.Min(patience, currentPatience + patienceRestore);
        Debug.Log($"无花果干茶恢复了{patienceRestore}点耐心");

        // 杯子被服务
        cup.OnServed();

        // 完成订单
        CoffeeOrderManager.Instance.CompleteOrder(coffee, this);
    }

    /// <summary>
    /// 正确服务咖啡的处理
    /// </summary>
    /// <param name="cup">被服务的杯子</param>
    /// <param name="coffee">咖啡数据</param>
    /// <summary>
    /// 正确服务咖啡的处理
    /// </summary>
    /// <param name="cup">被服务的杯子</param>
    /// <param name="coffee">咖啡数据</param>
    void ServeCoffee(Cup cup, Coffee coffee)
    {
        isWaiting = false;
        isServed = true;
        isLeaving = true; // 标记为正在离开

        // 更新外观
        if (happySprite != null)
        {
            spriteRenderer.sprite = happySprite;
        }

        // 隐藏UI
        if (orderIcon != null)
        {
            orderIcon.gameObject.SetActive(false);
        }

        if (patienceSlider != null)
        {
            patienceSlider.gameObject.SetActive(false);
        }

        Debug.Log($"顾客收到正确的{coffee.type}！");

        // 通知杯子被服务
        cup.OnServed();

        // 计算奖励
        int reward = coffee.value;

        // 额外奖励：根据剩余耐心
        float patienceBonus = currentPatience / patience;
        reward += Mathf.RoundToInt(reward * patienceBonus * 0.5f);

        // 更新咖啡价值（包含耐心奖励）
        coffee.value = reward;

        // 完成订单（这里会释放生成点）
        CoffeeOrderManager.Instance.CompleteOrder(coffee, this);

        // 通知咖啡机重置（重要！）
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine != null)
        {
            coffeeMachine.ResetCurrentCoffee();
        }
    }

    /// <summary>
    /// 订单错误的处理
    /// </summary>
    /// <param name="cup">错误的杯子</param>
    /// <summary>
    /// 订单错误的处理
    /// </summary>
    /// <param name="cup">错误的杯子</param>
    void OrderIncorrect(Cup cup)
    {
        // 更新为生气外观
        if (angrySprite != null)
        {
            spriteRenderer.sprite = angrySprite;
        }

        // 减少耐心作为惩罚
        currentPatience -= 10f;

        Debug.Log("订单错误！顾客不满意！");

        // 播放生气音效（待实现）
        // AudioManager.Instance.PlaySound("customerAngry");

        // 重置杯子状态，然后返回咖啡机
        ResetCupForReuse(cup);
        cup.ReturnToCoffeeMachine();

        // 检查耐心是否耗尽
        if (currentPatience <= 0)
        {
            LeaveAngry();
        }
    }

    /// <summary>
    /// 重置杯子以便重用
    /// </summary>
    private void ResetCupForReuse(Cup cup)
    {
        if (cup == null) return;

        // 通知咖啡机重置咖啡数据
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine != null)
        {
            coffeeMachine.ResetCurrentCoffee();
        }
    }

    // 修改：生气离开
    void LeaveAngry()
    {
        if (isLeaving) return; // 防止重复调用

        isLeaving = true;

        if (!isServed)
        {
            Debug.Log("顾客生气地离开了...");
        }

        // 通知订单管理器顾客生气离开
        CoffeeOrderManager.Instance.CustomerLeftAngry(this);

        Leave();
    }

    // 修改：离开通用处理
    void Leave()
    {
        isWaiting = false;

        // 播放离开动画
        StartCoroutine(LeaveAnimation());
    }

    /// <summary>
    /// 离开动画协程（向右移动并淡出）
    /// </summary>
    IEnumerator LeaveAnimation()
    {
        float duration = 1f; // 动画持续时间
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color; // 原始颜色

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; // 动画进度

            // 向右移动
            transform.position += Vector3.right * Time.deltaTime * 2f;

            // 淡出效果
            Color color = originalColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;

            yield return null; // 等待下一帧
        }
    }
}