using UnityEngine;
using UnityEngine.UI;

public class PlayerAK : MonoBehaviour
{
    public GameObject m_Target;

    [Header("攻击设置")]
    public int pesticideDamage = 3;      // 杀虫伤害
    public float attackRange = 5f;

    [Header("按钮引用")]
    public Button jiaoShuiButton;         // 浇水按钮
    public Button shaChongButton;          // 杀虫按钮

    // 新玩家引用
    private NewPlayerController newPlayer;

    void Start()
    {
        Debug.Log("=== PlayerAK 启动 ===");

        // 查找新玩家
        newPlayer = FindObjectOfType<NewPlayerController>();
        if (newPlayer == null)
        {
            Debug.LogError("找不到 NewPlayerController！");
        }

        if (jiaoShuiButton != null)
        {
            jiaoShuiButton.onClick.AddListener(UseJiaoShui);
            Debug.Log("浇水按钮绑定成功");
        }
        else
        {
            Debug.LogError("浇水按钮未绑定！");
        }

        if (shaChongButton != null)
        {
            shaChongButton.onClick.AddListener(UseShaChong);
            Debug.Log("杀虫按钮绑定成功");
        }
        else
        {
            Debug.LogError("杀虫按钮未绑定！");
        }
    }

    void Update()
    {
        FindTargetInRange();
    }

    void FindTargetInRange()
    {
        if (newPlayer == null) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(newPlayer.Position, attackRange);

        m_Target = null;
        float minDistance = attackRange;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(newPlayer.Position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    m_Target = hit.gameObject;
                }
            }
        }
    }

    public void UseJiaoShui()
    {
        Debug.Log("浇水按钮点击 - 只对作物有效");
    }

    public void UseShaChong()
    {
        if (m_Target == null)
        {
            Debug.Log("❌ 没有目标敌人");
            return;
        }

        EnemyPathWalker enemy = m_Target.GetComponent<EnemyPathWalker>();
        if (enemy == null) return;

        if (AudioManager1.Instance != null)
        {
            AudioManager1.Instance.PlaySFX(AudioManager1.Instance.attackSound);
        }

        enemy.TakeDamage(pesticideDamage);
        Debug.Log($"✅ 杀虫攻击造成 {pesticideDamage} 伤害");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}