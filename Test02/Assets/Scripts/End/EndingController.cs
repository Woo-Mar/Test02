using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndingController : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject panel1;
    public GameObject nextButton;
    public GameObject exitButton;

    [Header("漫画图片对象(按1到4顺序拖入)")]
    public GameObject[] comicImages;   // 存放4个Image游戏物体的数组

    [Header("音频设置")]
    public AudioSource audioSource;    // 可拖入AudioSource组件，如果不拖入则自动获取

    private int clickCount = 0;

    void Start()
    {
        // 1. 初始化UI状态
        panel1.SetActive(false);
        nextButton.SetActive(true);
        exitButton.SetActive(false);

        // 遍历隐藏所有的漫画图片
        foreach (GameObject img in comicImages)
        {
            img.SetActive(false);
        }

        // 如果没有手动拖入AudioSource，尝试从当前对象获取
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 2. 开启协程：2秒后显示Panel1
        StartCoroutine(ShowPanelAfterDelay());
    }

    private IEnumerator ShowPanelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        panel1.SetActive(true);
    }

    // 绑定给右下角按钮(NextButton)的点击事件
    public void OnNextButtonClicked()
    {
        clickCount++;

        // 如果点击次数在1到4之间
        if (clickCount >= 1 && clickCount <= 4)
        {
            // 获取当前需要展示的漫画对象 (数组索引从0开始，所以减1)
            GameObject currentComic = comicImages[clickCount - 1];
            currentComic.SetActive(true);

            // --- 新增：点击第二张时播放音频 ---
            if (clickCount == 2 && audioSource != null && audioSource.clip != null)
            {
                audioSource.Play(); // 如果希望同时播放多个音频，可以使用 PlayOneShot(audioSource.clip)
            }
            // --------------------------------

            // 当展示到第4张漫画时，触发结束延时
            if (clickCount == 4)
            {
                StartCoroutine(EndSequence());
            }
        }
    }

    private IEnumerator EndSequence()
    {
        // 将按钮设为不可交互，防止狂点
        nextButton.GetComponent<Button>().interactable = false;

        // 延时3秒
        yield return new WaitForSeconds(3f);

        // 隐藏右下角按钮，显示退出按钮
        nextButton.SetActive(false);
        exitButton.SetActive(true);
    }

    // 绑定给ExitButton的点击事件
    public void OnExitButtonClicked()
    {
        Debug.Log("退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}