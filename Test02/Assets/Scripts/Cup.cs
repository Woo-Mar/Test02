// Cup.cs - 修正无花果茶显示问题
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
    [Header("特殊外观")]
    public Sprite latteSprite;             // 拿铁外观
    public Sprite strawberryLatteSprite;   // 草莓拿铁外观
    public Sprite carambolaAmericanoSprite; // 杨桃美式外观
    public Sprite figTeaSprite;            // 无花果干茶外观
    public Sprite failedDrinkSprite;       // 失败饮品外观

    [Header("State")]
    public bool isEmpty = true;           // 是否为空杯子
    public bool hasCoffee = false;        // 是否有咖啡
    public bool hasIce = false;           // 是否有冰
    [Header("额外原料状态")]
    public bool hasMilk = false;           // 是否有牛奶
    public bool hasStrawberry = false;     // 是否有草莓酱
    public bool hasCarambola = false;      // 是否有杨桃片
    public bool hasFig = false;            // 是否有无花果干
    public bool isFailedDrink = false;     // 是否是失败饮品

    public bool isDraggable = true;       // 是否可拖拽
    public bool isBeingDragged = false;   // 是否正在被拖拽


    [Header("Drag Settings")]
    public float dragSpeed = 10f;         // 拖拽移动速度
    public float dragOffset = 0.5f;       // 拖拽时与鼠标位置的垂直偏移

    [Header("Z轴设置")]
    public float cupZPosition = -2f; // 杯子Z轴，设为-2确保在最前面

    [Header("垃圾桶检测")]
    public bool isOverTrashBin = false;
    public TrashBin currentTrashBin = null;

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

    void Update()
    {
        // 如果是空杯子且不在咖啡机上，确保Z轴正确
        if (isEmpty && !isBeingDragged)
        {
            EnsureCorrectZPosition();
        }

        // 检查是否在垃圾桶上方（如果正在拖拽）
        if (isBeingDragged)
        {
            CheckTrashBinOverlap();
        }
    }

    /// <summary>
    /// 添加额外原料
    /// </summary>
    public void AddExtraIngredient(string ingredient)
    {
        switch (ingredient.ToLower())
        {
            case "milk":
                hasMilk = true;
                break;
            case "strawberry":
                hasStrawberry = true;
                break;
            case "carambola":
                hasCarambola = true;
                break;
            case "fig":
                hasFig = true;
                // 无花果干茶不需要咖啡
                if (!hasCoffee)
                {
                    isEmpty = false;
                    isDraggable = true; // 重要：设置为可拖拽！
                    Debug.Log($"无花果茶制作完成，设置isDraggable = {isDraggable}");
                }
                break;
        }

        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerExtraIngredientAdded(this, ingredient);
        }
        // 检查配方是否正确，如果不正确则标记为失败饮品
        CheckRecipeValidity();

        // 更新外观
        UpdateCupAppearance();

        // 确保其他饮品也可以拖拽
        if ((hasCoffee || hasFig) && !isDraggable)
        {
            isDraggable = true;
        }
    }

    /// <summary>
    /// 检查配方是否正确
    /// </summary>
    private void CheckRecipeValidity()
    {
        // 如果没有饮品，不需要检查
        if (!hasCoffee && !hasFig) return;

        // 获取咖啡数据
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        if (coffeeMachine == null || coffeeMachine.currentCoffee == null)
        {
            return;
        }

        Coffee coffeeData = coffeeMachine.currentCoffee;

        // 检查是否是无花果茶
        if (coffeeData.type == Coffee.CoffeeType.FigOnly)
        {
            // 无花果茶：只能有无花果，不能有咖啡或其他原料
            if (coffeeData.hasBrewedCoffee || coffeeData.hasCoffeePowder ||
                coffeeData.hasIce || coffeeData.hasMilk ||
                coffeeData.hasStrawberry || coffeeData.hasCarambola)
            {
                isFailedDrink = true;
                Debug.Log("无花果茶配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }

        // 如果没有咖啡，则不是有效的咖啡饮品
        if (!hasCoffee)
        {
            isFailedDrink = true;
            return;
        }

        // 检查是否是热咖啡
        if (coffeeData.type == Coffee.CoffeeType.HotCoffee)
        {
            // 热咖啡：只有咖啡，不能有其他任何原料
            if (coffeeData.hasIce || coffeeData.hasMilk ||
                coffeeData.hasStrawberry || coffeeData.hasCarambola || coffeeData.hasFig)
            {
                isFailedDrink = true;
                Debug.Log("热咖啡配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }

        // 检查是否是冰咖啡
        if (coffeeData.type == Coffee.CoffeeType.IcedCoffee)
        {
            // 冰咖啡：咖啡+冰块，不能有其他原料
            if (coffeeData.hasMilk || coffeeData.hasStrawberry ||
                coffeeData.hasCarambola || coffeeData.hasFig)
            {
                isFailedDrink = true;
                Debug.Log("冰咖啡配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }

        // 检查是否是拿铁
        if (coffeeData.type == Coffee.CoffeeType.Latte)
        {
            // 拿铁：咖啡+牛奶，不能有其他原料
            if (coffeeData.hasIce || coffeeData.hasStrawberry ||
                coffeeData.hasCarambola || coffeeData.hasFig)
            {
                isFailedDrink = true;
                Debug.Log("拿铁配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }

        // 检查是否是草莓拿铁
        if (coffeeData.type == Coffee.CoffeeType.StrawberryLatte)
        {
            // 草莓拿铁：咖啡+牛奶+草莓，不能有其他原料
            if (coffeeData.hasIce || coffeeData.hasCarambola || coffeeData.hasFig)
            {
                isFailedDrink = true;
                Debug.Log("草莓拿铁配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }

        // 检查是否是杨桃美式
        if (coffeeData.type == Coffee.CoffeeType.CarambolaAmericano)
        {
            // 杨桃美式：咖啡+冰块+杨桃，不能有其他原料
            if (coffeeData.hasMilk || coffeeData.hasStrawberry || coffeeData.hasFig)
            {
                isFailedDrink = true;
                Debug.Log("杨桃美式配方错误：含有其他原料");
            }
            else
            {
                isFailedDrink = false;
            }
            return;
        }
    }

    /// <summary>
    /// 更新杯子外观（根据原料组合）
    /// </summary>
    private void UpdateCupAppearance()
    {
        if (spriteRenderer == null) return;

        // 如果是失败饮品，使用失败饮品sprite
        if (isFailedDrink && failedDrinkSprite != null)
        {
            spriteRenderer.sprite = failedDrinkSprite;
            Debug.Log("显示失败饮品外观");
            return;
        }

        // 首先检查无花果干茶
        if (hasFig && !hasCoffee)
        {
            // 无花果干茶
            if (figTeaSprite != null)
            {
                spriteRenderer.sprite = figTeaSprite;
                Debug.Log("显示无花果茶外观");
            }
            return; // 无花果茶不需要检查其他条件
        }

        // 如果只有咖啡而没有无花果，继续检查咖啡类型
        if (hasCoffee && !hasFig)
        {
            // 检查咖啡类型并更新外观
            if (hasStrawberry && hasMilk && !hasIce && !hasCarambola)
            {
                // 草莓拿铁
                if (strawberryLatteSprite != null)
                {
                    spriteRenderer.sprite = strawberryLatteSprite;
                    Debug.Log("显示草莓拿铁外观");
                }
            }
            else if (hasCarambola && hasIce && !hasMilk && !hasStrawberry)
            {
                // 杨桃美式
                if (carambolaAmericanoSprite != null)
                {
                    spriteRenderer.sprite = carambolaAmericanoSprite;
                    Debug.Log("显示杨桃美式外观");
                }
            }
            else if (hasMilk && !hasIce && !hasStrawberry && !hasCarambola)
            {
                // 拿铁
                if (latteSprite != null)
                {
                    spriteRenderer.sprite = latteSprite;
                    Debug.Log("显示拿铁外观");
                }
            }
            else if (hasIce && !hasMilk && !hasStrawberry && !hasCarambola)
            {
                // 冰咖啡
                spriteRenderer.sprite = icedCoffeeCupSprite;
                Debug.Log("显示冰咖啡外观");
            }
            else if (!hasIce && !hasMilk && !hasStrawberry && !hasCarambola && !hasFig)
            {
                // 热咖啡
                spriteRenderer.sprite = coffeeCupSprite;
                Debug.Log("显示热咖啡外观");
            }
        }
        else if (!hasCoffee && !hasFig)
        {
            // 空杯子
            spriteRenderer.sprite = emptyCupSprite;
            Debug.Log("显示空杯子外观");
        }
    }

    /// <summary>
    /// 检查是否在垃圾桶上方
    /// </summary>
    void CheckTrashBinOverlap()
    {
        TrashBin[] trashBins = FindObjectsOfType<TrashBin>();
        TrashBin closestBin = null;
        float closestDistance = float.MaxValue;

        foreach (TrashBin bin in trashBins)
        {
            float distance = Vector2.Distance(transform.position, bin.transform.position);
            if (distance <= bin.interactionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestBin = bin;
            }
        }

        // 通知垃圾桶有杯子在上方
        if (closestBin != null)
        {
            closestBin.CheckCupOverlap(this);
            isOverTrashBin = true;
            currentTrashBin = closestBin;
        }
        else
        {
            isOverTrashBin = false;
            currentTrashBin = null;
        }
    }

    // 确保杯子在正确的Z轴位置
    private void EnsureCorrectZPosition()
    {
        Vector3 pos = transform.position;
        pos.z = cupZPosition;
        transform.position = pos;
    }

    void OnMouseDown()
    {
        Debug.Log($"点击杯子 - hasCoffee: {hasCoffee}, hasFig: {hasFig}, isEmpty: {isEmpty}, isDraggable: {isDraggable}, isFailedDrink: {isFailedDrink}");

        EnsureCorrectZPosition();

        // 空杯子：点击后放置到咖啡机
        if (isEmpty && coffeeMachine != null)
        {
            coffeeMachine.PlaceCup(gameObject);
            return;
        }

        // 有饮品（咖啡或无花果茶）就可以拖拽
        bool hasAnyDrink = hasCoffee || hasFig;
        if (hasAnyDrink && isDraggable)
        {
            Debug.Log("开始拖拽杯子");
            StartDragging();
        }
        else
        {
            Debug.Log($"无法拖拽 - hasAnyDrink: {hasAnyDrink}, isDraggable: {isDraggable}");
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

        // 如果在垃圾桶上方，不检查顾客，直接丢弃
        if (isOverTrashBin && currentTrashBin != null)
        {
            // 这里不直接丢弃，因为垃圾桶会处理
            Debug.Log("杯子在垃圾桶上方释放");
            // 垃圾桶会在Update中检测到鼠标释放并处理
            ReturnToOriginalPosition(); // 或者让垃圾桶处理
        }
        else
        {
            // 检查顾客
            CheckForCustomer();
        }

        Debug.Log("停止拖拽杯子");
    }

    /// <summary>
    /// 返回到原始位置（当在垃圾桶上方释放但没有触发丢弃时）
    /// </summary>
    void ReturnToOriginalPosition()
    {
        if (coffeeMachine != null)
        {
            coffeeMachine.PlaceCup(gameObject);
        }
        else
        {
            transform.position = originalPosition;
            EnsureCorrectZPosition();
        }
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
            isDraggable = true; // 确保可拖拽
            spriteRenderer.sprite = coffeeCupSprite;

            Debug.Log($"杯子装满咖啡，设置isDraggable = {isDraggable}");

            // 装满咖啡后可拖拽
            EnsureCorrectZPosition();

            // 触发事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCupFilledWithCoffee(this);
            }

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

            // 添加冰块后检查配方是否正确
            CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
            if (coffeeMachine != null && coffeeMachine.currentCoffee != null)
            {
                coffeeMachine.currentCoffee.AddIngredient("ice");

                // 注意：这里不消耗库存，库存已在IceContainer.cs中消耗

                // 检查配方是否正确
                CheckRecipeValidity();
                UpdateCupAppearance();
            }

            // 触发杯子加冰事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCupIceAdded(this);
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("已加冰");
            }
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
        return hasCoffee || hasFig; // 有咖啡或无花果茶都可以服务
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
    /// 重置杯子状态（当订单错误时）
    /// </summary>
    public void ResetCupState()
    {
        // 重置所有原料状态
        hasMilk = false;
        hasStrawberry = false;
        hasCarambola = false;
        hasIce = false;
        hasFig = false;
        isFailedDrink = false; // 重置失败状态

        // 如果还有咖啡，保持hasCoffee为true，否则设为false
        if (!hasCoffee)
        {
            isEmpty = true;
        }

        // 重置外观为基本咖啡外观
        if (hasCoffee && !hasIce)
        {
            spriteRenderer.sprite = coffeeCupSprite;
        }
        else if (hasCoffee && hasIce)
        {
            spriteRenderer.sprite = icedCoffeeCupSprite;
        }
        else if (hasFig)
        {
            spriteRenderer.sprite = figTeaSprite;
        }
        else
        {
            spriteRenderer.sprite = emptyCupSprite;
            isEmpty = true;
            hasCoffee = false;
        }

        Debug.Log("杯子状态已重置");
    }

    /// <summary>
    /// 杯子被成功服务时的处理
    /// </summary>
    public void OnServed()
    {
        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerCupServed(this);
        }
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