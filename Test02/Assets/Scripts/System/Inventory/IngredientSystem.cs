// IngredientSystem.cs - 修改后的版本
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 原料系统管理器 - 管理所有原料的库存
/// </summary>
public class IngredientSystem : MonoBehaviour
{
    public static IngredientSystem Instance { get; private set; }

    [System.Serializable]
    public class Ingredient
    {
        public string id;           // 原料ID
        public string name;         // 原料名称
        public int currentAmount;   // 当前库存量
        public int maxAmount = 100; // 最大库存量
        public Sprite icon;         // 图标
        public string unit;         // 单位

        public void Add(int amount)
        {
            currentAmount = Mathf.Clamp(currentAmount + amount, 0, maxAmount);
        }

        public bool Use(int amount)
        {
            if (currentAmount >= amount)
            {
                currentAmount -= amount;
                return true;
            }
            return false;
        }
    }

    [Header("原料库存")]
    public Ingredient coffeeBeans = new Ingredient { id = "coffee", name = "咖啡豆", currentAmount = 100, unit = "g" };
    public Ingredient milk = new Ingredient { id = "milk", name = "牛奶", currentAmount = 50, unit = "ml" };
    public Ingredient strawberry = new Ingredient { id = "strawberry", name = "草莓酱", currentAmount = 30, unit = "g" };
    public Ingredient carambola = new Ingredient { id = "carambola", name = "杨桃片", currentAmount = 20, unit = "片" };
    public Ingredient fig = new Ingredient { id = "fig", name = "无花果干", currentAmount = 15, unit = "个" };
    public Ingredient ice = new Ingredient { id = "ice", name = "冰块", currentAmount = 200, unit = "块" };
    public Ingredient cup = new Ingredient { id = "cup", name = "杯子", currentAmount = 10, unit = "个" };

    [Header("消耗配置")]
    public int coffeeBeansPerCup = 10;      // 每杯咖啡消耗的咖啡豆
    public int milkPerLatte = 5;           // 每杯拿铁消耗的牛奶
    public int strawberryPerLatte = 3;     // 每杯草莓拿铁消耗的草莓酱
    public int carambolaPerAmericano = 1;  // 每杯杨桃美式消耗的杨桃片
    public int figPerTea = 1;              // 每份无花果茶消耗的无花果干
    public int icePerIcedCoffee = 3;       // 每杯冰咖啡消耗的冰块

    private Dictionary<string, Ingredient> ingredientDict;
    private bool isSubscribed = false; // 添加订阅标记

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIngredients();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 延迟订阅事件，确保EventManager已初始化
        Invoke("DelayedSubscribe", 0.5f);
    }

    void DelayedSubscribe()
    {
        // 只订阅必要的事件，咖啡豆消耗事件
        if (EventManager.Instance != null && !isSubscribed)
        {
            EventManager.Instance.OnCoffeeGrinded += OnCoffeeGrinded;
            isSubscribed = true;
            Debug.Log("IngredientSystem 已订阅咖啡研磨事件");
        }
    }

    void OnDestroy()
    {
        if (EventManager.Instance != null && isSubscribed)
        {
            EventManager.Instance.OnCoffeeGrinded -= OnCoffeeGrinded;
            isSubscribed = false;
            Debug.Log("IngredientSystem 已取消订阅事件");
        }
    }

    void InitializeIngredients()
    {
        ingredientDict = new Dictionary<string, Ingredient>
        {
            { coffeeBeans.id, coffeeBeans },
            { milk.id, milk },
            { strawberry.id, strawberry },
            { carambola.id, carambola },
            { fig.id, fig },
            { ice.id, ice },
            { cup.id, cup }
        };
    }

    /// <summary>
    /// 咖啡研磨时消耗咖啡豆（仅此一处消耗咖啡豆）
    /// </summary>
    void OnCoffeeGrinded(string coffeeType)
    {
        Debug.Log($"研磨咖啡消耗咖啡豆，配置值: {coffeeBeansPerCup}");

        if (coffeeBeans.Use(coffeeBeansPerCup))
        {
            EventManager.Instance.TriggerGameLog($"消耗{coffeeBeansPerCup}{coffeeBeans.unit}咖啡豆");
            OnInventoryChanged?.Invoke(coffeeBeans.id, coffeeBeans.currentAmount);
        }
        else
        {
            EventManager.Instance.TriggerGameLog($"咖啡豆不足！", LogType.Warning);
        }
    }

    /// <summary>
    /// 安全消耗原料方法（由容器直接调用）
    /// </summary>
    public bool ConsumeIngredient(string ingredientId, int amount, string source = "未知来源")
    {
        if (!ingredientDict.TryGetValue(ingredientId, out Ingredient ingredient))
        {
            Debug.LogError($"未知原料ID: {ingredientId}");
            return false;
        }

        if (ingredient.currentAmount < amount)
        {
            Debug.Log($"{source}: {ingredient.name} 库存不足，需要{amount}{ingredient.unit}，当前{ingredient.currentAmount}{ingredient.unit}");
            return false;
        }

        if (ingredient.Use(amount))
        {
            Debug.Log($"{source}: 成功消耗{amount}{ingredient.unit}{ingredient.name}");
            EventManager.Instance.TriggerGameLog($"消耗{amount}{ingredient.unit}{ingredient.name}");
            OnInventoryChanged?.Invoke(ingredientId, ingredient.currentAmount);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 增加原料库存
    /// </summary>
    public void AddIngredient(string ingredientId, int amount)
    {
        if (ingredientDict.TryGetValue(ingredientId, out Ingredient ingredient))
        {
            ingredient.Add(amount);
            EventManager.Instance.TriggerGameLog($"增加{ingredient.name} {amount}{ingredient.unit}");

            // 触发UI更新事件
            OnInventoryChanged?.Invoke(ingredientId, ingredient.currentAmount);
        }
    }

    /// <summary>
    /// 检查是否有足够的原料
    /// </summary>
    public bool HasEnoughIngredient(string ingredientId, int amount)
    {
        if (ingredientDict.TryGetValue(ingredientId, out Ingredient ingredient))
        {
            return ingredient.currentAmount >= amount;
        }
        return false;
    }

    /// <summary>
    /// 使用原料（兼容旧代码）
    /// </summary>
    public bool UseIngredient(string ingredientId, int amount)
    {
        return ConsumeIngredient(ingredientId, amount, "直接调用");
    }

    /// <summary>
    /// 获取原料信息
    /// </summary>
    public Ingredient GetIngredient(string ingredientId)
    {
        ingredientDict.TryGetValue(ingredientId, out Ingredient ingredient);
        return ingredient;
    }

    /// <summary>
    /// 获取所有原料
    /// </summary>
    public List<Ingredient> GetAllIngredients()
    {
        return new List<Ingredient>
        {
            coffeeBeans, milk, strawberry, carambola, fig, ice, cup
        };
    }

    /// <summary>
    /// 获取原料消耗量配置
    /// </summary>
    public int GetConsumptionAmount(string ingredientId)
    {
        switch (ingredientId)
        {
            case "coffee": return coffeeBeansPerCup;
            case "milk": return milkPerLatte;
            case "strawberry": return strawberryPerLatte;
            case "carambola": return carambolaPerAmericano;
            case "fig": return figPerTea;
            case "ice": return icePerIcedCoffee;
            case "cup": return 1;
            default: return 1;
        }
    }

    /// <summary>
    /// 库存变化事件
    /// </summary>
    public event Action<string, int> OnInventoryChanged;
}