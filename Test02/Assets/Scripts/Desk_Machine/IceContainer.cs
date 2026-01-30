// IceContainer.cs - 修正版本
using UnityEngine;

/// <summary>
/// 冰块容器控制器 - 处理加冰功能
/// 点击容器给附近的咖啡杯加冰
/// </summary>
public class IceContainer : MonoBehaviour
{
    [Header("冰块设置")]
    public GameObject iceCubePrefab;    // 冰块特效预制体
    public Transform iceSpawnPoint;     // 冰块生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.cyan;    // 高亮颜色
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
            Debug.Log("IceContainer 已订阅库存变化事件");
        }
    }

    void OnDestroy()
    {
        // 取消订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
            Debug.Log("IceContainer 已取消订阅库存变化事件");
        }
    }

    /// <summary>
    /// 鼠标点击冰块容器事件
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log("尝试添加冰块...");

        // 检查冰块库存
        if (!IngredientSystem.Instance.HasEnoughIngredient("ice", 3)) // 每杯冰咖啡消耗3块冰
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("冰块库存不足！", LogType.Warning);
            }
            return;
        }

        // 获取场景中所有杯子
        Cup[] cups = FindObjectsOfType<Cup>();

        // 遍历检查每个杯子
        foreach (Cup cup in cups)
        {
            // 检查条件：有咖啡、没加冰、在容器附近
            if (cup.hasCoffee && !cup.hasIce &&
                Vector2.Distance(cup.transform.position, transform.position) < 500f) // 距离判断
            {
                // 给杯子加冰
                cup.AddIce();

                // 获取咖啡数据并添加冰块原料
                CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
                if (coffeeMachine != null && coffeeMachine.currentCoffee != null)
                {
                    coffeeMachine.currentCoffee.AddIngredient("ice");

                    // 触发事件 - IngredientSystem会监听这个事件并消耗库存
                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerIngredientAdded("ice", coffeeMachine.currentCoffee, cup);
                    }
                }

                // 生成冰块视觉效果
                GameObject ice = Instantiate(iceCubePrefab, iceSpawnPoint.position, Quaternion.identity);

                // 添加自动销毁组件（1秒后消失）
                AutoDestroy autoDestroy = ice.AddComponent<AutoDestroy>();
                autoDestroy.destroyDelay = 1f;
                autoDestroy.fadeOut = true;
                autoDestroy.fadeDuration = 0.5f;

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerGameLog("冰块已加入咖啡");
                }

                // 更新容器外观
                UpdateContainerVisual();

                break; // 只处理一个杯子
            }
        }
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
    /// 根据库存状态更新容器外观
    /// </summary>
    void UpdateContainerVisual()
    {
        if (containerRenderer == null) return;

        IngredientSystem.Ingredient ice = IngredientSystem.Instance.GetIngredient("ice");
        if (ice == null) return;

        float ratio = (float)ice.currentAmount / ice.maxAmount;

        if (ice.currentAmount <= 0)
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
        if (ingredientId == "ice")
        {
            UpdateContainerVisual();
        }
    }
}