using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GrowthExperienceOfficer : MonoBehaviour
{
    [Header("UI References")]
    public Button openPanelButton;             // 屏幕上的按钮
    public GameObject experiencePanel;         // 体验官面板
    public Button closeButton;                  // 关闭按钮

    [Header("面板内容")]
    public Text descriptionText;                // 描述文本

    [Header("路口设置")]
    public GameObject exitGate;                 // 出口路口对象
    public string gameSceneName = "Level3";     // 小游戏场景名称
    public float fadeDuration = 1.5f;           // 转场淡入淡出时间

    [Header("奖励设置")]
    public int rewardGold = 100;                 // 完成挑战奖励金币

    void Start()
    {
        // 初始化UI
        if (experiencePanel != null)
            experiencePanel.SetActive(false);

        // 绑定按钮事件
        if (openPanelButton != null)
            openPanelButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // 设置描述文本
        if (descriptionText != null)
        {
            descriptionText.text = "玩家可以通关寻找出口去体验作物成长，" +
                                  "完成挑战后会获得作物种子噢！";
        }

        // 检查是否有未领取的奖励
        CheckAndGiveReward();
    }
    void Update()
    {
        // 测试用：按T键加100金币
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.playerGold += 100;
                ShopSystem.Instance.UpdateGoldText();
                Debug.Log("测试：手动加了100金币");
            }
        }
    }
    void OpenPanel()
    {
        if (experiencePanel != null)
        {
            experiencePanel.SetActive(true);
            Debug.Log("打开体验官面板");
        }
    }

    void ClosePanel()
    {
        if (experiencePanel != null)
            experiencePanel.SetActive(false);
    }

    // 这个方法挂载到路口的Trigger上
    public void OnPlayerReachExit()
    {
        Debug.Log("玩家到达路口，开始转场");
        StartCoroutine(TransitionToGame());
    }

    IEnumerator TransitionToGame()
    {
        // 这里可以用渐黑效果
        // 如果有CanvasGroup可以做淡入淡出
        // 简单起见，直接加载场景
        yield return new WaitForSeconds(0.5f);

        // 保存当前场景信息
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);

        // 加载小游戏场景
        SceneManager.LoadScene(gameSceneName);
    }

    void CheckAndGiveReward()
    {
        Debug.Log("检查小游戏奖励，GameCompleted=" + PlayerPrefs.GetInt("GameCompleted", 0));

        if (PlayerPrefs.GetInt("GameCompleted", 0) == 1)
        {
            Debug.Log("找到小游戏完成标记，准备发放奖励");

            // 查找 ShopSystem
            ShopSystem shop = FindObjectOfType<ShopSystem>();
            if (shop != null)
            {
                shop.AddGold(rewardGold);  // 调用新方法
                Debug.Log($"✅ 领取小游戏奖励：{rewardGold}金币成功");
            }
            else
            {
                Debug.LogError("场景中找不到 ShopSystem！");
            }

            // 显示奖励提示
            StartCoroutine(ShowRewardMessage());

            // 清除标记
            PlayerPrefs.SetInt("GameCompleted", 0);
            PlayerPrefs.Save();
        }
    }

    IEnumerator ShowRewardMessage()
    {
        if (descriptionText != null)
        {
            string originalText = descriptionText.text;
            descriptionText.text = $"✨ 获得 {rewardGold} 金币奖励！ ✨";
            descriptionText.color = Color.yellow;

            yield return new WaitForSeconds(2f);

            descriptionText.text = originalText;
            descriptionText.color = Color.white;
        }
    }
}