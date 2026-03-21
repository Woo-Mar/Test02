using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum E_State
{
    CreateLettersState,   // 出字符阶段
    InPutState,           // 输入阶段
}

public class Level3Mgr : SingletonMono<Level3Mgr>
{
    // 方向键列表
    private List<string> m_RecordDirections = new List<string>();
    private string[] m_DirectionSymbols = { "↑", "↓", "←", "→" };

    // 当前阶段
    public E_State m_CurState;

    public List<Text> m_RecordDirectionsT;  // 显示方向的Text（↑↓←→）
    public List<Toggle> m_Toogles;
    private int m_Index;

    private float m_CurTime;
    public Image timebar;

    public int m_CurWrongNums;
    public int m_MaxWrongNums;

    public Text m_CurWrongNumsT;
    public Text m_MaxWorngNumsT;

    public int m_CurChengZhangValue;
    public int m_MaxChengZhangValue;

    public Image m_ChengZhangBar;
    public GameObject GameOverUI;
    public GameObject GameWinUI;

    // 按钮
    public Button restartButton;      // 失败时的重新开始按钮
    public Button backToMainButton;   // 成功时返回主场景的按钮

    [Header("返回场景")]
    public string mainSceneName = "Game";  // 主场景名称

    // 农作物 - 这里有3个植物对象，代表3个生长阶段
    public List<GameObject> NongZuoWuList; // 索引0=幼苗, 1=生长中, 2=成熟

    void Start()
    {
        // 初始化：只显示幼苗（阶段0），隐藏其他阶段
        ShowGrowthStage(0);

        // 绑定重新开始按钮
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }

        // 绑定返回主场景按钮
        if (backToMainButton != null)
        {
            backToMainButton.onClick.AddListener(BackToMainScene);
            backToMainButton.gameObject.SetActive(false);
        }

