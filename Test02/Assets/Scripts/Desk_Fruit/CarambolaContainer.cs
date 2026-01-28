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
            Debug.Log("请先放置一个装有咖啡的杯子");
            return;
        }

        Cup cup = coffeeMachine.currentCup.GetComponent<Cup>();
        if (cup == null || !cup.hasCoffee)
        {
            Debug.Log("杯子没有咖啡，无法添加杨桃");
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasCarambola)
        {
            Debug.Log("已经添加过杨桃片了");
            return;
        }

        // 添加杨桃原料到咖啡数据
        coffeeData.AddIngredient("carambola");

        // 添加杨桃原料到杯子
        cup.AddExtraIngredient("carambola");

        //// 生成杨桃片特效
        //if (carambolaEffectPrefab != null)
        //{
        //    GameObject effect = Instantiate(carambolaEffectPrefab, effectSpawnPoint.position, Quaternion.identity);

        //    AutoDestroy autoDestroy = effect.AddComponent<AutoDestroy>();
        //    autoDestroy.destroyDelay = 1.5f;
        //    autoDestroy.fadeOut = true;
        //    autoDestroy.fadeDuration = 1f;
        //}

        Debug.Log("已添加杨桃片！当前咖啡类型：" + coffeeData.type);
    }
}