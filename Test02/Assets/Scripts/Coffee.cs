// Coffee.cs - 咖啡数据类
[System.Serializable]
public class Coffee
{
    // 咖啡制作状态标记
    public bool hasCoffeePowder = false;  // 是否有咖啡粉
    public bool hasBrewedCoffee = false;  // 是否已萃取咖啡
    public bool hasIce = false;           // 是否加冰
    public bool isInCup = false;          // 是否在杯子中
    public bool isComplete = false;       // 是否完成制作
    public int value = 10;                // 咖啡价值（金币）

    /// <summary>
    /// 重置咖啡状态到初始值
    /// </summary>
    public void Reset()
    {
        hasCoffeePowder = false;
        hasBrewedCoffee = false;
        hasIce = false;
        isInCup = false;
        isComplete = false;
    }
}