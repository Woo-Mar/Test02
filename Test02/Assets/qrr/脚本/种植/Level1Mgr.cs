using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level1Mgr : SingletonMono<Level1Mgr>
{
    //草莓
    public int m_CurCaoMeiNums;
    public int m_MaxCaoMeiNums;
    public Text m_CurCaoMeiNumsT;
    public Text m_MaxCaiMeiNumsT;

    //杨桃
    public int m_CurYangMeiNums;
    public int m_MaxYangMeiNums;
    public Text m_CurYangMeiNumsT;
    public Text m_MaxYangMeiNumsT;

    //无花果
    public int m_CurWuHuaGuoNums;
    public int m_MaxWuHuaGuoNums;
    public Text m_CurWuHuaGuoNumsT;
    public Text m_MaxWuHuaGuoNumsT;

    public int m_CurDZhiWuNums;
    public int m_MaxDZhiWuNums;
    public Text m_CurDZhiWuNumsT;
    public Text m_MaxDZhiWuNumsT;

    public GameObject GameWinUI;
    public GameObject GameOverUI;

    // ★★★ 枯萎失败面板和重新开始按钮 ★★★
    public GameObject witherFailPanel;      // 枯萎失败面板
    public Button restartGameButton;        // 重新开始按钮

    void Start()
    {
        // 初始隐藏失败面板
        if (witherFailPanel != null)
            witherFailPanel.SetActive(false);

        // 绑定重新开始按钮
        if (restartGameButton != null)
            restartGameButton.onClick.AddListener(RestartGame);
    }

    //增加草莓
    public void AddCaoMeiNums()
    {
        m_CurCaoMeiNums++;
        if (m_CurCaoMeiNums >= m_MaxCaoMeiNums)
        {
            m_CurCaoMeiNums = m_MaxCaoMeiNums;
            //WinLogic();
        }
    }

    //增加杨桃
    public void AddYangMeiNums()
    {
        m_CurYangMeiNums++;
        if (m_CurYangMeiNums >= m_MaxYangMeiNums)
        {
            m_CurYangMeiNums = m_MaxYangMeiNums;
            //WinLogic();
        }
    }

    //增加无花果
    public void AddWuHuaGuoNums()
    {
        m_CurWuHuaGuoNums++;
        if (m_CurWuHuaGuoNums >= m_MaxWuHuaGuoNums)
        {
            m_CurWuHuaGuoNums = m_MaxWuHuaGuoNums;
            //WinLogic();
        }
    }

    //增加枯萎数量
    public void AddDZhiWuNums()
    {
        m_CurDZhiWuNums++;
        if (m_CurDZhiWuNums >= m_MaxDZhiWuNums)
        {
            m_CurDZhiWuNums = m_MaxDZhiWuNums;

            // ★★★ 显示枯萎失败面板 ★★★
            if (witherFailPanel != null)
                witherFailPanel.SetActive(true);

            // 暂停游戏
            Time.timeScale = 0;
        }
    }

    // ★★★ 重新开始游戏的方法 ★★★
    public void RestartGame()
    {
        // 恢复时间
        Time.timeScale = 1;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Debug.Log("游戏重新开始");
    }

    //public void WinLogic()
    //{
    //    if ((m_CurCaoMeiNums == m_MaxCaoMeiNums) &&
    //        (m_CurYangMeiNums == m_MaxYangMeiNums) &&
    //        (m_CurWuHuaGuoNums == m_MaxWuHuaGuoNums))
    //    {
    //        GameWinUI.SetActive(true);
    //    }
    //}

    void Update()
    {
        //草莓
        if (m_CurCaoMeiNumsT != null)
            m_CurCaoMeiNumsT.text = m_CurCaoMeiNums.ToString();
        if (m_MaxCaiMeiNumsT != null)
            m_MaxCaiMeiNumsT.text = m_MaxCaoMeiNums.ToString();

        //杨桃 
        if (m_CurYangMeiNumsT != null)
            m_CurYangMeiNumsT.text = m_CurYangMeiNums.ToString();
        if (m_MaxYangMeiNumsT != null)
            m_MaxYangMeiNumsT.text = m_MaxYangMeiNums.ToString();

        //无花果 
        if (m_CurWuHuaGuoNumsT != null)
            m_CurWuHuaGuoNumsT.text = m_CurWuHuaGuoNums.ToString();
        if (m_MaxWuHuaGuoNumsT != null)
            m_MaxWuHuaGuoNumsT.text = m_MaxWuHuaGuoNums.ToString();

        if (m_CurDZhiWuNumsT != null)
            m_CurDZhiWuNumsT.text = m_CurDZhiWuNums.ToString();
        if (m_MaxDZhiWuNumsT != null)
            m_MaxDZhiWuNumsT.text = m_MaxDZhiWuNums.ToString();
    }
}