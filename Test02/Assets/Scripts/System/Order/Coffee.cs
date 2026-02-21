// Coffee.cs - 咖啡数据类
[System.Serializable]
public class Coffee
{
    // 基础原料标记
    public bool hasCoffeePowder = false;   // 咖啡粉
    public bool hasBrewedCoffee = false;   // 已萃取咖啡
    public bool hasIce = false;            // 冰块
    public bool hasMilk = false;           // 牛奶
    public bool hasStrawberry = false;     // 草莓酱
    public bool hasCarambola = false;      // 杨桃片
    public bool hasFig = false;            // 无花果干

    // 状态标记
    public bool isInCup = false;           // 在杯子中
    public bool isComplete = false;        // 完成制作
    public CoffeeType type = CoffeeType.HotCoffee; // 咖啡类型
    public int value = 10;                 // 咖啡价值（金币）

    // 制作计时
    public float preparationTime = 0f;     // 制作花费的时间
    public float requiredTime = 2f;        // 标准制作时间

    // 咖啡类型枚举
    public enum CoffeeType
    {
        HotCoffee,      // 热咖啡
        IcedCoffee,     // 冰咖啡
        Latte,          // 拿铁
        StrawberryLatte, // 草莓拿铁
        CarambolaAmericano, // 杨桃美式
        FigOnly         // 无花果干茶（非饮品）
    }

    /// <summary>
    /// 根据当前原料确定咖啡类型
    /// </summary>
    public CoffeeType DetermineCoffeeType()
    {
        // 检查无花果干（单独售卖）
        if (hasFig && !hasCoffeePowder && !hasBrewedCoffee)
        {
            return CoffeeType.FigOnly;
        }

        // 必须有咖啡才能是饮品（除了无花果干茶）
        if (!hasBrewedCoffee) return CoffeeType.HotCoffee;

        // 检查草莓拿铁：咖啡 + 牛奶 + 草莓
        if (hasStrawberry && hasMilk && !hasIce && !hasCarambola)
        {
            return CoffeeType.StrawberryLatte;
        }

        // 检查杨桃美式：咖啡 + 冰块 + 杨桃
        if (hasCarambola && hasIce && !hasMilk && !hasStrawberry)
        {
            return CoffeeType.CarambolaAmericano;
        }

        // 检查拿铁：咖啡 + 牛奶
        if (hasMilk && !hasIce && !hasStrawberry && !hasCarambola)
        {
            return CoffeeType.Latte;
        }

        // 检查冰咖啡：咖啡 + 冰块
        if (hasIce && !hasMilk && !hasStrawberry && !hasCarambola)
        {
            return CoffeeType.IcedCoffee;
        }

        // 默认热咖啡
        return CoffeeType.HotCoffee;
    }

    /// <summary>
    /// 计算咖啡价值
    /// </summary>
    public int CalculateValue()
    {
        switch (type)
        {
            case CoffeeType.HotCoffee:
                return 10;
            case CoffeeType.IcedCoffee:
                return 15;
            case CoffeeType.Latte:
                return 15;
            case CoffeeType.StrawberryLatte:
                return 25;
            case CoffeeType.CarambolaAmericano:
                return 20;
            case CoffeeType.FigOnly:
                return 5;
            default:
                return 10;
        }
    }

    /// <summary>
    /// 重置咖啡状态
    /// </summary>
    public void Reset()
    {
        hasCoffeePowder = false;
        hasBrewedCoffee = false;
        hasIce = false;
        hasMilk = false;
        hasStrawberry = false;
        hasCarambola = false;
        hasFig = false;
        isInCup = false;
        isComplete = false;
        type = CoffeeType.HotCoffee;
        preparationTime = 0f;
    }

    /// <summary>
    /// 添加原料
    /// </summary>
    public void AddIngredient(string ingredient)
    {
        switch (ingredient.ToLower())
        {
            case "strawberry":
                hasStrawberry = true;
                break;
            case "carambola":
                hasCarambola = true;
                break;
            case "fig":
                hasFig = true;
                break;
            case "milk":
                hasMilk = true;
                break;
            case "ice":
                hasIce = true;
                break;
        }

        // 添加原料后重新确定类型
        type = DetermineCoffeeType();
        value = CalculateValue();
    }
}