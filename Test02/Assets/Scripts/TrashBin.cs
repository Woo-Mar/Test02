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
    public GameObject trashEffectPrefab;   // 丢弃特效
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
        Debug.Log($"准备丢弃杯子: {cup.name}, 咖啡状态: {cup.hasCoffee}");

        // 获取咖啡机引用
        CoffeeMachine coffeeMachine = FindObjectOfType<CoffeeMachine>();
        CupContainer cupContainer = FindObjectOfType<CupContainer>();

        if (coffeeMachine == null)
        {
            Debug.LogError("未找到咖啡机！");
            return;
        }

        // 播放丢弃特效
        if (trashEffectPrefab != null)
        {
            GameObject effect = Instantiate(trashEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 播放音效
        if (trashSound != null)
        {
            AudioSource.PlayClipAtPoint(trashSound, transform.position);
        }

        // 检查是否是咖啡机上的当前杯子
        if (coffeeMachine.currentCup == cup.gameObject)
        {
            Debug.Log("丢弃咖啡机上的当前杯子");

            // 解除咖啡机的绑定
            coffeeMachine.currentCup = null;
            coffeeMachine.UpdateUI();
        }

        // 如果杯子容器存在，增加可用杯子数量
        if (cupContainer != null)
        {
            cupContainer.RefillCups(1);
            Debug.Log("杯子容器可用杯子数量+1");
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