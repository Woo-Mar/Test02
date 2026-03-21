using UnityEngine;
using UnityEngine.UI;

public class ButtonPanelManager : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject infoPanel;        // 要弹出的面板
    public Text panelText;              // 面板中的文字
    public Button closeButton;          // 关闭按钮

    [Header("按钮")]
    public Button openButton;           // 打开面板的按钮

    [Header("文字内容")]
    [TextArea(3, 5)]
    public string displayText = "这里是显示的文字内容";

    void Start()
    {
        // 初始隐藏面板
        if (infoPanel != null)
            infoPanel.SetActive(false);

        // 绑定打开按钮
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        // 绑定关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    void OpenPanel()
    {
        if (infoPanel != null)
        {
            // 设置文字
            if (panelText != null)
                panelText.text = displayText;

            infoPanel.SetActive(true);
            Debug.Log("打开信息面板");
        }
    }

    void ClosePanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
        Debug.Log("关闭信息面板");
    }
}