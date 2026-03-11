using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("入口按钮")]
    public Button openMenuButton;

    [Header("主菜单")]
    public GameObject menuPanel;

    [Header("系统面板")]
    public GameObject inventoryPanel;
    public GameObject purchasePanel;
    public GameObject upgradePanel;
    public GameObject achievementPanel;

    [Header("切换按钮")]
    public Button inventoryButton;
    public Button purchaseButton;
    public Button upgradeButton;
    public Button achievementButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        menuPanel.SetActive(false);

        openMenuButton.onClick.AddListener(OpenMenu);

        inventoryButton.onClick.AddListener(OpenInventory);
        purchaseButton.onClick.AddListener(OpenPurchase);
        upgradeButton.onClick.AddListener(OpenUpgrade);
        achievementButton.onClick.AddListener(OpenAchievement);
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);

        GameManager.Instance.SetPause(true);

        // 默认打开
        OpenInventory();
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);

        GameManager.Instance.SetPause(false);
    }

    void HideAll()
    {
        inventoryPanel.SetActive(false);
        purchasePanel.SetActive(false);
        upgradePanel.SetActive(false);
        achievementPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        HideAll();
        inventoryPanel.SetActive(true);
    }

    public void OpenPurchase()
    {
        HideAll();
        purchasePanel.SetActive(true);
    }

    public void OpenUpgrade()
    {
        HideAll();
        upgradePanel.SetActive(true);
    }

    public void OpenAchievement()
    {
        HideAll();
        achievementPanel.SetActive(true);
    }
}
