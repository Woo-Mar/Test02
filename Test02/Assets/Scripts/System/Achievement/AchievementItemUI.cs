using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text progressText;
    public Button claimButton;
    public TMP_Text buttonText;

    private Achievement data;
    private AchievementManager manager;

    public void Setup(Achievement achievement, AchievementManager mngr)
    {
        data = achievement;
        manager = mngr;
        RefreshUI();
    }

    public void RefreshUI()
    {
        nameText.text = data.name;
        descText.text = data.description;
        iconImage.sprite = data.icon;
        progressText.text = $"{data.currentProgress}/{data.goalValue}";

        // 按钮状态逻辑
        if (data.isClaimed)
        {
            claimButton.interactable = false;
            buttonText.text = "已领取";
        }
        else if (data.isReached)
        {
            claimButton.interactable = true;
            buttonText.text = "领取奖励";
        }
        else
        {
            claimButton.interactable = false;
            buttonText.text = "未达成";
        }

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => manager.ClaimReward(data.id));
    }
}
