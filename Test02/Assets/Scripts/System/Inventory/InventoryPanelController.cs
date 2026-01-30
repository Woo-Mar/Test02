// InventoryPanelController.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 原料面板UI控制器
/// </summary>
public class InventoryPanelController : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject inventoryPanel;           // 原料面板
    public Button openInventoryButton;          // 打开原料面板按钮
    public Button closeButton;                  // 关闭按钮

    [Header("原料显示模板")]
    public GameObject ingredientItemPrefab;     // 原料项预制体
    public Transform ingredientListContainer;   // 原料列表容器

    [Header("UI元素")]
    public TMP_Text titleText;                      // 标题文本
    public TMP_Text totalItemsText;                 // 总物品数文本

    private Dictionary<string, GameObject> ingredientUIItems = new Dictionary<string, GameObject>();
    private bool isInitialized = false;

    void Start()
    {
        Debug.Log("InventoryPanelController Start");

        // 初始化UI
        if (openInventoryButton != null)
        {
            openInventoryButton.onClick.AddListener(OpenInventoryPanel);
        }
        else
        {
            Debug.LogError("openInventoryButton 未设置！");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInventoryPanel);
        }
        else
        {
            Debug.LogError("closeButton 未设置！");
        }

        // 隐藏面板
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("inventoryPanel 未设置！");
        }

        // 检查预制体
        if (ingredientItemPrefab == null)
        {
            Debug.LogError("ingredientItemPrefab 未设置！");
        }

        // 检查容器
        if (ingredientListContainer == null)
        {
            Debug.LogError("ingredientListContainer 未设置！");
        }

        // 延迟初始化，确保IngredientSystem已初始化
        Invoke("DelayedInitialize", 0.5f);
    }

    /// <summary>
    /// 延迟初始化
    /// </summary>
    void DelayedInitialize()
    {
        Debug.Log("开始初始化原料面板...");

        // 初始创建UI项
        CreateIngredientUIItems();

        // 订阅库存变化事件
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged += OnInventoryChanged;
            Debug.Log("已订阅库存变化事件");
        }
        else
        {
            Debug.LogError("IngredientSystem.Instance 为 null！");
        }

        isInitialized = true;

    }

    void OnDestroy()
    {
        if (IngredientSystem.Instance != null)
        {
            IngredientSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    /// <summary>
    /// 打开原料面板
    /// </summary>
    public void OpenInventoryPanel()
    {
        Debug.Log("打开原料面板");

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            UpdateAllIngredientUI();

            // 暂停游戏（可选）
            Time.timeScale = 0f;

            EventManager.Instance.TriggerGameLog("打开原料面板");
        }
        else
        {
            Debug.LogError("无法打开面板：inventoryPanel 为 null");
        }
    }

    /// <summary>
    /// 关闭原料面板
    /// </summary>
    public void CloseInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);

            // 恢复游戏
            Time.timeScale = 1f;

            EventManager.Instance.TriggerGameLog("关闭原料面板");
        }
    }

    /// <summary>
    /// 创建所有原料的UI项
    /// </summary>
    void CreateIngredientUIItems()
    {
        Debug.Log("创建原料UI项...");

        if (ingredientItemPrefab == null || ingredientListContainer == null)
        {
            Debug.LogError("原料UI预制体或容器未设置！");
            return;
        }

        // 清空现有项
        foreach (Transform child in ingredientListContainer)
        {
            Destroy(child.gameObject);
        }
        ingredientUIItems.Clear();

        // 获取所有原料并创建UI项
        if (IngredientSystem.Instance == null)
        {
            Debug.LogError("IngredientSystem 未初始化！");
            return;
        }

        List<IngredientSystem.Ingredient> ingredients = IngredientSystem.Instance.GetAllIngredients();
        Debug.Log($"获取到 {ingredients.Count} 种原料");

        foreach (var ingredient in ingredients)
        {
            Debug.Log($"创建原料项: {ingredient.name} (ID: {ingredient.id})");

            GameObject itemGO = Instantiate(ingredientItemPrefab, ingredientListContainer);

            // 确保itemGO是激活的
            itemGO.SetActive(true);

            // 设置itemGO的位置和缩放
            itemGO.transform.localScale = Vector3.one;
            itemGO.transform.localPosition = Vector3.zero;

            // 更新UI
            UpdateIngredientUIItem(itemGO, ingredient);

            // 添加到字典
            ingredientUIItems[ingredient.id] = itemGO;

            Debug.Log($"原料 {ingredient.name} 的UI项已创建");
        }

        // 更新总物品数
        UpdateTotalItemsCount();


        Debug.Log($"原料UI项创建完成，共 {ingredientUIItems.Count} 项");
    }

    /// <summary>
    /// 更新单个原料的UI项
    /// </summary>
    void UpdateIngredientUIItem(GameObject itemGO, IngredientSystem.Ingredient ingredient)
    {
        if (itemGO == null || ingredient == null)
        {
            Debug.LogError("更新UI项时参数为空！");
            return;
        }

        // 获取IngredientItemUI组件
        IngredientItemUI itemUI = itemGO.GetComponent<IngredientItemUI>();
        if (itemUI != null)
        {
            Debug.Log($"使用IngredientItemUI组件更新: {ingredient.name}");
            itemUI.UpdateUI(ingredient);
        }
        else
        {
            Debug.LogWarning($"GameObject {itemGO.name} 上没有IngredientItemUI组件，尝试手动更新...");

            // 备用方法：手动查找UI组件
            Transform itemTransform = itemGO.transform;

            // 查找UI元素
            Image iconImage = itemTransform.Find("IconImage")?.GetComponent<Image>();
            Text nameText = itemTransform.Find("NameText")?.GetComponent<Text>();
            Text amountText = itemTransform.Find("AmountText")?.GetComponent<Text>();
            Text unitText = itemTransform.Find("UnitText")?.GetComponent<Text>();
            Text idText = itemTransform.Find("IDText")?.GetComponent<Text>();
            Slider progressSlider = itemTransform.Find("ProgressSlider")?.GetComponent<Slider>();

            // 调试信息
            Debug.Log($"查找UI元素结果: iconImage={iconImage != null}, nameText={nameText != null}, amountText={amountText != null}");

            // 更新UI
            if (nameText != null) nameText.text = ingredient.name;
            else Debug.LogWarning("未找到NameText");

            if (amountText != null) amountText.text = ingredient.currentAmount.ToString();
            else Debug.LogWarning("未找到AmountText");

            if (unitText != null) unitText.text = ingredient.unit;
            else Debug.LogWarning("未找到UnitText");

            if (idText != null) idText.text = ingredient.id;

            if (iconImage != null && ingredient.icon != null)
            {
                iconImage.sprite = ingredient.icon;
                iconImage.enabled = true;
            }

            if (progressSlider != null)
            {
                progressSlider.maxValue = ingredient.maxAmount;
                progressSlider.value = ingredient.currentAmount;
            }
        }
    }

    /// <summary>
    /// 库存变化时更新UI
    /// </summary>
    void OnInventoryChanged(string ingredientId, int newAmount)
    {
        Debug.Log($"库存变化: {ingredientId} -> {newAmount}");

        if (ingredientUIItems.TryGetValue(ingredientId, out GameObject itemGO))
        {
            IngredientSystem.Ingredient ingredient = IngredientSystem.Instance.GetIngredient(ingredientId);
            if (ingredient != null)
            {
                UpdateIngredientUIItem(itemGO, ingredient);
            }
        }

        // 更新总物品数
        UpdateTotalItemsCount();
    }

    /// <summary>
    /// 更新所有原料的UI
    /// </summary>
    void UpdateAllIngredientUI()
    {
        if (!isInitialized) return;

        List<IngredientSystem.Ingredient> ingredients = IngredientSystem.Instance.GetAllIngredients();

        foreach (var ingredient in ingredients)
        {
            if (ingredientUIItems.TryGetValue(ingredient.id, out GameObject itemGO))
            {
                UpdateIngredientUIItem(itemGO, ingredient);
            }
        }

        UpdateTotalItemsCount();
    }

    /// <summary>
    /// 更新总物品数
    /// </summary>
    void UpdateTotalItemsCount()
    {
        if (totalItemsText != null && IngredientSystem.Instance != null)
        {
            int total = 0;
            List<IngredientSystem.Ingredient> ingredients = IngredientSystem.Instance.GetAllIngredients();

            foreach (var ingredient in ingredients)
            {
                total += ingredient.currentAmount;
            }

            totalItemsText.text = $"总库存: {total} 件";
        }
    }

    /// <summary>
    /// 强制刷新UI（用于调试）
    /// </summary>
    public void ForceRefreshUI()
    {
        CreateIngredientUIItems();
    }
}