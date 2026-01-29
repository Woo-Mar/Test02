// StrawberryContainer.cs
using UnityEngine;

/// <summary>
/// 草莓篮控制器 - 处理添加草莓酱功能
/// </summary>
public class StrawberryContainer : MonoBehaviour
{
    [Header("草莓设置")]
    //public GameObject strawberryEffectPrefab;    // 草莓酱特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.red;     // 高亮颜色

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
        AddStrawberryToCup();
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
    /// 给杯子添加草莓酱
    /// </summary>
    void AddStrawberryToCup()
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
                EventManager.Instance.TriggerGameLog("杯子没有咖啡，无法添加草莓酱", LogType.Warning);
            }
            return;
        }

        // 检查是否已经添加过草莓
        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasStrawberry)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已经添加过草莓酱了", LogType.Warning);
            }
            return;
        }

        // 添加草莓原料到咖啡数据
        coffeeData.AddIngredient("strawberry");

        // 添加草莓原料到杯子
        cup.AddExtraIngredient("strawberry");

        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerIngredientAdded("strawberry", coffeeData, cup);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog($"已添加草莓酱！当前咖啡类型：{coffeeData.type}");
        }
    }
}