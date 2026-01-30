// CarambolaContainer.cs - 修改版
using UnityEngine;

/// <summary>
/// 杨桃容器控制器 - 处理添加杨桃片功能
/// </summary>
public class CarambolaContainer : MonoBehaviour
{
    [Header("杨桃设置")]
    //public GameObject carambolaEffectPrefab;    // 杨桃特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.yellow;  // 高亮颜色
    public Color lowStockColor = Color.yellow;   // 低库存颜色
    public Color outOfStockColor = Color.gray;   // 缺货颜色

    private Color originalColor;

    void Start()
    {
        if (containerRenderer != null)
        {
            originalColor = containerRenderer.color;
            UpdateContainerVisual(); // 初始更新容器外观
        }

        // 订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged += OnInventoryChanged;
            Debug.Log("CarambolaContainer 已订阅库存变化事件");
        }
    }

    void OnDestroy()
    {
        // 取消订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
            Debug.Log("CarambolaContainer 已取消订阅库存变化事件");
        }
    }

    void OnMouseDown()
    {
        AddCarambolaToCup();
    }

    void OnMouseEnter()
    {
        if (containerRenderer != null)
        {
            containerRenderer.color = highlightColor;
        }
    }

    void OnMouseExit()
    {
        if (containerRenderer != null)
        {
            UpdateContainerVisual(); // 恢复库存状态颜色
        }
    }

    /// <summary>
    /// 给杯子添加杨桃片
    /// </summary>
    void AddCarambolaToCup()
    {
        Debug.Log("尝试添加杨桃片...");

        // 获取杨桃片消耗量配置
        int amountToConsume = IngredientSystem.Instance.GetConsumptionAmount("carambola"); // 1片 每杯

        // 检查杨桃片库存
        if (!IngredientSystem.Instance.HasEnoughIngredient("carambola", amountToConsume))
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("杨桃片库存不足！", LogType.Warning);
            }
            return;
        }

        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCup == null)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("请先放置一个装有咖啡的杯子", LogType.Warning);
            }
            return;
        }

        Cup cup = coffeeMachine.currentCup.GetComponent<Cup>();
        if (cup == null || !cup.hasCoffee)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("杯子没有咖啡，无法添加杨桃片", LogType.Warning);
            }
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasCarambola)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过杨桃片了", LogType.Warning);
            }
            return;
        }

        // 安全消耗原料
        if (IngredientSystem.Instance.ConsumeIngredient("carambola", amountToConsume, "CarambolaContainer"))
        {
            // 添加杨桃片原料到咖啡数据
            coffeeData.AddIngredient("carambola");

            // 添加杨桃片到杯子
            cup.AddExtraIngredient("carambola");

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog($"已添加杨桃片！当前咖啡类型：{coffeeData.type}");
            }

            // 更新容器外观
            UpdateContainerVisual();
        }
        else
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("杨桃片库存不足！", LogType.Warning);
            }
        }
    }

    /// <summary>
    /// 根据库存状态更新容器外观
    /// </summary>
    void UpdateContainerVisual()
    {
        if (containerRenderer == null) return;

        IngredientSystem.Ingredient carambola = IngredientSystem.Instance.GetIngredient("carambola");
        if (carambola == null) return;

        float ratio = (float)carambola.currentAmount / carambola.maxAmount;

        if (carambola.currentAmount <= 0)
        {
            // 缺货状态 - 灰色
            containerRenderer.color = outOfStockColor;
        }
        else if (ratio < 0.3f)
        {
            // 低库存状态 - 黄色
            containerRenderer.color = lowStockColor;
        }
        else
        {
            // 正常库存 - 原始颜色
            containerRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// 当原料库存变化时更新外观
    /// </summary>
    public void OnInventoryChanged(string ingredientId, int newAmount)
    {
        if (ingredientId == "carambola")
        {
            UpdateContainerVisual();
        }
    }
}