        // 初始隐藏失败面板和胜利面板
        if (GameOverUI != null)
            GameOverUI.SetActive(false);
        if (GameWinUI != null)
            GameWinUI.SetActive(false);
    }

    /// <summary>
    /// 显示指定的生长阶段，隐藏其他阶段
    /// </summary>
    public void ShowGrowthStage(int stageIndex)
    {
        for (int i = 0; i < NongZuoWuList.Count; i++)
        {
            if (NongZuoWuList[i] != null)
            {
                bool shouldShow = (i == stageIndex);
                NongZuoWuList[i].SetActive(shouldShow);
            }
        }
        Debug.Log($"切换到生长阶段 {stageIndex}");
    }

    // 直接切换阶段
    public void GroupUp(int _value)
    {
        ShowGrowthStage(_value);
    }

    // 增加错误次数
    public void AddWrongNums()
    {
        m_CurWrongNums++;
        if (m_CurWrongNums >= m_MaxWrongNums)
        {
            m_CurWrongNums = m_MaxWrongNums;
            GameOverUI.SetActive(true);

            if (restartButton != null)
                restartButton.gameObject.SetActive(true);
        }
    }

    public void AddChengZhangValue(int _value)
    {
        int oldValue = m_CurChengZhangValue;
        m_CurChengZhangValue += _value;
        m_CurChengZhangValue = Mathf.Clamp(m_CurChengZhangValue, 0, m_MaxChengZhangValue);

        // 检查是否达到新的生长阶段
        if (oldValue < 50 && m_CurChengZhangValue >= 50)
        {
            GroupUp(1);
        }
        else if (oldValue < 60 && m_CurChengZhangValue >= 60)
        {
            GroupUp(2);
        }

        if (m_CurChengZhangValue >= m_MaxChengZhangValue)
        {
            m_CurChengZhangValue = m_MaxChengZhangValue;
            Invoke("GameWin", 1.5f);
        }
    }

    // 创建方向键序列
    public void CreateDirectionSequence()
    {
        m_RecordDirections.Clear();

        for (int i = 0; i < m_Toogles.Count; i++)
        {
            m_Toogles[i].isOn = false;
        }

        List<string> randomDirections = GetRandomDirections(5);

        for (int i = 0; i < randomDirections.Count; i++)
        {
            m_RecordDirections.Add(randomDirections[i]);
        }

        for (int i = 0; i < m_RecordDirectionsT.Count; i++)
        {
            m_RecordDirectionsT[i].text = m_RecordDirections[i];
        }
    }

    void Update()
    {
        if (m_CurWrongNumsT != null)
            m_CurWrongNumsT.text = m_CurWrongNums.ToString();
        if (m_MaxWorngNumsT != null)
            m_MaxWorngNumsT.text = m_MaxWrongNums.ToString();

        switch (m_CurState)
        {
            case E_State.CreateLettersState:
                CreateDirectionSequence();
                m_CurState = E_State.InPutState;
                break;

            case E_State.InPutState:
                HandleDirectionInput();

                m_CurTime += Time.deltaTime;
                if (m_CurTime >= 6)
                {
                    m_CurTime = 0;
                    m_CurState = E_State.CreateLettersState;
                    m_Index = 0;
                    AddWrongNums();
                }
                break;
        }

        if (timebar != null)
            timebar.fillAmount = (float)m_CurTime / 6.0f;

        if (m_ChengZhangBar != null)
            m_ChengZhangBar.fillAmount = (float)m_CurChengZhangValue / m_MaxChengZhangValue;
    }

    private void HandleDirectionInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CheckDirection("↑");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CheckDirection("↓");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckDirection("←");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckDirection("→");
        }
    }

    private void CheckDirection(string inputDir)
    {
        if (m_Index >= m_RecordDirections.Count) return;

        if (m_RecordDirections[m_Index] == inputDir)
        {
            m_Toogles[m_Index].isOn = true;
            m_Index++;

            if (m_Index >= m_RecordDirections.Count)
            {
                m_CurState = E_State.CreateLettersState;
                m_Index = 0;
                AddChengZhangValue(5);
                m_CurTime = 0;
            }
        }
        else
        {
            AddWrongNums();
            m_CurState = E_State.CreateLettersState;
            m_Index = 0;
        }
    }

    public List<string> GetRandomDirections(int count)
    {
        List<string> result = new List<string>();
        System.Random random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = random.Next(0, m_DirectionSymbols.Length);
            result.Add(m_DirectionSymbols[randomIndex]);
        }

        return result;
    }

    // ★★★ 游戏胜利时调用 ★★★
    public void GameWin()
    {
        GameWinUI.SetActive(true);

        if (backToMainButton != null)
            backToMainButton.gameObject.SetActive(true);

        PlayerPrefs.SetInt("HasGameReward", 1);
        PlayerPrefs.Save();
        Debug.Log("游戏完成，奖励标记已保存");
    }

    // ★★★ 添加奖励种子的方法 ★★★
    void AddRewardSeeds()
    {
        // 查找背包
        Backpack backpack = FindObjectOfType<Backpack>();
        if (backpack != null)
        {
            // 草莓种子 +1 (ID=0)
            backpack.AddSeed(0, 1);
            // 杨桃种子 +1 (ID=1)
            backpack.AddSeed(1, 1);
            // 无花果种子 +1 (ID=2)
            backpack.AddSeed(2, 1);

            Debug.Log("✅ 获得奖励：草莓+1，杨桃+1，无花果+1");

            // 显示提示
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("✨ 获得奖励种子！草莓+1，杨桃+1，无花果+1 ✨");
        }
        else
        {
            Debug.LogError("找不到背包，无法发放奖励");
        }
    }

    // 重新开始游戏的方法
    public void RestartGame()
    {
        // 重置所有数据
        m_CurWrongNums = 0;
        m_CurChengZhangValue = 0;
        m_CurState = E_State.CreateLettersState;
        m_Index = 0;
        m_CurTime = 0;

        ShowGrowthStage(0);

        for (int i = 0; i < m_Toogles.Count; i++)
        {
            if (m_Toogles[i] != null)
                m_Toogles[i].isOn = false;
        }

        m_RecordDirections.Clear();

        if (GameOverUI != null)
            GameOverUI.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        CreateDirectionSequence();
        m_CurState = E_State.InPutState;

        Debug.Log("游戏重新开始");
    }

    // 返回主场景的方法
    public void BackToMainScene()
    {
        Debug.Log($"返回主场景: {mainSceneName}");
        SceneManager.LoadScene(mainSceneName);
    }
}