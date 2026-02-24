using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text costText;
    public Button actionButton;
    public TMP_Text buttonText;

    private int upgradeIndex;
    private int cost;
    private UpgradeManager manager;
    private bool isUnlocked;

    public void Setup(int index, string name, int c, Sprite icon, bool unlocked, UpgradeManager mngr)
    {
        upgradeIndex = index;
        nameText.text = name;
        cost = c;
        iconImage.sprite = icon;
        isUnlocked = unlocked;
        manager = mngr;

        UpdateUI();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => manager.TryUpgrade(upgradeIndex, cost));
    }

    public void UpdateUI()
    {
        if (isUnlocked)
        {
            buttonText.text = "已解锁";
            actionButton.interactable = false;
            costText.text = "MAX";
        }
        else
        {
            buttonText.text = "解锁";
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
