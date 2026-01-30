// FigContainer.cs - 修正版本
using UnityEngine;

/// <summary>
/// 无花果干篮控制器 - 处理添加无花果干功能
/// </summary>
public class FigContainer : MonoBehaviour
{
    [Header("无花果设置")]
    //public GameObject figEffectPrefab;           // 无花果干特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.magenta; // 高亮颜色
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
            Debug.Log("FigContainer 已订阅库存变化事件");
        }
    }

    void OnDestroy()
    {
        // 取消订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
            Debug.Log("FigContainer 已取消订阅库存变化事件");
        }
    }

    void OnMouseDown()
    {
        AddFigToCup();
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
    /// 给杯子添加无花果干
    /// </summary>
    void AddFigToCup()
    {
        Debug.Log("尝试添加无花果干...");

        // 检查无花果干库存
        if (!IngredientSystem.Instance.HasEnoughIngredient("fig", 1)) // 每份无花果茶消耗1个无花果干
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("无花果干库存不足！", LogType.Warning);
            }
            return;
        }

        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCup == null)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("请先放置一个杯子", LogType.Warning);
            }
            return;
        }

        Cup cup = coffeeMachine.currentCup.GetComponent<Cup>();
        if (cup == null)
        {
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasFig)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过无花果干了", LogType.Warning);
            }
            return;
        }

        // 添加无花果原料到咖啡数据
        coffeeData.AddIngredient("fig");

        // 添加无花果原料到杯子
        cup.AddExtraIngredient("fig");

        // 触发事件 - IngredientSystem会监听这个事件并消耗库存
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerIngredientAdded("fig", coffeeData, cup);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog($"已添加无花果干！当前产品类型：{coffeeData.type}");
        }

        // 更新容器外观
        UpdateContainerVisual();
    }

    /// <summary>
    /// 根据库存状态更新容器外观
    /// </summary>
    void UpdateContainerVisual()
    {
        if (containerRenderer == null) return;

        IngredientSystem.Ingredient fig = IngredientSystem.Instance.GetIngredient("fig");
        if (fig == null) return;

        float ratio = (float)fig.currentAmount / fig.maxAmount;

        if (fig.currentAmount <= 0)
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
        if (ingredientId == "fig")
        {
            UpdateContainerVisual();
        }
    }
}