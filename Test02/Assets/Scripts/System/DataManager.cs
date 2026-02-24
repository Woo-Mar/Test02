// DataManager.cs - 统一管理游戏数据
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("咖啡配方配置")]
    public List<CoffeeRecipe> coffeeRecipes = new List<CoffeeRecipe>();

    [Header("原料配置")]
    public List<IngredientData> ingredientData = new List<IngredientData>();

    [Header("顾客配置")]
    public List<CustomerTypeData> customerTypes = new List<CustomerTypeData>();

    [Header("价格配置")]
    public CoffeePrices coffeePrices = new CoffeePrices();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化默认数据
            InitializeDefaultData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultData()
    {
        // 咖啡配方
        coffeeRecipes = new List<CoffeeRecipe>
        {
            new CoffeeRecipe
            {
                type = Coffee.CoffeeType.HotCoffee,
                requiredIngredients = new List<string> { "coffee" },
                forbiddenIngredients = new List<string> { "ice", "milk", "strawberry", "carambola", "fig" },
                value = 10
            },
            new CoffeeRecipe
            {
                type = Coffee.CoffeeType.IcedCoffee,
                requiredIngredients = new List<string> { "coffee", "ice" },
                forbiddenIngredients = new List<string> { "milk", "strawberry", "carambola", "fig" },
                value = 15
            },
            // 其他配方...
        };

        // 原料数据
        ingredientData = new List<IngredientData>
        {
            new IngredientData { id = "coffee", name = "咖啡", basePrice = 5, unit = "g" },
            new IngredientData { id = "milk", name = "牛奶", basePrice = 3, unit = "ml" },
            new IngredientData { id = "strawberry", name = "草莓", basePrice = 8, unit = "g" },
            new IngredientData { id = "carambola", name = "杨桃", basePrice = 10, unit = "片" },
            new IngredientData { id = "fig", name = "无花果", basePrice = 12, unit = "个" },
            new IngredientData { id = "ice", name = "冰块", basePrice = 1, unit = "块" }
        };

        // 价格配置
        coffeePrices = new CoffeePrices
        {
            hotCoffee = 10,
            icedCoffee = 15,
            latte = 15,
            strawberryLatte = 25,
            carambolaAmericano = 20,
            figTea = 5
        };
    }

    public CoffeeRecipe GetRecipe(Coffee.CoffeeType type)
    {
        return coffeeRecipes.Find(r => r.type == type);
    }

    public IngredientData GetIngredientData(string ingredientId)
    {
        return ingredientData.Find(i => i.id == ingredientId);
    }

    public int GetCoffeePrice(Coffee.CoffeeType type)
    {
        int basePrice = 10;
        switch (type)
        {
            case Coffee.CoffeeType.HotCoffee: basePrice = coffeePrices.hotCoffee;break;
            case Coffee.CoffeeType.IcedCoffee: basePrice = coffeePrices.icedCoffee; break;
            case Coffee.CoffeeType.Latte: basePrice = coffeePrices.latte; break;
            case Coffee.CoffeeType.StrawberryLatte: basePrice = coffeePrices.strawberryLatte; break;
            case Coffee.CoffeeType.CarambolaAmericano: basePrice = coffeePrices.carambolaAmericano; break;
            case Coffee.CoffeeType.FigOnly: basePrice = coffeePrices.figTea; break;
        }
        if (UpgradeManager.Instance != null)
            return Mathf.RoundToInt(basePrice * UpgradeManager.Instance.priceMultiplier);
        return basePrice;
    }
}

[System.Serializable]
public class CoffeeRecipe
{
    public Coffee.CoffeeType type;
    public List<string> requiredIngredients;  // 必须的原料
    public List<string> forbiddenIngredients; // 禁止的原料
    public int value;  // 基础价值
    public float preparationTime = 2f;  // 准备时间
}

[System.Serializable]
public class IngredientData
{
    public string id;
    public string name;
    public int basePrice;
    public string unit;
    public Sprite icon;
}

[System.Serializable]
public class CustomerTypeData
{
    public string id;
    public string name;
    public float basePatience = 30f;
    public int baseReward = 10;
    public Sprite sprite;
}

[System.Serializable]
public class CoffeePrices
{
    public int hotCoffee = 10;
    public int icedCoffee = 15;
    public int latte = 15;
    public int strawberryLatte = 25;
    public int carambolaAmericano = 20;
    public int figTea = 5;
}