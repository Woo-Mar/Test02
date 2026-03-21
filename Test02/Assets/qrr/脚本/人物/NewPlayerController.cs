using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;

    [Header("组件")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 移动方向
    private Vector2 moveInput;
    private Vector2 lastMoveDirection;

    // 用于其他脚本调用的公共属性
    public Vector2 Position => transform.position;
    public GameObject PlayerObject => gameObject;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 默认朝下
        lastMoveDirection = Vector2.down;

        // 确保 Rigidbody2D 设置正确
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 获取输入
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 判断是否在移动
        bool isMoving = moveInput != Vector2.zero;

        if (isMoving)
        {
            moveInput.Normalize();
            lastMoveDirection = moveInput;
        }

        // 更新动画参数
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
        animator.SetBool("isMoving", isMoving);

        // 没有输入时用上次方向
        if (!isMoving)
        {
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }
    }

    void FixedUpdate()
    {
        // 移动
        if (rb != null)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // 供其他脚本调用的方法
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}