using UnityEngine;

public class RewardManager : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("HasGameReward", 0) == 1)
        {
            Backpack backpack = FindObjectOfType<Backpack>();
            if (backpack != null)
            {
                backpack.GiveGameReward();
                PlayerPrefs.SetInt("HasGameReward", 0);
                PlayerPrefs.Save();
                Debug.Log("½±ÀøÒÑ·¢·Å");
            }
        }
    }
}