// TrashBin.cs - 垃圾桶控制器
using UnityEngine;
using System.Collections;

public class TrashBin : MonoBehaviour
{
    [Header("垃圾桶设置")]
    public float interactionRadius = 1.5f; // 交互半径
    public SpriteRenderer binRenderer;     // 垃圾桶精灵渲染器
    public Color highlightColor = Color.red; // 高亮颜色

    [Header("视觉效果")]
    public AudioClip trashSound;          // 丢弃音效

    private Color originalColor;
    private bool isCupOver = false;
    private Cup currentCupOver = null;

    void Start()
    {
        if (binRenderer != null)
        {
            originalColor = binRenderer.color;
        }
    }

    void Update()
    {
        // 如果鼠标按下且有杯子在垃圾桶上方，处理丢弃
        if (Input.GetMouseButtonUp(0) && isCupOver && currentCupOver != null)
        {
            DiscardCup(currentCupOver);
        }
    }

    /// <summary>
    /// 检查杯子是否在垃圾桶上方
    /// </summary>
    public void CheckCupOverlap(Cup cup)
    {
        if (cup == null) return;

        float distance = Vector2.Distance(transform.position, cup.transform.position);

        if (distance <= interactionRadius)
        {
            // 杯子在垃圾桶范围内
            isCupOver = true;
            currentCupOver = cup;

            // 高亮垃圾桶
            if (binRenderer != null)
            {
                binRenderer.color = highlightColor;
            }
        }
        else
        {
            // 杯子离开垃圾桶范围
            if (currentCupOver == cup)
            {
                isCupOver = false;
                currentCupOver = null;

                // 恢复颜色
                if (binRenderer != null)
                {
                    binRenderer.color = originalColor;
                }
            }
        }
    }

    /// <summary>
    /// 丢弃杯子
    /// </summary>
    void DiscardCup(Cup cup)
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog($"准备丢弃杯子: {cup.name}, 咖啡状态: {cup.hasCoffee}");
        }

        // 获取咖啡机引用
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        CupContainer cupContainer = FindObjectOfType<CupContainer>();

        if (coffeeMachine == null)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("未找到咖啡机！", LogType.Error);
            }
            return;
        }

        // 播放音效
        if (trashSound != null)
        {
            AudioSource.PlayClipAtPoint(trashSound, transform.position);
        }

        // 检查是否是咖啡机上的当前杯子
        if (coffeeMachine.currentCup == cup.gameObject)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameLog("丢弃咖啡机上的当前杯子");
            }

            // 解除咖啡机的绑定
            coffeeMachine.currentCup = null;
            coffeeMachine.UpdateUI();
        }

        // 重要：不再增加杯子容器数量，因为杯子已经被消耗了
        // 杯子被丢弃就是被消耗了，不应该再回到库存中
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameLog("杯子已被丢弃，从库存中扣除");
        }

        // 触发杯子丢弃事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerCupDiscarded(cup);
        }

        // 销毁杯子
        cup.DestroyCup();

        // 重置状态
        isCupOver = false;
        currentCupOver = null;

        // 恢复颜色
        if (binRenderer != null)
        {
            binRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// 在编辑器中显示交互范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}