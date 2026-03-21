using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CafeSystem : MonoBehaviour
{
    public static CafeSystem Instance { get; private set; }

    [Header("UI面板")]
    public GameObject cafeMainPanel;

    [Header("金币显示")]
    public Text playerGoldText;

    [Header("作物库存显示")]
    public Text strawberryCountText;
    public Text carambolaCountText;
    public Text figCountText;

    [Header("作物售价")]
    public int strawberryPrice = 10;
    public int carambolaPrice = 20;
    public int figPrice = 30;

    [Header("出售按钮")]
    public Button sellStrawberryButton;
    public Button sellCarambolaButton;
    public Button sellFigButton;
    public Button closeButton;

    [Header("提示文本")]
    public Text messageText;
    public float messageDuration = 2f;

    private int strawberryCount = 0;
    private int carambolaCount = 0;
    private int figCount = 0;
    private Coroutine messageCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (cafeMainPanel != null)
            cafeMainPanel.SetActive(false);

        // 绑定关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCafe);

        // 绑定出售按钮
        if (sellStrawberryButton != null)
            sellStrawberryButton.onClick.AddListener(() => SellCrop(0));

        if (sellCarambolaButton != null)
            sellCarambolaButton.onClick.AddListener(() => SellCrop(1));

        if (sellFigButton != null)
            sellFigButton.onClick.AddListener(() => SellCrop(2));

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        UpdateUI();
    }

    // ★★★ 鼠标点击咖啡馆精灵时调用 ★★★
    void OnMouseDown()
    {
        // 如果鼠标在 UI 上，则忽略本次点击
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Debug.Log("=== OnMouseDown 被触发 ===");
        Debug.Log($"游戏对象: {gameObject.name}");
        Debug.Log($"位置: {transform.position}");
        OpenCafe();
    }

    public void OpenCafe()
    {
        if (cafeMainPanel != null)
        {
            cafeMainPanel.SetActive(true);
            UpdateUI();
            Debug.Log("打开咖啡馆面板");
        }
        else
        {
            Debug.LogError("cafeMainPanel 为空！");
        }
    }

    public void CloseCafe()
    {
        if (cafeMainPanel != null)
            cafeMainPanel.SetActive(false);
    }

    void SellCrop(int cropId)
    {
        int price = 0;
        int currentCount = 0;

        switch (cropId)
        {
            case 0:
                price = strawberryPrice;
                currentCount = strawberryCount;
                break;
            case 1:
                price = carambolaPrice;
                currentCount = carambolaCount;
                break;
            case 2:
                price = figPrice;
                currentCount = figCount;
                break;
        }

        if (currentCount <= 0)
        {
            ShowMessage("没有作物可以出售！");
            return;
        }

        switch (cropId)
        {
            case 0:
                strawberryCount--;
                if (Level1Mgr.GetInstance() != null && Level1Mgr.GetInstance().m_CurCaoMeiNums > 0)
                {
                    Level1Mgr.GetInstance().m_CurCaoMeiNums--;
                }
                break;
            case 1:
                carambolaCount--;
                if (Level1Mgr.GetInstance() != null && Level1Mgr.GetInstance().m_CurYangMeiNums > 0)
                {
                    Level1Mgr.GetInstance().m_CurYangMeiNums--;
                }
                break;
            case 2:
                figCount--;
                if (Level1Mgr.GetInstance() != null && Level1Mgr.GetInstance().m_CurWuHuaGuoNums > 0)
                {
                    Level1Mgr.GetInstance().m_CurWuHuaGuoNums--;
                }
                break;
        }

        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.playerGold += price;
            ShopSystem.Instance.UpdateGoldText();
        }

        ShowMessage($"出售成功！获得{price}金币");
        UpdateUI();
    }

    public void AddCrop(int cropId)
    {
        switch (cropId)
        {
            case 0:
                strawberryCount++;
                break;
            case 1:
                carambolaCount++;
                break;
            case 2:
                figCount++;
                break;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (strawberryCountText != null)
            strawberryCountText.text = "x " + strawberryCount.ToString();

        if (carambolaCountText != null)
            carambolaCountText.text = "x " + carambolaCount.ToString();

        if (figCountText != null)
            figCountText.text = "x " + figCount.ToString();

        if (playerGoldText != null && ShopSystem.Instance != null)
            playerGoldText.text = "金币: " + ShopSystem.Instance.playerGold.ToString();
    }

    void ShowMessage(string msg)
    {
        if (messageText == null) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageCoroutine(msg));
    }

    IEnumerator ShowMessageCoroutine(string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}