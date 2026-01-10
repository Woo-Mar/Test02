// TrashEffect.cs - 垃圾桶特效
using UnityEngine;
using System.Collections;

public class TrashEffect : MonoBehaviour
{
    public float lifeTime = 2f;
    public float fadeDuration = 0.5f;
    public float layer = -2f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = lifeTime;

        // 随机旋转
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // 淡出效果
        if (timer <= fadeDuration && spriteRenderer != null)
        {
            float alpha = timer / fadeDuration;
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        // 销毁
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}