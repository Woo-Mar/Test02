// FigContainer.cs
using UnityEngine;

/// <summary>
/// 无花果干篮控制器 - 处理添加无花果干功能
/// </summary>
public class FigContainer : MonoBehaviour
{
    [Header("无花果设置")]
    //public GameObject figEffectPrefab;           // 无花果干特效预制体
    //public Transform effectSpawnPoint;          // 特效生成位置

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;     // 容器精灵渲染器
    public Color highlightColor = Color.magenta; // 高亮颜色

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
        AddFigToCup();
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
    /// 给杯子添加无花果干
    /// </summary>
    void AddFigToCup()
    {
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCup == null)
        {
            Debug.Log("请先放置一个杯子");
            return;
        }

        Cup cup = coffeeMachine.currentCup.GetComponent<Cup>();
        if (cup == null)
        {
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;
        if (coffeeData.hasFig)
        {
            Debug.Log("已经添加过无花果干了");
            return;
        }

        // 添加无花果原料到咖啡数据
        coffeeData.AddIngredient("fig");

        // 添加无花果原料到杯子
        cup.AddExtraIngredient("fig");

        //// 生成无花果干特效
        //if (figEffectPrefab != null)
        //{
        //    GameObject effect = Instantiate(figEffectPrefab, effectSpawnPoint.position, Quaternion.identity);

        //    AutoDestroy autoDestroy = effect.AddComponent<AutoDestroy>();
        //    autoDestroy.destroyDelay = 1.5f;
        //    autoDestroy.fadeOut = true;
        //    autoDestroy.fadeDuration = 1f;
        //}

        Debug.Log("已添加无花果干！当前产品类型：" + coffeeData.type);
    }
}