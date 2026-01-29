// CupContainer.cs - 修正版
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 杯子容器管理器 - 管理杯子的存储和供应
/// 点击容器可以获取新杯子
/// </summary>
public class CupContainer : MonoBehaviour
{
    [Header("杯子容器设置")]
    public CoffeeMachine coffeeMachine;       // 咖啡机引用
    public GameObject cupPrefab;              // 杯子预制体
    public int maxCupsInContainer = 5;        // 容器最大容量

    [Header("视觉效果")]
    public SpriteRenderer containerRenderer;  // 容器精灵渲染器
    public Sprite emptyContainerSprite;       // 空容器精灵
    public Sprite fullContainerSprite;        // 满容器精灵

    private int currentCupsInContainer = 3;   // 当前杯子数量

    void Start()
    {
        // 自动查找咖啡机引用
        if (coffeeMachine == null)
        {
            coffeeMachine = FindObjectOfType<CoffeeMachine>();
        }

        UpdateVisuals(); // 更新容器外观
    }

    /// <summary>
    /// 鼠标点击容器事件
    /// </summary>
    void OnMouseDown()
    {
        TrySpawnCup(); // 尝试生成杯子
    }

    /// <summary>
    /// 尝试从容器生成杯子（公共方法，咖啡机也可调用）
    /// </summary>
    public void TrySpawnCup()
    {
        Debug.Log("尝试从容器获取杯子...");
        Debug.Log($"容器当前杯子数: {currentCupsInContainer}");
        Debug.Log($"咖啡机当前杯子: {coffeeMachine.currentCup}");

        // 检查容器是否为空
        if (currentCupsInContainer <= 0)
        {
            Debug.Log("杯子容器已空！");
            return;
        }

        // 检查咖啡机是否已有杯子
        if (coffeeMachine.currentCup != null)
        {
            Debug.Log("咖啡机上已有杯子，请先使用当前杯子！");

            // 如果咖啡机上的杯子是空的，自动清空它
            Cup cupScript = coffeeMachine.currentCup.GetComponent<Cup>();
            if (cupScript != null && cupScript.isEmpty)
            {
                Debug.Log("检测到咖啡机上的杯子是空的，正在清空...");
                coffeeMachine.ClearCurrentCup();

                // 清空后重新尝试生成
                TrySpawnCup();
            }
            return;
        }

        // 减少容器杯子数量并生成新杯子
        currentCupsInContainer--;
        SpawnCupOnMachine(); // 在咖啡机上生成杯子
        UpdateVisuals();      // 更新容器外观

        Debug.Log($"生成杯子成功，容器剩余杯子: {currentCupsInContainer}");
    }

    /// <summary>
    /// 在咖啡机上生成杯子
    /// </summary>
    private void SpawnCupOnMachine()
    {
        if (cupPrefab == null || coffeeMachine == null)
        {
            Debug.LogError("杯子预制体或咖啡机引用为空！");
            return;
        }

        Debug.Log($"咖啡机cupPosition位置: {coffeeMachine.cupPosition.position}");

        // 实例化新杯子
        GameObject newCup = Instantiate(cupPrefab);

        // 确保初始Z轴正确
        Vector3 cupPos = newCup.transform.position;
        cupPos.z = -2f; // 设置初始Z轴
        newCup.transform.position = cupPos;

        // 获取杯子脚本组件
        Cup cupScript = newCup.GetComponent<Cup>();
        if (cupScript == null)
        {
            Debug.LogError("杯子预制体没有Cup组件！");
            Destroy(newCup);
            return;
        }

        // 设置杯子初始状态
        cupScript.isEmpty = true;
        cupScript.isDraggable = false;

        // 调用咖啡机的放置杯子方法
        coffeeMachine.PlaceCup(newCup);
    }

    /// <summary>
    /// 更新容器外观（根据剩余杯子数量）
    /// </summary>
    void UpdateVisuals()
    {
        if (containerRenderer == null) return;

        // 根据杯子数量切换精灵
        if (currentCupsInContainer <= 0)
        {
            containerRenderer.sprite = emptyContainerSprite;
        }
        else
        {
            containerRenderer.sprite = fullContainerSprite;
        }

        // 根据剩余数量调整透明度（视觉反馈）
        float fullness = (float)currentCupsInContainer / maxCupsInContainer;
        Color color = containerRenderer.color;
        color.a = 0.5f + fullness * 0.5f; // 透明度从50%到100%
        containerRenderer.color = color;
    }

    /// <summary>
    /// 从咖啡机回收空杯子到容器
    /// </summary>
    /// <param name="cup">要回收的杯子</param>
    public void ReturnCupToContainer(GameObject cup)
    {
        // 检查容器是否已满
        if (currentCupsInContainer >= maxCupsInContainer)
        {
            Debug.Log("杯子容器已满，无法回收更多杯子");
            Destroy(cup); // 容器满时销毁杯子
            return;
        }

        // 回收杯子
        currentCupsInContainer++;
        UpdateVisuals();

        Debug.Log($"回收杯子，容器当前杯子: {currentCupsInContainer}");
    }

    /// <summary>
    /// 补充杯子到容器
    /// </summary>
    /// <param name="amount">补充数量</param>
    public void RefillCups(int amount)
    {
        currentCupsInContainer = Mathf.Min(currentCupsInContainer + amount, maxCupsInContainer);
        UpdateVisuals();

        Debug.Log($"补充杯子，当前数量: {currentCupsInContainer}");
    }

    /// <summary>
    /// 检查容器中是否有杯子
    /// </summary>
    public bool HasCups()
    {
        return currentCupsInContainer > 0;
    }

    /// <summary>
    /// 获取剩余杯子数量
    /// </summary>
    public int GetRemainingCups()
    {
        return currentCupsInContainer;
    }

    /// <summary>
    /// 鼠标悬停效果
    /// </summary>
    void OnMouseEnter()
    {
        if (containerRenderer != null)
        {
            containerRenderer.color = Color.yellow; // 高亮显示
        }
    }

    /// <summary>
    /// 鼠标离开效果
    /// </summary>
    void OnMouseExit()
    {
        UpdateVisuals(); // 恢复原始外观
    }
}