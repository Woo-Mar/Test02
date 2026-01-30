// StrawberryContainer.cs - 修改版
using UnityEngine;

/// <summary>
/// 草莓酱容器控制器 - 处理添加草莓酱功能
/// </summary>
public class StrawberryContainer : MonoBehaviour
{
    [Header("草莓酱设置")]
    //public GameObject strawberryEffectPrefab;    // 草莓酱特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.red;     // 高亮颜色
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
            Debug.Log("StrawberryContainer 已订阅库存变化事件");
        }
    }

    void OnDestroy()
    {
        // 取消订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
            Debug.Log("StrawberryContainer 已取消订阅库存变化事件");
        }
    }

    void OnMouseDown()
    {
        AddStrawberryToCup();
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
    /// 给杯子添加草莓酱
    /// </summary>
    void AddStrawberryToCup()
    {
        Debug.Log("尝试添加草莓酱...");

        // 获取草莓酱消耗量配置
        int amountToConsume = IngredientSystem.Instance.GetConsumptionAmount("strawberry"); // 3g 每杯

        // 检查草莓酱库存
        if (!IngredientSystem.Instance.HasEnoughIngredient("strawberry", amountToConsume))
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("草莓酱库存不足！", LogType.Warning);
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
                EventManager.Instance.TriggerGameLog("杯子没有咖啡，无法添加草莓酱", LogType.Warning);
            }
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasStrawberry)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过草莓酱了", LogType.Warning);
            }
            return;
        }

        // 安全消耗原料
        if (IngredientSystem.Instance.ConsumeIngredient("strawberry", amountToConsume, "StrawberryContainer"))
        {
            // 添加草莓酱原料到咖啡数据
            coffeeData.AddIngredient("strawberry");

            // 添加草莓酱到杯子
            cup.AddExtraIngredient("strawberry");

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog($"已添加草莓酱！当前咖啡类型：{coffeeData.type}");
            }

            // 更新容器外观
            UpdateContainerVisual();
        }
        else
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("草莓酱库存不足！", LogType.Warning);
            }
        }
    }

    /// <summary>
    /// 根据库存状态更新容器外观
    /// </summary>
    void UpdateContainerVisual()
    {
        if (containerRenderer == null) return;

        IngredientSystem.Ingredient strawberry = IngredientSystem.Instance.GetIngredient("strawberry");
        if (strawberry == null) return;

        float ratio = (float)strawberry.currentAmount / strawberry.maxAmount;

        if (strawberry.currentAmount <= 0)
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
        if (ingredientId == "strawberry")
        {
            UpdateContainerVisual();
        }
    }
}