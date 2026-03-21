using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pressEPanel;      // 按T提示面板
    public GameObject dialoguePanel;     // 对话面板
    public Text titleText;               // 标题文本
    public Text contentText;             // 内容文本
    public Button closeButton;           // 关闭按钮

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string welcomeMessage = "欢迎来到港头村果园！我是生活在这里多年的老爷爷。";

    [TextArea(3, 5)]
    public string taskMessage = "你的任务是：\n种植草莓，杨桃，无花果作物获得更多金币。\n有了作物和金币，你就可以开咖啡馆经营啦！";

    [TextArea(3, 5)]
    public string completeMessage = "恭喜你体验到种植过程！\n现在你可以继续种植更多作物了。";

    [Header("Task Settings")]
    public Level1Mgr levelManager;        // 关卡管理器

    private bool isPlayerInRange = false;
    private bool taskStarted = false;
    private bool taskCompleted = false;

    void Start()
    {
        // 初始化UI状态
        if (pressEPanel != null)
            pressEPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 设置关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDialogue);

        // 如果levelManager为空，尝试查找
        if (levelManager == null)
            levelManager = FindObjectOfType<Level1Mgr>();

        // 修改提示面板的文字为按T
        if (pressEPanel != null)
        {
            Text panelText = pressEPanel.GetComponentInChildren<Text>();
            if (panelText != null)
                panelText.text = "T";
        }
    }

    void Update()
    {
        // 检测玩家是否在范围内并按T键
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.T))
        {
            // ★★★ 播放交互音效 ★★★
            if (AudioManager1.Instance != null && AudioManager1.Instance.interactSound != null)
            {
                AudioManager1.Instance.PlaySFX(AudioManager1.Instance.interactSound);
            }

            ShowDialogue();
        }

        // 检查任务是否完成
        if (taskStarted && !taskCompleted)
        {
            CheckTaskCompletion();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (pressEPanel != null)
                pressEPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (pressEPanel != null)
                pressEPanel.SetActive(false);

            // 离开时关闭对话
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
    }

    void ShowDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // 根据任务状态显示不同内容
        if (!taskStarted)
        {
            // 第一次对话，开始任务
            titleText.text = "港头村老爷爷";
            contentText.text = welcomeMessage + "\n\n" + taskMessage;
            taskStarted = true;
            closeButton.GetComponentInChildren<Text>().text = "知道了";
        }
        else if (taskCompleted)
        {
            // 已完成任务
            titleText.text = "港头村老爷爷";
            contentText.text = completeMessage;
            closeButton.GetComponentInChildren<Text>().text = "继续种植";
        }
        else
        {
            // 任务进行中 - 只显示简单文本
            titleText.text = "港头村老爷爷";
            contentText.text = "继续努力种植吧！完成新手任务后可以获得奖励。";
            closeButton.GetComponentInChildren<Text>().text = "继续努力";
        }
    }

    bool AllTasksCompleted()
    {
        if (levelManager == null) return false;

        return levelManager.m_CurCaoMeiNums >= levelManager.m_MaxCaoMeiNums &&
               levelManager.m_CurYangMeiNums >= levelManager.m_MaxYangMeiNums &&
               levelManager.m_CurWuHuaGuoNums >= levelManager.m_MaxWuHuaGuoNums;
    }

    void CheckTaskCompletion()
    {
        if (AllTasksCompleted())
        {
            taskCompleted = true;

            // 如果当前正在对话，更新内容
            if (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                ShowDialogue();
            }
        }
    }

    void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}