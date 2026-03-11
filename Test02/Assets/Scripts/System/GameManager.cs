// GameManager.cs - 游戏管理
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;


/// <summary>
/// 游戏管理器 - 管理游戏状态、金币系统和UI更新
/// 单例模式，全局游戏控制中心
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 单例实例
    public bool isPaused = false; // 记录当前是否暂停

    [Header("UI")]
    public TMP_Text moneyText;             // 金币显示文本
    public TMP_Text customerCountText;     // 顾客数量显示文本

    [Header("Game Data")]
    public int money = 100;            // 当前金币数量

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateUI(); // 初始化UI显示
                    // 触发游戏开始事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerGameStarted();
        }
    }

    /// <summary>
    /// 增加金币
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void AddMoney(int amount, string source = "unknown")
    {
        money += amount;
        // 触发事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerMoneyEarned(amount, source);
        }
        UpdateUI(); // 更新UI显示
    }

    /// <summary>
    /// 消费金币
    /// </summary>
    /// <param name="amount">消费数量</param>
    /// <returns>消费是否成功</returns>
    public bool SpendMoney(int amount, string purpose = "unknown")
    {
        // 检查金币是否足够
        if (money >= amount)
        {
            money -= amount;

            // 触发事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerMoneySpent(amount, purpose);
            }

            UpdateUI(); // 更新UI
            return true; // 消费成功
        }
        return false; // 金币不足，消费失败
    }

    /// <summary>
    /// 更新游戏UI显示
    /// </summary>
    void UpdateUI()
    {
        // 更新金币显示
        if (moneyText != null)
            moneyText.text = $"{money}";

        // 更新顾客数量显示
        if (customerCountText != null)
        {
            int customerCount = CoffeeOrderManager.Instance.waitingCustomers.Count;
            customerCountText.text = $"{customerCount}";
        }
    }
    /// <summary>
    /// 切换暂停状态
    /// </summary>
    // GameManager.cs
    public void SetPause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = isPaused ? 0f : 1f;

        if (EventManager.Instance != null)
        {
            if (isPaused)
                EventManager.Instance.TriggerGamePaused();
            else
                EventManager.Instance.TriggerGameResumed();
        }
        Debug.Log(isPaused ? "游戏已暂停" : "游戏已恢复");
    }


}