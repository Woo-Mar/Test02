using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ProgressGuideManager : MonoBehaviour
{
    public static ProgressGuideManager Instance;

    [Header("Panel")]
    public GameObject guidePanel;
    public Image guideImage;
    public TMP_Text guideText;
    public Button guideButton;

    [Header("Images")]
    public Sprite image1;
    public Sprite image2;

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

    private int soldCount = 0;
    private int earnedMoney = 0;

    private int guideStep = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        upgradeButton.SetActive(false);
        achievementButton.SetActive(false);
        ushadow.gameObject.SetActive(true);
        ashadow.gameObject.SetActive(true);

        guideButton.onClick.AddListener(OnGuideButtonClick);

        ShowGuide(image1, text1);
        guideStep = 1;

        EventManager.Instance.OnOrderCompleted += OnOrderCompleted;
        EventManager.Instance.OnMoneyEarned += OnMoneyEarned;
    }

    void OnOrderCompleted(Customer c, int reward)
    {
        soldCount++;

        if (guideStep == 1 && soldCount >= 3)
        {
            ShowGuide(image2, text2);
            guideStep = 2;
        }

        else if (guideStep == 2 && soldCount >= 6)
        {
            ShowGuide(image2, text3);
            guideStep = 3;
        }
    }

    void OnMoneyEarned(int amount, string source)
    {
        earnedMoney += amount;

        if (guideStep == 3 && earnedMoney >= 400)
        {
            ShowGuide(image2, text4);
            guideStep = 4;
        }
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
            // 第二阶段：订单进入阶段2
            CoffeeOrderManager.Instance.SetOrderPhase(2);

        }

        if (guideStep == 3)
        {
            achievementButton.SetActive(true);
            ashadow.gameObject.SetActive(false);
            // 第三阶段：订单进入阶段3
            CoffeeOrderManager.Instance.SetOrderPhase(3);

        }

        if (guideStep == 4)
        {
            SceneTransitionManager.LoadSceneClean("Scene1");
        }
    }
}
