// StartPanelController.cs - 开始面板控制器
using UnityEngine;
using UnityEngine.UI;

public class StartPanelController : MonoBehaviour
{
    [Header("UI元素")]
    public GameObject startPanel;        // 开始面板
    public Image tutorialImage;          // 教学图片
    public Button startButton;           // 开始游戏按钮

    [Header("游戏管理器引用")]
    public CoffeeOrderManager orderManager;   // 订单管理器
    public CoffeeMachine coffeeMachine;       // 咖啡机
    public GameManager gameManager;           // 游戏管理器

    [Header("设置")]
    public bool autoStart = false;       // 是否自动开始（用于测试）

    void Start()
    {
        // 初始化面板状态
        InitializePanel();

        // 如果设置为自动开始，跳过教学
        if (autoStart)
        {
            StartGame();
        }
    }

    /// <summary>
    /// 初始化开始面板
    /// </summary>
    void InitializePanel()
    {
        // 确保面板在游戏开始时显示
        if (startPanel != null)
        {
            startPanel.SetActive(true);

            // 暂停游戏逻辑
            PauseGameLogic();

            // 设置按钮点击事件
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners(); // 清除现有监听器
                startButton.onClick.AddListener(StartGame);
            }
        }
        else
        {
            Debug.LogWarning("开始面板未设置，直接开始游戏");
            StartGame();
        }
    }

    /// <summary>
    /// 暂停游戏逻辑，直到玩家点击开始
    /// </summary>
    void PauseGameLogic()
    {
        // 暂停顾客生成
        if (orderManager != null)
        {
            orderManager.StopSpawningCustomers();
        }

        // 禁用咖啡机交互
        if (coffeeMachine != null)
        {
            coffeeMachine.enabled = false;
        }

        // 暂停其他需要暂停的系统...
        Debug.Log("游戏暂停，等待玩家点击开始");
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        Debug.Log("开始游戏");

        // 隐藏开始面板
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // 恢复游戏逻辑
        ResumeGameLogic();

        // 可选：播放开始音效
        // AudioManager.Instance.PlaySound("gameStart");
    }

    /// <summary>
    /// 恢复游戏逻辑
    /// </summary>
    void ResumeGameLogic()
    {
        // 恢复顾客生成
        if (orderManager != null)
        {
            orderManager.ResumeSpawningCustomers();
        }

        // 启用咖啡机交互
        if (coffeeMachine != null)
        {
            coffeeMachine.enabled = true;
        }

        // 恢复其他系统...
        Debug.Log("游戏开始，所有系统已激活");
    }

    /// <summary>
    /// 显示开始面板（可用于暂停菜单）
    /// </summary>
    public void ShowStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            PauseGameLogic();
        }
    }
}