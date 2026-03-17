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
        ShowGuide(image1, "后山的路还没修好，村里也没什么物资。目前我们只能用后山的无花果制作简单的果茶。磨豆机也是老旧的手动款。");
        CoffeeOrderManager.Instance.SetOrderPhase(1);
    }

    void EnterPhase2()
    {
        guideStep = 2;
        ShowGuide(image2, "我们修通了进村的小路！现在可以去镇上采购咖啡豆和牛奶了。同时解锁了“村庄建设（升级）”系统，你可以尝试修缮道路来缩短物流时间！");
    }

    void EnterPhase3()
    {
        guideStep = 3;
        ShowGuide(image3, "小店名气越来越大，吸引了更多游客！解锁了“乡村荣誉（成就）”系统。虽然订单变难了，但我们的致富路也更宽了！");
    }

    void EnterPhase4()
    {
        guideStep = 4;
        string summary = $"振兴成功！\n总饮品数：{soldCount}\n资产：{GameManager.Instance.money}";
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
