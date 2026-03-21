using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitGateTrigger : MonoBehaviour
{
    [Header("转场效果")]
    public Image fadeImage;                 // 渐黑用的图片
    public float fadeDuration = 1.5f;        // 渐黑时间

    [Header("体验官系统")]
    public GrowthExperienceOfficer experienceOfficer;  // 引用体验官脚本

    private bool isPlayerInRange = false;

    void Start()
    {
        // 如果没有指定体验官，自动查找
        if (experienceOfficer == null)
            experienceOfficer = FindObjectOfType<GrowthExperienceOfficer>();

        // 如果有fadeImage，初始设为透明
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isPlayerInRange = true;
            Debug.Log("玩家到达路口");

            // 自动触发进入小游戏
            if (experienceOfficer != null)
            {
                StartCoroutine(FadeAndEnterGame());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isPlayerInRange = false;
        }
    }

    IEnumerator FadeAndEnterGame()
    {
        // 渐黑
        if (fadeImage != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = timer / fadeDuration;
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
                yield return null;
            }
        }

        // 进入小游戏
        if (experienceOfficer != null)
        {
            experienceOfficer.OnPlayerReachExit();
        }
    }
}