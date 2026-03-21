using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    [System.Serializable]
    public class ShopSeed
    {
        public int seedId;
        public string seedName;
        public int price;
        public Sprite icon;
        public Button buyButton;
    }

    [System.Serializable]
    public class ShopLand
    {
        public int landId;
        public string landName;
        public int price;
        public Sprite icon;
        public Button buyButton;
    }

    [System.Serializable]
    public class ShopFertilizer
    {
        public int fertilizerId;
        public string fertilizerName;
        public int price;
        public Sprite icon;
        public Button buyButton;
    }

    [Header("UI References")]
    public GameObject shopPanel;
    public Button closeButton;

    [Header("金币显示")]
    public Text goldTextUI;
    public Text shopGoldText;

    [Header("商店种子")]
    public List<ShopSeed> seedsForSale = new List<ShopSeed>();

    [Header("商店土地")]
    public List<ShopLand> landsForSale = new List<ShopLand>();

    [Header("商店有机肥料")]
    public ShopFertilizer organicFertilizer;

    [Header("系统引用")]
    public Backpack seedBackpack;

    [Header("玩家金币")]
    public int playerGold = 60;

    void Awake()
    {
        Instance = this;
        LoadGold();
    }

    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        InitializeSeedButtons();
        InitializeLandButtons();
        InitializeFertilizerButton();

        // ★★★ 读取奖励金币 ★★★
        int rewardGold = PlayerPrefs.GetInt("RewardGold", 0);
        if (rewardGold > 0)
        {
            playerGold += rewardGold;
            PlayerPrefs.SetInt("RewardGold", 0);
            PlayerPrefs.Save();
            Debug.Log($"✅ 领取奖励金币：{rewardGold}，当前金币：{playerGold}");
        }

        UpdateGoldText();
    }

    // ★★★ 鼠标点击商店精灵时调用 ★★★
    void OnMouseDown()
    {
        Debug.Log("点击商店精灵");
        OpenShop();
    }

    void LoadGold()
    {
        // 强制重置金币为100，忽略保存的数据
        playerGold = 60;
        SaveGold();  // 保存新数据

        // 或者用这行读取保存的数据
        // playerGold = PlayerPrefs.GetInt("PlayerGold", 100);
    }

    void SaveGold()
    {
        PlayerPrefs.SetInt("PlayerGold", playerGold);
        PlayerPrefs.Save();
    }

    void InitializeSeedButtons()
    {
        foreach (var seed in seedsForSale)
        {
            if (seed.buyButton != null)
            {
                seed.buyButton.onClick.RemoveAllListeners();
                int seedId = seed.seedId;
                seed.buyButton.onClick.AddListener(() => BuySeed(seedId));
            }
        }
    }

    void InitializeLandButtons()
    {
        foreach (var land in landsForSale)
        {
            if (land.buyButton != null)
            {
                land.buyButton.onClick.RemoveAllListeners();
                int landId = land.landId;
                land.buyButton.onClick.AddListener(() => BuyLand(landId));
            }
        }
    }

    void InitializeFertilizerButton()
    {
        if (organicFertilizer != null && organicFertilizer.buyButton != null)
        {
            organicFertilizer.buyButton.onClick.RemoveAllListeners();
            organicFertilizer.buyButton.onClick.AddListener(BuyOrganicFertilizer);
        }
    }

    void BuySeed(int seedId)
    {
        ShopSeed seed = seedsForSale.Find(s => s.seedId == seedId);
        if (seed == null) return;

        if (playerGold >= seed.price)
        {
            playerGold -= seed.price;

            if (seedBackpack != null)
            {
                seedBackpack.AddSeed(seedId, 1);
            }

            SaveGold();
            UpdateGoldText();
        }
        else
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("金币不足！");
        }
    }

    void BuyLand(int landId)
    {
        ShopLand land = landsForSale.Find(l => l.landId == landId);
        if (land == null) return;

        if (playerGold >= land.price)
        {
            BoZhongTrigger[] allLands = FindObjectsOfType<BoZhongTrigger>();
            bool landFound = false;

            foreach (BoZhongTrigger landTrigger in allLands)
            {
                if (landTrigger.currentState == LandState.Uncultivated)
                {
                    landTrigger.Cultivate();
                    landFound = true;
                    break;
                }
            }

            if (!landFound)
            {
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("所有土地都已开垦！");
                return;
            }

            playerGold -= land.price;
            SaveGold();
            UpdateGoldText();

            BugWarningUI warning2 = FindObjectOfType<BugWarningUI>();
            if (warning2 != null)
                warning2.ShowWarning("购买成功！新土地已开垦");
        }
        else
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("金币不足！");
        }
    }

    void BuyOrganicFertilizer()
    {
        if (organicFertilizer == null) return;

        if (playerGold >= organicFertilizer.price)
        {
            playerGold -= organicFertilizer.price;

            if (seedBackpack != null)
            {
                seedBackpack.AddOrganicFertilizer(1);
            }

            SaveGold();
            UpdateGoldText();
        }
        else
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("金币不足！");
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateGoldText();
            Debug.Log("打开商店面板");
        }
    }

    void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void UpdateGoldText()
    {
        if (goldTextUI != null)
            goldTextUI.text = "金币: " + playerGold;

        if (shopGoldText != null)
            shopGoldText.text = "金币: " + playerGold;
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        UpdateGoldText();
        SaveGold();
        Debug.Log($"增加 {amount} 金币，当前金币：{playerGold}");
    }
}