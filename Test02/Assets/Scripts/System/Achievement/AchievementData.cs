using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string name;
    public string description;
    public Sprite icon;
    public int goalValue;      // 目标数值（如10份，1000金币）
    public int rewardMoney;    // 奖励金额

    [Header("运行时状态")]
    public int currentProgress; // 当前进度
    public bool isReached;      // 是否达成目标
    public bool isClaimed;      // 是否已领取奖励
}
