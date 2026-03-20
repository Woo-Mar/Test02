using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("主控制")]
    public Button openMenuButton;
    public GameObject menuPanel;

    [Header("子系统面板")]
    public GameObject inventoryPanel;
    public GameObject purchasePanel;
    public GameObject upgradePanel;
    public GameObject achievementPanel;

    [Header("切换按钮 (Tabs)")]
    public Button inventoryBtn;
    public Button purchaseBtn;
    public Button upgradeBtn;
    public Button achievementBtn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 检查引用是否完整
        CheckReferences();

        // 2. 绑定事件
        if (openMenuButton != null) openMenuButton.onClick.AddListener(OpenMenu);

        if (inventoryBtn != null) inventoryBtn.onClick.AddListener(() => { Debug.Log("点击了库存按钮"); OpenInventory(); });
        if (purchaseBtn != null) purchaseBtn.onClick.AddListener(() => { Debug.Log("点击了采购按钮"); OpenPurchase(); });
        if (upgradeBtn != null) upgradeBtn.onClick.AddListener(() => { Debug.Log("点击了升级按钮"); OpenUpgrade(); });
        if (achievementBtn != null) achievementBtn.onClick.AddListener(() => { Debug.Log("点击了成就按钮"); OpenAchievement(); });

        // 初始关闭
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    private void CheckReferences()
    {
        if (inventoryBtn == null) Debug.LogError("MenuManager: inventoryBtn 未赋值！");
        if (purchaseBtn == null) Debug.LogError("MenuManager: purchaseBtn 未赋值！");
        if (inventoryPanel == null) Debug.LogError("MenuManager: inventoryPanel 未赋值！");
    }

    public void OpenMenu()
    {
        AudioManager.Instance.PlayButtonSound();
        Debug.Log("打开主菜单");
        menuPanel.SetActive(true);
        GameManager.Instance.SetPause(true);
        OpenInventory(); // 默认打开库存
    }

    public void CloseMenu()
    {
        AudioManager.Instance.PlayButtonSound();
        Debug.Log("关闭主菜单");
        menuPanel.SetActive(false);
        GameManager.Instance.SetPause(false);
    }

    private void HideAll()
    {
        Debug.Log("隐藏所有面板");
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (purchasePanel != null) purchasePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        AudioManager.Instance.PlayButtonSound();
        HideAll();
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        Debug.Log("库存面板已激活");
    }

    public void OpenPurchase()
    {
        AudioManager.Instance.PlayButtonSound();
        HideAll();
        if (purchasePanel != null) purchasePanel.SetActive(true);
        Debug.Log("采购面板已激活");
    }

    public void OpenUpgrade()
    {
        AudioManager.Instance.PlayButtonSound();
        HideAll();
        if (upgradePanel != null) upgradePanel.SetActive(true);
        Debug.Log("升级面板已激活");
    }

    public void OpenAchievement()
    {
        AudioManager.Instance.PlayButtonSound();
        HideAll();
        if (achievementPanel != null) achievementPanel.SetActive(true);
        Debug.Log("成就面板已激活");
    }
}
