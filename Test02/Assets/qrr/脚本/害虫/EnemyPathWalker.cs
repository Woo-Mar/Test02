using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class EnemyPathWalker : MonoBehaviour
{
    public int m_ID;  // 0:第一种害虫, 1:第二种害虫

    [Header("移动设置")]
    public float moveSpeed = 2f;
    public Transform[] waypoints;
    public float reachDistance = 0.1f;

    [Header("血量设置")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("血量UI")]
    public Slider healthSlider;           // 血条Slider
    public GameObject healthBarCanvas;    // 血条Canvas容器

    [Header("攻击设置")]
    public int damageToCrop = 20;          // 对作物的伤害
    public float attackInterval = 2f;      // 攻击间隔
    private float attackTimer;

    private bool isFacingRight = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isAttacking = false;      // 是否进入攻击模式
    private Transform targetCrop;           // 目标作物

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        UpdateHealthBar();
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);
    }

    public void InitData(Transform _lujing)
    {
        waypoints = new Transform[_lujing.childCount];
        for (int i = 0; i < _lujing.childCount; i++)
        {
            waypoints[i] = _lujing.GetChild(i);
        }
    }

    void Update()
    {
        if (isAttacking)
        {
            // 攻击模式：寻找并攻击作物
            AttackMode();
        }
        else
        {
            // 路径模式：沿路径移动
            FollowPath();
        }

        // 更新血条方向
        if (healthBarCanvas != null && healthBarCanvas.activeSelf)
        {
            healthBarCanvas.transform.LookAt(
                healthBarCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    void FollowPath()
    {
        if (waypoints.Length == 0) return;

        Vector2 targetPosition = waypoints[currentWaypointIndex].position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (animator != null)
            animator.SetBool("IsMoving", true);

        float distanceToWaypoint = Vector2.Distance(transform.position, targetPosition);
        if (distanceToWaypoint < reachDistance)
        {
            currentWaypointIndex++;

            // 到达最后一个路点，切换到攻击模式
            if (currentWaypointIndex >= waypoints.Length)
            {
                isAttacking = true;
                if (animator != null)
                    animator.SetBool("IsMoving", false);
                FindNearestCrop();
            }
        }

        UpdateFacingDirection(targetPosition);
    }

    void AttackMode()
    {
        if (targetCrop == null || !targetCrop.gameObject.activeSelf)
        {
            FindNearestCrop();
            if (targetCrop == null) return;
        }

        float distanceToCrop = Vector2.Distance(transform.position, targetCrop.position);

        if (distanceToCrop < 0.5f)  // 攻击范围
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                AttackCrop();
                attackTimer = 0;
                Debug.Log($"害虫攻击！距离：{distanceToCrop}");
            }
        }
        else
        {
            // 向作物移动
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetCrop.position,
                moveSpeed * Time.deltaTime
            );
            UpdateFacingDirection(targetCrop.position);
        }
    }
    void FindNearestCrop()
    {
        NongZuoWuC[] crops = FindObjectsOfType<NongZuoWuC>();
        float minDistance = float.MaxValue;
        Transform nearestCrop = null;

        foreach (NongZuoWuC crop in crops)
        {
            if (crop.gameObject.activeSelf && crop.GroupUpValue < 3)  // 只攻击未成熟的作物
            {
                float distance = Vector2.Distance(transform.position, crop.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCrop = crop.transform;
                }
            }
        }

        targetCrop = nearestCrop;
    }

    void AttackCrop()
    {
        if (targetCrop != null)
        {
            NongZuoWuC crop = targetCrop.GetComponent<NongZuoWuC>();
            if (crop != null)
            {
                // 确保这行代码存在且被调用
                crop.OnPestAttack(damageToCrop);

                if (animator != null)
                    animator.SetTrigger("Attack");

                Debug.Log($"🐛 害虫攻击作物！造成{damageToCrop}伤害，作物当前健康：{crop.m_CurJianKangPower}");
            }
        }
    }

    private void UpdateFacingDirection(Vector2 targetPos)
    {
        float moveDirectionX = targetPos.x - transform.position.x;
        if (moveDirectionX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveDirectionX < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"=== 敌人受伤 ===");
        Debug.Log($"受伤前血量: {currentHealth}/{maxHealth}");
        Debug.Log($"受到伤害: {damage}");

        currentHealth -= damage;

        Debug.Log($"受伤后血量: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashRed());
        UpdateHealthBar();

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Debug.Log("💀 敌人死亡");
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthSlider.value = healthPercent;

            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (healthPercent > 0.6f)
                    fillImage.color = Color.green;
                else if (healthPercent > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }

        if (healthBarCanvas != null && currentHealth < maxHealth)
        {
            healthBarCanvas.SetActive(true);
        }
    }

    // 修改这里：消灭害虫得1金币
    private void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        // 奖励金币（统一给1金币）
        if (ShopSystem.Instance != null)
        {
            int reward = 1;  // 不管害虫类型，统一给1金币
            ShopSystem.Instance.playerGold += reward;
            ShopSystem.Instance.UpdateGoldText();
            Debug.Log($"💰 消灭害虫(ID:{m_ID})，获得{reward}金币");
        }

        Destroy(gameObject, 0.5f);
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            int nextIndex = (i + 1) % waypoints.Length;
            Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.1f);
        }
    }
}