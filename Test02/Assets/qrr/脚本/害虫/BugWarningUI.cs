using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BugWarningUI : MonoBehaviour
{
    public Text warningText;
    public float displayTime = 2f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    public void ShowWarning(string message)
    {
        StopAllCoroutines();

        if (warningText != null)
            warningText.text = message;

        StartCoroutine(WarningSequence());
    }

    IEnumerator WarningSequence()
    {
        // µ≠»Î
        float timer = 0;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer / 0.3f;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // œ‘ æ
        yield return new WaitForSeconds(displayTime);

        // µ≠≥ˆ
        timer = 0;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1 - (timer / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}