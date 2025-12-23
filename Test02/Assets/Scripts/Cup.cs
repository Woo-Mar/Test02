// Cup.cs - 完整修改版（添加鼠标拖拽功能）
using UnityEngine;
using System.Collections;

/// <summary>
/// 杯子控制器 - 管理杯子的状态、拖拽交互和服务逻辑
/// 支持拖拽到顾客处完成服务
/// </summary>
public class Cup : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite emptyCupSprite;        // 空杯子精灵
    public Sprite coffeeCupSprite;       // 热咖啡杯子精灵
    public Sprite icedCoffeeCupSprite;   // 冰咖啡杯子精灵

    [Header("State")]
    public bool isEmpty = true;           // 是否为空杯子
    public bool hasCoffee = false;        // 是否有咖啡
    public bool hasIce = false;           // 是否有冰
    public bool isDraggable = true;       // 是否可拖拽
    public bool isBeingDragged = false;   // 是否正在被拖拽

    [Header("Drag Settings")]
    public float dragSpeed = 10f;         // 拖拽移动速度
    public float dragOffset = 0.5f;       // 拖拽时与鼠标位置的垂直偏移

    [Header("Z轴设置")]
    public float cupZPosition = -2f; // 杯子Z轴，设为-2确保在最前面

    private SpriteRenderer spriteRenderer; // 精灵渲染器
    private CoffeeMachine coffeeMachine;   // 咖啡机引用
    private Vector3 dragOffsetVector;      // 拖拽偏移向量
    private Vector3 originalPosition;       // 原始位置（拖拽前）
    private Collider2D cupCollider;         // 碰撞器
    private int originalSortingOrder;       // 原始渲染层级

    void Start()
    {
        // 组件初始化
        spriteRenderer = GetComponent<SpriteRenderer>();
        cupCollider = GetComponent<Collider2D>();
        coffeeMachine = FindObjectOfType<CoffeeMachine>(); // 查找场景中的咖啡机

        // 初始化Z轴
        EnsureCorrectZPosition();

        // 设置初始外观
        if (isEmpty)
        {
            spriteRenderer.sprite = emptyCupSprite;
        }

        originalSortingOrder = spriteRenderer.sortingOrder; // 保存原始渲染层级
    }

    // 确保杯子在正确的Z轴位置
    private void EnsureCorrectZPosition()
    {
        Vector3 pos = transform.position;
        pos.z = cupZPosition;
        transform.position = pos;
    }
    /// <summary>
    /// 鼠标按下事件处理
    /// </summary>
    void OnMouseDown()
    {
        // 先确保Z轴正确
        EnsureCorrectZPosition();

        // 空杯子：点击后放置到咖啡机
        if (isEmpty && coffeeMachine != null)
        {
            coffeeMachine.PlaceCup(gameObject);
            return;
        }

        // 有咖啡的杯子：开始拖拽
        if (hasCoffee && isDraggable)
        {
            StartDragging();
        }
    }

    /// <summary>
    /// 开始拖拽杯子
    /// </summary>
    void StartDragging()
    {
        isBeingDragged = true;
        originalPosition = transform.position;
        // 确保原始位置的Z轴正确
        originalPosition.z = cupZPosition;

        // 计算拖拽偏移（让杯子显示在鼠标上方）
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = cupZPosition; // 鼠标位置也要用杯子的Z轴
        dragOffsetVector = transform.position - mousePos;
        dragOffsetVector.y += dragOffset; // 垂直偏移
        dragOffsetVector.z = 0; // 偏移向量的Z轴设为0，我们会在其他地方控制Z轴

        // 提高渲染层级，确保拖拽时在最上层
        spriteRenderer.sortingOrder = 100;
        // 拖拽视觉效果：轻微放大
        transform.localScale *= 1.1f;

        Debug.Log("开始拖拽杯子");
    }

    /// <summary>
    /// 鼠标拖拽中事件处理
    /// </summary>
    void OnMouseDrag()
    {
        if (isBeingDragged)
        {
            // 平滑跟随鼠标移动
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = cupZPosition; // 确保使用杯子的Z轴

            Vector3 targetPosition = mousePos + dragOffsetVector;
            targetPosition.z = cupZPosition; // 确保目标位置的Z轴正确
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);
            // 确保Z轴正确（防止插值导致Z轴变化）
            EnsureCorrectZPosition();
        }
    }

    /// <summary>
    /// 鼠标释放事件处理
    /// </summary>
    void OnMouseUp()
    {
        if (isBeingDragged)
        {
            StopDragging();
        }
    }

    /// <summary>
    /// 停止拖拽
    /// </summary>
    void StopDragging()
    {
        isBeingDragged = false;

        // 恢复原始渲染层级和大小
        spriteRenderer.sortingOrder = originalSortingOrder;
        transform.localScale /= 1.1f;

        // 确保Z轴正确
        EnsureCorrectZPosition();
        // 检查是否拖拽到了顾客附近
        CheckForCustomer();

        Debug.Log("停止拖拽杯子");
    }

    // 每次修改位置后都调用此方法
    void LateUpdate()
    {
        // 如果是空杯子且不在咖啡机上，确保Z轴正确
        if (isEmpty && !isBeingDragged)
        {
            EnsureCorrectZPosition();
        }
    }

    /// <summary>
    /// 检查杯子是否在顾客服务范围内
    /// </summary>
    void CheckForCustomer()
    {
        // 查找场景中所有顾客
        Customer[] customers = FindObjectsOfType<Customer>();

        foreach (Customer customer in customers)
        {
            // 计算与顾客的距离
            float distance = Vector2.Distance(transform.position, customer.transform.position);
            float serveDistance = 1.5f; // 服务有效距离

            if (distance <= serveDistance)
            {
                // 尝试服务该顾客
                customer.TryServeCoffee(this);
                return;
            }
        }

        // 没有找到可服务的顾客，返回咖啡机
        ReturnToCoffeeMachine();
    }

    /// <summary>
    /// 返回咖啡机位置
    /// </summary>
    public void ReturnToCoffeeMachine()
    {
        if (coffeeMachine != null)
        {
            // 重新放置到咖啡机
            coffeeMachine.PlaceCup(gameObject);
        }
        else
        {
            // 没有咖啡机则返回原始位置
            transform.position = originalPosition;
        }
    }

    /// <summary>
    /// 给杯子装满咖啡
    /// </summary>
    public void FillWithCoffee()
    {
        if (isEmpty)
        {
            hasCoffee = true;
            isEmpty = false;
            spriteRenderer.sprite = coffeeCupSprite; // 更新为咖啡杯子外观

            // 装满咖啡后可拖拽
            isDraggable = true;
            // 装咖啡后确保Z轴正确
            EnsureCorrectZPosition();
            Debug.Log("杯子已装满咖啡");
        }
    }

    /// <summary>
    /// 给咖啡加冰
    /// </summary>
    public void AddIce()
    {
        if (hasCoffee && !hasIce)
        {
            hasIce = true;
            spriteRenderer.sprite = icedCoffeeCupSprite; // 更新为冰咖啡外观

            Debug.Log("已加冰");
        }
    }

    /// <summary>
    /// 检查咖啡是否符合顾客订单要求
    /// </summary>
    /// <param name="wantsIce">顾客是否要冰咖啡</param>
    /// <returns>是否符合要求</returns>
    public bool MatchesOrder(bool wantsIce)
    {
        if (!hasCoffee) return false; // 没有咖啡肯定不符合

        // 冰咖啡需求匹配检查
        return (wantsIce && hasIce) || (!wantsIce && !hasIce);
    }

    /// <summary>
    /// 检查杯子是否准备好服务（有咖啡即可服务）
    /// </summary>
    public bool IsReadyToServe()
    {
        return hasCoffee;
    }

    /// <summary>
    /// 销毁杯子
    /// </summary>
    public void DestroyCup()
    {
        Debug.Log("销毁杯子");

        // 如果有咖啡机引用，清空当前杯子引用
        if (coffeeMachine != null && coffeeMachine.currentCup == this.gameObject)
        {
            coffeeMachine.currentCup = null;
            coffeeMachine.UpdateUI(); // 更新咖啡机UI状态
        }

        // 销毁杯子对象
        Destroy(gameObject);
    }

    /// <summary>
    /// 杯子被成功服务时的处理
    /// </summary>
    public void OnServed()
    {
        // 播放服务动画效果
        StartCoroutine(ServeAnimation());
    }

    /// <summary>
    /// 服务动画协程（缩小淡出效果）
    /// </summary>
    IEnumerator ServeAnimation()
    {
        float duration = 0.5f; // 动画持续时间
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale; // 原始大小

        // 动画循环
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; // 动画进度（0→1）

            // 逐渐缩小
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            // 同时淡出透明度
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;

            yield return null; // 等待下一帧
        }

        // 动画完成后重置杯子
        DestroyCup();
    }
}