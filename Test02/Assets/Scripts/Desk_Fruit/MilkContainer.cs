// MilkContainer.cs
using UnityEngine;

/// <summary>
/// 牛奶瓶控制器 - 处理添加牛奶功能
/// </summary>
public class MilkContainer : MonoBehaviour
{
    [Header("牛奶设置")]
    //public GameObject milkEffectPrefab;          // 牛奶特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.white;   // 高亮颜色

    private Color originalColor;

    void Start()
    {
        if (containerRenderer != null)
        {
            originalColor = containerRenderer.color;
        }
    }

    void OnMouseDown()
    {
        AddMilkToCup();
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
            containerRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// 给杯子添加牛奶
    /// </summary>
    void AddMilkToCup()
    {
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
                EventManager.Instance.TriggerGameLog("杯子没有咖啡，无法添加牛奶", LogType.Warning);
            }
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasMilk)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过牛奶了", LogType.Warning);
            }
            return;
        }

        // 添加牛奶原料到咖啡数据
        coffeeData.AddIngredient("milk");

        // 添加牛奶原料到杯子
        cup.AddExtraIngredient("milk");

        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerIngredientAdded("milk", coffeeData, cup);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog($"已添加牛奶！当前咖啡类型：{coffeeData.type}");
        }
    }
}