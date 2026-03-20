// InventoryListener.cs - 未来原料管理系统的监听器示例
using UnityEngine;

public class InventoryListener : MonoBehaviour
{
    [Header("原料库存")]
    public int coffeeBeans = 100;
    public int milk = 50;
    public int strawberry = 30;
    public int carambola = 20;
    public int fig = 15;
    public int ice = 200;

    void Start()
    {
        // 订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCoffeeGrinded += OnCoffeeGrinded;
            EventManager.Instance.OnIngredientAdded += OnIngredientAdded;
            EventManager.Instance.OnOrderCompleted += OnOrderCompleted;
            EventManager.Instance.OnGameLog += OnGameLog;
        }
    }

    void OnDestroy()
    {
        // 取消订阅
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCoffeeGrinded -= OnCoffeeGrinded;
            EventManager.Instance.OnIngredientAdded -= OnIngredientAdded;
            EventManager.Instance.OnOrderCompleted -= OnOrderCompleted;
            EventManager.Instance.OnGameLog -= OnGameLog;
        }
    }

    void OnCoffeeGrinded(string coffeeType)
    {
        // 消耗咖啡豆
        coffeeBeans -= 10;
        Debug.Log($"咖啡豆剩余: {coffeeBeans}g");

        if (coffeeBeans < 20)
        {
            EventManager.Instance.TriggerGameLog("咖啡豆库存不足，请及时补充！", LogType.Warning);
        }
    }

    void OnIngredientAdded(string ingredient, Coffee coffee, Cup cup)
    {
        // 消耗原料
        switch (ingredient.ToLower())
        {
            case "milk":
                milk -= 5;
                Debug.Log($"牛奶剩余: {milk}ml");
                break;
            case "strawberry":
                strawberry -= 3;
                Debug.Log($"草莓剩余: {strawberry}g");
                break;
            case "carambola":
                carambola -= 1;
                Debug.Log($"杨桃剩余: {carambola}片");
                break;
            case "fig":
                fig -= 1;
                Debug.Log($"无花果剩余: {fig}个");
                break;
        }
    }

    void OnOrderCompleted(Customer customer, int totalReward)
    {
        Debug.Log($"订单完成，总奖励: {totalReward}");
    }

    void OnGameLog(string message, LogType logType)
    {
        // 记录重要日志
        if (logType == LogType.Error || logType == LogType.Warning)
        {
            Debug.Log($"系统日志: [{logType}] {message}");
        }
    }

    // 未来原料管理系统的公共方法
    public bool HasEnoughIngredient(string ingredient, int amount)
    {
        switch (ingredient.ToLower())
        {
            case "coffee": return coffeeBeans >= amount;
            case "milk": return milk >= amount;
            case "strawberry": return strawberry >= amount;
            case "carambola": return carambola >= amount;
            case "fig": return fig >= amount;
            case "ice": return ice >= amount;
            default: return false;
        }
    }

    public void AddIngredient(string ingredient, int amount)
    {
        switch (ingredient.ToLower())
        {
            case "coffee": coffeeBeans += amount; break;
            case "milk": milk += amount; break;
            case "strawberry": strawberry += amount; break;
            case "carambola": carambola += amount; break;
            case "fig": fig += amount; break;
            case "ice": ice += amount; break;
        }
    }
}