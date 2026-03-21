using UnityEngine;
using UnityEngine.UI;

public class LandManager : MonoBehaviour
{
    [Header("土地状态")]
    public bool isFallow = false;
    public float fallowTimeRemaining = 0;
    public float totalFallowTime = 60f;

    [Header("UI显示")]
    public Text fallowTimerText;

    [Header("交互设置")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    private BoZhongTrigger boZhongTrigger;
    private SpriteRenderer landSprite;
    private GameObject player;

    void Start()
    {
        boZhongTrigger = GetComponent<BoZhongTrigger>();
        landSprite = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (fallowTimerText != null)
            fallowTimerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // 只有已开垦的土地才能交互
        if (boZhongTrigger == null || boZhongTrigger.currentState != LandState.Cultivated)
            return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        bool isInRange = distance <= interactRange;

        // 休耕逻辑
        if (isFallow)
        {
            fallowTimeRemaining -= Time.deltaTime;

            if (fallowTimerText != null)
            {
                fallowTimerText.text = $"休耕: {Mathf.Ceil(fallowTimeRemaining)}秒";
                fallowTimerText.gameObject.SetActive(true);
            }

            if (fallowTimeRemaining <= 0)
            {
                EndFallow();
            }
        }
        else
        {
            if (fallowTimerText != null)
                fallowTimerText.gameObject.SetActive(false);
        }
    }

    void ShowPressEUI(string text)
    {
        if (boZhongTrigger != null && boZhongTrigger.PressEUI != null)
        {
            boZhongTrigger.PressEUI.SetActive(true);
            Text pressText = boZhongTrigger.PressEUI.GetComponentInChildren<Text>();
            if (pressText != null)
                pressText.text = text;
        }
    }

    void HidePressEUI()
    {
        if (boZhongTrigger != null && boZhongTrigger.PressEUI != null)
            boZhongTrigger.PressEUI.SetActive(false);
    }

    // 收获时调用
    public void OnCropHarvested()
    {
        Debug.Log("作物已收获");
    }

    // ★★★ 改为public，让其他脚本可以调用 ★★★
    public void StartFallow()
    {
        isFallow = true;
        fallowTimeRemaining = totalFallowTime;

        // 土地变灰
        if (landSprite != null)
            landSprite.color = Color.yellow;

        HidePressEUI();
        ShowWarning("开始休耕");
        Debug.Log("开始休耕");
    }

    void EndFallow()
    {
        isFallow = false;

        // 恢复土地颜色（已开垦的白色）
        if (landSprite != null && boZhongTrigger != null)
            landSprite.color = boZhongTrigger.cultivatedColor;

        if (fallowTimerText != null)
            fallowTimerText.gameObject.SetActive(false);

        ShowWarning("休耕结束");
        Debug.Log("休耕结束");
    }

    void ShowWarning(string message)
    {
        BugWarningUI warning = FindObjectOfType<BugWarningUI>();
        if (warning != null)
            warning.ShowWarning(message);
    }
}