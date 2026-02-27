using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Button actionButton;
    public TMP_Text buttonText;

    private int upgradeIndex;
    private int cost;
    private UpgradeManager manager;
    private bool isUnlocked;

    public void Setup(int index, string name, int c, string description, Sprite icon, bool unlocked, UpgradeManager mngr)
    {
        upgradeIndex = index;
        nameText.text = name;
        cost = c;
        if (descriptionText != null)
            descriptionText.text = description;
        iconImage.sprite = icon;
        isUnlocked = unlocked;
        manager = mngr;

        UpdateUI();

        actionButton.onClick.RemoveAllListeners();
        // 【修改1】把自身（this）传递给管理器，方便成功后更新UI状态
        actionButton.onClick.AddListener(() => manager.TryUpgrade(upgradeIndex, cost, this));
    }

    public void UpdateUI()
    {
        if (isUnlocked)
        {
            buttonText.text = "已解锁";
            actionButton.interactable = false; // 控制按钮不可再次点击
            costText.text = "MAX";
        }
        else
        {
            buttonText.text = "升级";
            actionButton.interactable = true;
            costText.text = cost + " 金币";
        }
    }

    public void SetUnlocked()
    {
        isUnlocked = true;
        UpdateUI();
    }
}