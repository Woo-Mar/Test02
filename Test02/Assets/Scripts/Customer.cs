// Customer.cs
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
    public int iceBonus = 5;            // 加冰额外奖励

    [Header("UI")]
    public Slider patienceSlider;       // 耐心条UI
    public Image orderIcon;             // 订单图标
    public Sprite coffeeOrderSprite;   // 热咖啡订单图标
    public Sprite icedCoffeeOrderSprite; // 冰咖啡订单图标

    [Header("Order Type")]
    public bool wantsIcedCoffee = false; // 顾客想要的咖啡类型

    private SpriteRenderer spriteRenderer; // 精灵渲染器
    private float currentPatience;        // 当前剩余耐心
    private bool isWaiting = true;       // 是否在等待状态
    private bool isServed = false;       // 是否已被服务

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentPatience = patience; // 初始化耐心值

        // 随机决定顾客想要什么咖啡
        wantsIcedCoffee = Random.value > 0.5f;

        // 设置顾客初始外观
        if (waitingSprite != null)
        {
            spriteRenderer.sprite = waitingSprite;
        }

        InitializeUI(); // 初始化UI显示
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
            // 随机决定咖啡类型
            wantsIcedCoffee = Random.value > 0.5f;
            // 根据订单类型设置图标
            orderIcon.sprite = wantsIcedCoffee ? icedCoffeeOrderSprite : coffeeOrderSprite;
            orderIcon.gameObject.SetActive(true); // 显示订单图标

            Debug.Log($"新顾客到达！想要{(wantsIcedCoffee ? "冰咖啡" : "热咖啡")}");
        }
    }

    void Update()
    {
        // 更新等待中的顾客状态
        if (isWaiting && !isServed)
        {
            currentPatience -= Time.deltaTime; // 减少耐心值

            // 更新耐心条UI
            if (patienceSlider != null)
            {
                patienceSlider.value = currentPatience;
            }

            UpdatePatienceColor(); // 更新耐心条颜色

            // 耐心耗尽处理
            if (currentPatience <= 0)
            {
                LeaveAngry(); // 生气离开
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

        Debug.Log($"尝试服务顾客，顾客想要{(wantsIcedCoffee ? "冰咖啡" : "热咖啡")}，杯子状态：咖啡={cup.hasCoffee}, 冰={cup.hasIce}");

        // 基本检查：杯子必须有咖啡
        if (!cup.hasCoffee)
        {
            Debug.Log("杯子是空的，没有咖啡！");
            return;
        }

        // 检查订单是否符合要求
        bool orderCorrect = true;

        if (wantsIcedCoffee && !cup.hasIce)
        {
            orderCorrect = false;
            Debug.Log("顾客想要冰咖啡，但杯子没加冰！");
        }
        else if (!wantsIcedCoffee && cup.hasIce)
        {
            orderCorrect = false;
            Debug.Log("顾客想要热咖啡，但杯子加了冰！");
        }

        // 根据订单正确性处理
        if (orderCorrect)
        {
            ServeCoffee(cup); // 正确服务
        }
        else
        {
            OrderIncorrect(cup); // 订单错误
        }
    }

    /// <summary>
    /// 正确服务咖啡的处理
    /// </summary>
    /// <param name="cup">被服务的杯子</param>
    void ServeCoffee(Cup cup)
    {
        isWaiting = false;
        isServed = true;

        // 更新顾客外观为开心状态
        if (happySprite != null)
        {
            spriteRenderer.sprite = happySprite;
        }

        // 隐藏UI元素
        if (orderIcon != null)
        {
            orderIcon.gameObject.SetActive(false);
        }

        if (patienceSlider != null)
        {
            patienceSlider.gameObject.SetActive(false);
        }

        Debug.Log("顾客收到正确的咖啡！");

        // 通知杯子被服务（触发动画）
        cup.OnServed();

        // 计算奖励金额
        int reward = baseReward;
        if (cup.hasIce && wantsIcedCoffee)
        {
            reward += iceBonus; // 加冰额外奖励
        }

        // 根据剩余耐心计算额外奖励
        float patienceBonus = currentPatience / patience;
        reward += Mathf.RoundToInt(reward * patienceBonus * 0.5f);

        // 创建咖啡数据对象用于奖励计算
        Coffee servedCoffee = new Coffee
        {
            hasCoffeePowder = true,
            hasBrewedCoffee = true,
            hasIce = cup.hasIce,
            isInCup = true,
            isComplete = true,
            value = reward
        };

        // 完成订单（发放金币）
        CoffeeOrderManager.Instance.CompleteOrder(servedCoffee, this);

        // 延迟后开心离开
        Invoke("LeaveHappy", 2f);
    }

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

        // 杯子返回咖啡机
        cup.ReturnToCoffeeMachine();

        // 检查耐心是否耗尽
        if (currentPatience <= 0)
        {
            LeaveAngry();
        }
    }

    /// <summary>
    /// 开心离开
    /// </summary>
    void LeaveHappy()
    {
        Debug.Log("顾客满意地离开了");
        Leave();
    }

    /// <summary>
    /// 生气离开
    /// </summary>
    void LeaveAngry()
    {
        if (!isServed)
        {
            Debug.Log("顾客生气地离开了...");

            // 播放生气离开音效（待实现）
            // AudioManager.Instance.PlaySound("customerLeaveAngry");
        }
        Leave();
    }

    /// <summary>
    /// 离开场景的通用处理
    /// </summary>
    void Leave()
    {
        // 从等待列表中移除
        if (CoffeeOrderManager.Instance.waitingCustomers.Contains(this))
        {
            CoffeeOrderManager.Instance.waitingCustomers.Remove(this);
        }

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

        Destroy(gameObject); // 销毁顾客对象
    }
}