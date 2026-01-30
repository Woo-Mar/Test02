// CupContainer.cs - 修改为完全使用库存系统
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 杯子容器管理器 - 完全使用库存系统管理杯子
/// 点击容器可以从库存系统中获取新杯子
/// </summary>
public class CupContainer : MonoBehaviour
{
    [Header("杯子容器设置")]
    public CoffeeMachine coffeeMachine;       // 咖啡机引用
    public GameObject cupPrefab;              // 杯子预制体

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;  // 容器精灵渲染器
    public Sprite emptyContainerSprite;       // 空容器精灵
    public Sprite fullContainerSprite;        // 满容器精灵
    public Color highlightColor = Color.yellow;   // 高亮颜色
    public Color lowStockColor = Color.yellow;   // 低库存颜色
    public Color outOfStockColor = Color.red;    // 缺货颜色

    private Color originalColor;
    private bool isInitialized = false;

    void Start()
    {
        // 自动查找咖啡机引用
        if (coffeeMachine == null)
        {
            coffeeMachine = FindObjectOfType<CoffeeMachine>();
        }

        if (containerRenderer != null)
        {
            originalColor = containerRenderer.color;
        }

        // 延迟初始化，确保IngredientSystem已加载
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        // 订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged += OnInventoryChanged;
            Debug.Log("CupContainer 已订阅库存变化事件");
            UpdateVisuals(); // 初始更新容器外观
            isInitialized = true;
        }
        else
        {
            Debug.LogError("IngredientSystem 未初始化！");
            Invoke("Initialize", 0.5f); // 重试
        }
    }

    void OnDestroy()
    {
        // 取消订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
            Debug.Log("CupContainer 已取消订阅库存变化事件");
        }
    }

    /// <summary>
    /// 鼠标点击容器事件
    /// </summary>
    void OnMouseDown()
    {
        TrySpawnCup(); // 尝试生成杯子
    }

    /// <summary>
    /// 尝试从库存系统中获取杯子（公共方法，咖啡机也可调用）
    /// </summary>
    public void TrySpawnCup()
    {
        if (!isInitialized) return;

        Debug.Log("尝试从库存获取杯子...");

        // 检查杯子库存
        if (!IngredientSystem.Instance.HasEnoughIngredient("cup", 1))
        {
            Debug.Log("杯子库存不足！");
            EventManager.Instance.TriggerGameLog("杯子库存不足！", LogType.Warning);
            return;
        }

        // 检查咖啡机是否已有杯子
        if (coffeeMachine.currentCup != null)
        {
            Debug.Log("咖啡机上已有杯子，请先使用当前杯子！");

            // 如果咖啡机上的杯子是空的，自动清空它
            Cup cupScript = coffeeMachine.currentCup.GetComponent<Cup>();
            if (cupScript != null && cupScript.isEmpty)
            {
                Debug.Log("检测到咖啡机上的杯子是空的，正在清空...");
                coffeeMachine.ClearCurrentCup();

                // 清空后重新尝试生成
                TrySpawnCup();
            }
            return;
        }

        // 使用杯子库存
        if (IngredientSystem.Instance.UseIngredient("cup", 1))
        {
            SpawnCupOnMachine(); // 在咖啡机上生成杯子

            Debug.Log($"生成杯子成功，杯子库存: {IngredientSystem.Instance.GetIngredient("cup").currentAmount}");
        }
    }

    /// <summary>
    /// 在咖啡机上生成杯子
    /// </summary>
    private void SpawnCupOnMachine()
    {
        if (cupPrefab == null || coffeeMachine == null)
        {
            Debug.LogError("杯子预制体或咖啡机引用为空！");
            return;
        }

        Debug.Log($"咖啡机cupPosition位置: {coffeeMachine.cupPosition.position}");

        // 实例化新杯子
        GameObject newCup = Instantiate(cupPrefab);

        // 确保初始Z轴正确
        Vector3 cupPos = newCup.transform.position;
        cupPos.z = -2f; // 设置初始Z轴
        newCup.transform.position = cupPos;

        // 获取杯子脚本组件
        Cup cupScript = newCup.GetComponent<Cup>();
        if (cupScript == null)
        {
            Debug.LogError("杯子预制体没有Cup组件！");
            Destroy(newCup);
            return;
        }

        // 设置杯子初始状态
        cupScript.isEmpty = true;
        cupScript.isDraggable = false;

        // 调用咖啡机的放置杯子方法
        coffeeMachine.PlaceCup(newCup);
    }

    /// <summary>
    /// 更新容器外观（根据库存数量）
    /// </summary>
    void UpdateVisuals()
    {
        if (containerRenderer == null || !isInitialized) return;

        IngredientSystem.Ingredient cup = IngredientSystem.Instance.GetIngredient("cup");
        if (cup == null)
        {
            Debug.LogError("无法获取杯子库存信息！");
            return;
        }

        // 根据杯子数量切换精灵
        if (cup.currentAmount <= 0)
        {
            containerRenderer.sprite = emptyContainerSprite;
        }
        else
        {
            containerRenderer.sprite = fullContainerSprite;
        }

        // 根据库存状态设置颜色
        float ratio = (float)cup.currentAmount / cup.maxAmount;

        if (cup.currentAmount <= 0)
        {
            // 缺货状态 - 红色
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

        Debug.Log($"杯子容器外观更新: 库存{cup.currentAmount}, 最大{cup.maxAmount}, 比例{ratio:F2}");
    }

    /// <summary>
    /// 当原料库存变化时更新外观
    /// </summary>
    public void OnInventoryChanged(string ingredientId, int newAmount)
    {
        if (ingredientId == "cup")
        {
            Debug.Log($"杯子库存变化: {newAmount}");
            UpdateVisuals();
        }
    }

    /// <summary>
    /// 检查是否有杯子库存
    /// </summary>
    public bool HasCups()
    {
        if (!isInitialized) return false;

        IngredientSystem.Ingredient cup = IngredientSystem.Instance.GetIngredient("cup");
        return cup != null && cup.currentAmount > 0;
    }

    /// <summary>
    /// 获取剩余杯子数量（从库存系统获取）
    /// </summary>
    public int GetRemainingCups()
    {
        if (!isInitialized) return 0;

        IngredientSystem.Ingredient cup = IngredientSystem.Instance.GetIngredient("cup");
        return cup != null ? cup.currentAmount : 0;
    }

    /// <summary>
    /// 鼠标悬停效果
    /// </summary>
    void OnMouseEnter()
    {
        if (containerRenderer != null)
        {
            containerRenderer.color = highlightColor;
        }
    }

    /// <summary>
    /// 鼠标离开效果
    /// </summary>
    void OnMouseExit()
    {
        UpdateVisuals(); // 恢复库存状态颜色
    }

    /// <summary>
    /// 补充杯子库存（从采购系统调用）
    /// </summary>
    /// <param name="amount">补充数量</param>
    public void RefillCups(int amount)
    {
        if (!isInitialized) return;

        IngredientSystem.Instance.AddIngredient("cup", amount);
        Debug.Log($"补充杯子库存: {amount}个");
    }

    /// <summary>
    /// 设置杯子最大库存（从采购系统调用）
    /// </summary>
    /// <param name="maxAmount">最大库存量</param>
    public void SetMaxCups(int maxAmount)
    {
        if (!isInitialized) return;

        IngredientSystem.Ingredient cup = IngredientSystem.Instance.GetIngredient("cup");
        if (cup != null)
        {
            cup.maxAmount = maxAmount;
            Debug.Log($"设置杯子最大库存: {maxAmount}个");
        }
    }
}