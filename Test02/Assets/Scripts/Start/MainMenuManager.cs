using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Video;
using Unity.VisualScripting;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button exitButton;
    public Image startButtonImage; // 用于替换图片的组件
    public Sprite startButtonClickedSprite; // 点击后的新图片

    [Header("Panel & Photos")]
    public GameObject mainPanel;
    public GameObject[] photos; // 将4张照片按顺序拖入这个数组 (大小设为4)
    public GameObject panelNextButton; // Panel右下角的按钮

    [Header("Timeline")]
    public PlayableDirector timelineDirector;

    [Header("视频背景控制")]
    //修改：直接引用 RawImage 组件
    public RawImage videoRawImage;
    public VideoPlayer videoPlayer;
    public GameObject videoBackground;

    [Header("UI背景控制")]
    public GameObject uiButton;

    private void Start()
    {

        // 初始化：确保视频透明度是 1 (不透明)
        if (videoRawImage != null)
        {
            Color c = videoRawImage.color;
            c.a = 1f;
            videoRawImage.color = c;
        }

        // 初始化界面状态
        mainPanel.SetActive(false);
        panelNextButton.SetActive(false);
        foreach (var photo in photos)
        {
            photo.SetActive(false);
        }

        // 绑定按钮点击事件
        startButton.onClick.AddListener(OnStartClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // 绑定Panel内按钮的点击事件
        panelNextButton.GetComponent<Button>().onClick.AddListener(OnPanelNextButtonClicked);

        // 监听Timeline播放结束事件
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnTimelineFinished;
        }
    }

    public void OnExitClicked()
    {
        AudioManager.Instance.PlayButtonSound();
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnStartClicked()
    {
        AudioManager.Instance.PlayButtonSound();

        exitButton.gameObject.SetActive(false);
        //因为位置和nextbutton一致

        // 防止玩家多次点击
        startButton.interactable = false;

        // 切换按钮图片
        if (startButtonImage != null && startButtonClickedSprite != null)
        {
            startButtonImage.sprite = startButtonClickedSprite;
        }

        // 开启协程执行延时显示逻辑
        StartCoroutine(ShowPanelSequence());
    }

    private IEnumerator ShowPanelSequence()
    {
        // 2秒后显示Panel
        yield return new WaitForSeconds(0.5f);
        mainPanel.SetActive(true);

        // --- 核心修改：将视频背景设为完全透明 ---
        if (videoRawImage != null)
        {
            Color c = videoRawImage.color;
            c.a = 0f; // 透明度设为 0
            videoRawImage.color = c;

            // 可选：同时也关闭射线检测，防止透明状态下还能挡住后面的点击
            videoRawImage.raycastTarget = false;
        }

        // 停止视频播放（节省性能，即使透明了后台也会跑，建议停止）
        if (videoPlayer != null) videoPlayer.Stop();
        videoBackground.SetActive(false);
        uiButton.SetActive(false);
        startButton.gameObject.SetActive(false);

        // 依次显示4张照片，每次间隔0.5秒
        for (int i = 0; i < photos.Length; i++)
        {
            yield return new WaitForSeconds(2f);
            photos[i].SetActive(true);
        }

        // 4张照片显示完后，再过0.5秒显示右下角的Button
        yield return new WaitForSeconds(0.5f);
        panelNextButton.SetActive(true);
    }

    public void OnPanelNextButtonClicked()
    {
        AudioManager.Instance.PlayButtonSound(); // 可选：点击Panel按钮也播放音效

        // 隐藏Panel
        mainPanel.SetActive(false);

        // 播放Timeline
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }
        else
        {
            Debug.LogError("Timeline Director 未指定，直接跳转场景");
            LoadNextScene();
        }
    }

    // Timeline播放完毕后自动调用的回调函数
    private void OnTimelineFinished(PlayableDirector director)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // 取消事件监听，防止内存泄漏
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimelineFinished;
        }

        // 跳转到scene1
        SceneManager.LoadScene("End");
    }
}
