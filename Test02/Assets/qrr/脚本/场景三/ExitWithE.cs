using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitWithE : MonoBehaviour
{
    [Header("UI提示")]
    public GameObject pressEUI;              // 按E提示面板

    [Header("转场效果")]
    public Image fadeImage;                  // 渐黑用的图片
    public float fadeDuration = 1.5f;        // 渐黑时间

    [Header("目标场景")]
    public string targetSceneName = "Game4"; // 要跳转的场景名称

    private bool isPlayerInRange = false;
    private bool hasTriggered = false;

    void Start()
    {
        // 隐藏提示UI
        if (pressEUI != null)
            pressEUI.SetActive(false);

        // 设置渐变图片初始透明
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // 玩家在范围内且按E键
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log($"玩家按E键，跳转到场景: {targetSceneName}");
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isPlayerInRange = true;

            // 显示按E提示
            if (pressEUI != null)
            {
                pressEUI.SetActive(true);
                Text tipText = pressEUI.GetComponentInChildren<Text>();
                if (tipText != null)
                    tipText.text = "E";
            }

            Debug.Log("玩家到达出口，按E键进入下一关");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isPlayerInRange = false;

            // 隐藏按E提示
            if (pressEUI != null)
                pressEUI.SetActive(false);
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        // 渐变淡出（变黑）
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

        // 加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 渐变淡入（恢复透明）
        if (fadeImage != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = 1 - (timer / fadeDuration);
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
                yield return null;
            }
        }
    }

    // 可视化显示触发范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}