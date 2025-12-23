// AutoDestroy.cs - 修正版
using UnityEngine;
using System.Collections;

/// <summary>
/// 自动销毁游戏对象组件，支持淡出效果
/// 常用于临时特效、UI元素等的生命周期管理
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("销毁设置")]
    public float destroyDelay = 2f;     // 延迟销毁时间（秒）
    public bool fadeOut = true;         // 是否启用淡出效果
    public float fadeDuration = 0.5f;   // 淡出动画持续时间

    private float timer = 0f;           // 销毁倒计时器
    private SpriteRenderer spriteRenderer; // 精灵渲染器（用于淡出效果）
    private bool isFading = false;      // 是否正在淡出中

    void Start()
    {
        timer = destroyDelay; // 初始化计时器

        // 检查是否需要淡出效果
        if (fadeOut)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                // 如果没有SpriteRenderer组件，自动关闭淡出效果
                fadeOut = false;
            }
        }
    }

    void Update()
    {
        timer -= Time.deltaTime; // 每帧减少计时器

        // 检查是否应该开始淡出效果
        if (fadeOut && timer <= fadeDuration && !isFading)
        {
            StartCoroutine(FadeOut()); // 启动淡出协程
        }

        // 计时器归零时销毁对象
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 淡出效果协程，逐渐降低对象透明度
    /// </summary>
    IEnumerator FadeOut()
    {
        isFading = true; // 标记为正在淡出
        float fadeTimer = fadeDuration; // 淡出计时器

        while (fadeTimer > 0f)
        {
            fadeTimer -= Time.deltaTime;
            float alpha = fadeTimer / fadeDuration; // 计算当前透明度（1→0）

            // 更新精灵颜色透明度
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            yield return null; // 等待下一帧
        }
    }
}