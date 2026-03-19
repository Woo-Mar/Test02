using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ProgressGuideManager : MonoBehaviour
{
    public static ProgressGuideManager Instance;

    [Header("Panel UI 引用")]
    public GameObject guidePanel;
    public Image guideImage;
    public TMP_Text guideText;
    public Button guideButton;

    [Header("阶段资源")]
    public Sprite image1, image2, image3, image4;

    [Header("Text")]
    [TextArea] public string text1;
    [TextArea] public string text2;
    [TextArea] public string text3;
    [TextArea] public string text4;

    [Header("Buttons")]
    public GameObject upgradeButton;
    public GameObject achievementButton;
    public Image ushadow;
    public Image ashadow;
    public GameObject grindButton, brewButton;
    public GameObject milkContainer, strawberryContainer, carambolaContainer;
    public GameObject brewProgressBar; // 萃取进度条物体

    [HideInInspector] public int guideStep = 0;
    private int soldCount = 0;

    void Awake() { Instance = this; }

    void Start()
    {
        // --- 初始隐藏设置 ---
        upgradeButton.SetActive(false);
        achievementButton.SetActive(false);
        ushadow.gameObject.SetActive(true);
        ashadow.gameObject.SetActive(true);
        grindButton.SetActive(false);
        brewButton.SetActive(false);
        milkContainer.SetActive(false);
        strawberryContainer.SetActive(false);
        carambolaContainer.SetActive(false);
        if (brewProgressBar!= null) brewProgressBar.SetActive(false);

        guideButton.onClick.AddListener(OnGuideButtonClick);

        // 订阅事件
        EventManager.Instance.OnOrderCompleted += OnOrderCompleted;

        // 启动第一阶段
        EnterPhase1();
    }

    void OnOrderCompleted(Customer c, int reward)
    {
        soldCount++;
        // 阶段 1 -> 2 (售出3杯无花果茶)
        if (guideStep == 1 && soldCount >= 3) EnterPhase2();
        // 阶段 2 -> 3 (售出6杯)
        else if (guideStep == 2 && soldCount >= 6) EnterPhase3();
        // 阶段 3 -> 4 (400金币)
        if (GameManager.Instance.money >= 400 && guideStep < 4) EnterPhase4();
    }

    void EnterPhase1()
    {
        guideStep = 1;
        ShowGuide(image1, "后山路还没修好，村里物资少少的，暂时只能喝无花果茶啦～");
        CoffeeOrderManager.Instance.SetOrderPhase(1);
    }

    void EnterPhase2()
    {
        guideStep = 2;
        ShowGuide(image2, "小路终于通车啦！能去镇上买咖啡牛奶咯～解锁村庄建设，修路还能提速！VIP客户登场，每单1-2杯！");
    }

    void EnterPhase3()
    {
        guideStep = 3;
        ShowGuide(image3, "小店名气传开啦，游客越来越多！解锁乡村荣誉，订单变难？不怕！新增急躁客户，每单2-3杯，生意更火爆啦！");
    }

    void EnterPhase4()
    {
        guideStep = 4;
        string summary = $"恭喜你通关啦！\n总售卖饮品数：{soldCount}！\n资产：{GameManager.Instance.money}！";
        ShowGuide(image4, summary);
    }

    void ShowGuide(Sprite img, string txt)
    {
        guidePanel.SetActive(true);
        guideImage.sprite = img;
        guideText.text = txt;
        GameManager.Instance.SetPause(true);
    }

    void OnGuideButtonClick()
    {
        guidePanel.SetActive(false);
        GameManager.Instance.SetPause(false);

        if (guideStep == 2)
        {
            upgradeButton.SetActive(true);
            ushadow.gameObject.SetActive(false);
            grindButton.SetActive(true);
            brewButton.SetActive(true);
            milkContainer.SetActive(true);
            strawberryContainer.SetActive(true);
            CoffeeOrderManager.Instance.SetOrderPhase(2);
        }
        else if (guideStep == 3)
        {
            achievementButton.SetActive(true);
            ashadow.gameObject.SetActive(false);
            carambolaContainer.SetActive(true);
            CoffeeOrderManager.Instance.SetOrderPhase(3);
        }
        else if (guideStep == 4)
        {
            SceneTransitionManager.LoadSceneClean("Scene1");
        }
    }
}
