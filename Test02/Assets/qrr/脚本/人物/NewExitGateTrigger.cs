using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewExitGateTrigger : MonoBehaviour
{
    [Header("转场效果")]
    public Image fadeImage;                 // 渐黑用的图片
    public float fadeDuration = 1.5f;        // 渐黑时间

    [Header("目标场景")]
    public string targetSceneName = "Level2"; // 要跳转的场景名称

    [Header("收获条件")]
    public int requiredStrawberry = 1;       // 需要草莓数量
    public int requiredCarambola = 0;        // 需要杨桃数量
    public int requiredFig = 0;             // 需要无花果数量

    [Header("提示UI")]
    public GameObject conditionPanel;        // 条件未满足提示面板
    public Text conditionText;               // 条件提示文字
    public float panelDisplayTime = 3f;      // 提示显示时间

    private bool hasTriggered = false;
    private bool isConditionMet = false;

    void Start()
    {
        // 设置渐变图片初始透明
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }

        // 隐藏提示面板
        if (conditionPanel != null)
            conditionPanel.SetActive(false);
    }

    void Update()
    {
        // 每帧检查收获条件
        CheckHarvestCondition();
    }

    void CheckHarvestCondition()
    {
        Level1Mgr levelMgr = Level1Mgr.GetInstance();
        if (levelMgr == null) return;

        bool strawberryOk = levelMgr.m_CurCaoMeiNums >= requiredStrawberry;
        bool carambolaOk = levelMgr.m_CurYangMeiNums >= requiredCarambola;
        bool figOk = levelMgr.m_CurWuHuaGuoNums >= requiredFig;

        // 如果条件满足且还没触发过提示
        if (strawberryOk && carambolaOk && figOk && !isConditionMet)
        {
            isConditionMet = true;
            ShowSuccessMessage();
        }
    }

    void ShowSuccessMessage()
    {
        Debug.Log("✅ 收获条件满足！可以前往咖啡馆了！");

        // 显示成功提示
        BugWarningUI warning = FindObjectOfType<BugWarningUI>();
        if (warning != null)
        {
            warning.ShowWarning("✨ 已完成作物收获！\n可以去咖啡馆经营啦！ ✨");
        }

        // 或者用专门的面板显示
        if (conditionPanel != null && conditionText != null)
        {
            conditionText.text = "✨ 已完成作物收获！\n可以去咖啡馆经营啦！ ✨";
            conditionPanel.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(panelDisplayTime));
        }
    }

    IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (conditionPanel != null)
            conditionPanel.SetActive(false);
    }

    void ShowConditionFailedMessage()
    {
        Level1Mgr levelMgr = Level1Mgr.GetInstance();
        if (levelMgr == null) return;

        int strawberryLeft = Mathf.Max(0, requiredStrawberry - levelMgr.m_CurCaoMeiNums);
        int carambolaLeft = Mathf.Max(0, requiredCarambola - levelMgr.m_CurYangMeiNums);
        int figLeft = Mathf.Max(0, requiredFig - levelMgr.m_CurWuHuaGuoNums);

        string message = $"还需要:\n";
        if (strawberryLeft > 0) message += $"🍓 草莓: {strawberryLeft} 颗\n";
        if (carambolaLeft > 0) message += $"🍑 杨桃: {carambolaLeft} 颗\n";
        if (figLeft > 0) message += $"🍈 无花果: {figLeft} 颗";

        // 显示提示
        BugWarningUI warning = FindObjectOfType<BugWarningUI>();
        if (warning != null)
        {
            warning.ShowWarning(message);
        }

        // 或者用专门的面板显示
        if (conditionPanel != null && conditionText != null)
        {
            conditionText.text = message;
            conditionPanel.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(panelDisplayTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player") return;

        Level1Mgr levelMgr = Level1Mgr.GetInstance();
        if (levelMgr == null) return;

        // 检查收获条件
        bool strawberryOk = levelMgr.m_CurCaoMeiNums >= requiredStrawberry;
        bool carambolaOk = levelMgr.m_CurYangMeiNums >= requiredCarambola;
        bool figOk = levelMgr.m_CurWuHuaGuoNums >= requiredFig;

        if (strawberryOk && carambolaOk && figOk)
        {
            // 条件满足，可以跳转
            if (!hasTriggered)
            {
                hasTriggered = true;
                Debug.Log($"玩家到达出口，跳转到场景: {targetSceneName}");
                StartCoroutine(FadeAndLoadScene());
            }
        }
        else
        {
            // 条件不满足，显示提示
            ShowConditionFailedMessage();
            Debug.Log("收获条件不满足，无法进入咖啡馆");
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