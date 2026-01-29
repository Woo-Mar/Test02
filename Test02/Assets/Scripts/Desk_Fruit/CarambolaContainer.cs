// CarambolaContainer.cs
using UnityEngine;

/// <summary>
/// 杨桃篮控制器 - 处理添加杨桃片功能
/// </summary>
public class CarambolaContainer : MonoBehaviour
{
    [Header("杨桃设置")]
    //public GameObject carambolaEffectPrefab;    // 杨桃片特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.yellow;  // 高亮颜色

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
        AddCarambolaToCup();
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
    /// 给杯子添加杨桃片
    /// </summary>
    void AddCarambolaToCup()
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
                EventManager.Instance.TriggerGameLog("杯子没有咖啡，无法添加杨桃", LogType.Warning);
            }
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasCarambola)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过杨桃片了", LogType.Warning);
            }
            return;
        }

        // 添加杨桃原料到咖啡数据
        coffeeData.AddIngredient("carambola");

        // 添加杨桃原料到杯子
        cup.AddExtraIngredient("carambola");

        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerIngredientAdded("carambola", coffeeData, cup);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog($"已添加杨桃片！当前咖啡类型：{coffeeData.type}");
        }
    }
}