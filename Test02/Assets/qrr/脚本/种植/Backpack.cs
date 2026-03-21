using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Backpack : MonoBehaviour
{
    [System.Serializable]
    public class SeedSlot
    {
        public int seedId;              // 种子ID (0:草莓,1:杨桃,2:无花果)
        public int count;                // 当前数量
        public Button slotButton;         // 整个格子的按钮
        public Text countText;            // 显示数量的文本
    }

    [System.Serializable]
    public class FertilizerSlot
    {
        public int fertilizerId;          // 肥料ID (0:有机肥料)
        public int count;                  // 当前数量
        public Button slotButton;           // 整个格子的按钮
        public Text countText;              // 显示数量的文本
    }

    [Header("种子格子")]
    public List<SeedSlot> slots = new List<SeedSlot>();  // 草莓、杨桃、无花果

    [Header("有机肥料格子")]
    public FertilizerSlot organicFertilizerSlot;

    [Header("按钮")]
    public Button jiaoShuiButton;
    public Button shaChongButton;
    public Button normalFertilizerButton;
    public Button chanChuButton;  // ★★★ 新增铲除按钮 ★★★

    [Header("玩家引用")]
    public PlayZhongZhiC playerPlanting;

    void Start()
    {
        // ★★★ 先读取奖励种子 ★★★
        int rewardStrawberry = PlayerPrefs.GetInt("RewardSeeds_Strawberry", 0);
        int rewardCarambola = PlayerPrefs.GetInt("RewardSeeds_Carambola", 0);
        int rewardFig = PlayerPrefs.GetInt("RewardSeeds_Fig", 0);

        if (rewardStrawberry > 0 || rewardCarambola > 0 || rewardFig > 0)
        {
            // 添加奖励种子
            AddSeed(0, rewardStrawberry);
            AddSeed(1, rewardCarambola);
            AddSeed(2, rewardFig);

            // 清除奖励标记
            PlayerPrefs.SetInt("RewardSeeds_Strawberry", 0);
            PlayerPrefs.SetInt("RewardSeeds_Carambola", 0);
            PlayerPrefs.SetInt("RewardSeeds_Fig", 0);
            PlayerPrefs.Save();

            Debug.Log($"✅ 获得奖励种子：草莓+{rewardStrawberry}，杨桃+{rewardCarambola}，无花果+{rewardFig}");

            // 显示提示
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning($"✨ 获得奖励种子！草莓+{rewardStrawberry}，杨桃+{rewardCarambola}，无花果+{rewardFig} ✨");
        }

        // 原有的初始化代码...
        foreach (var slot in slots)
        {
            if (slot.countText != null)
                slot.countText.text = "数量：" + slot.count.ToString();

            if (slot.slotButton != null)
            {
                int id = slot.seedId;
                slot.slotButton.onClick.AddListener(() => UseSeed(id));
            }
        }

        // 初始化有机肥料格子
        if (organicFertilizerSlot != null)
        {
            if (organicFertilizerSlot.countText != null)
                organicFertilizerSlot.countText.text = "数量：" + organicFertilizerSlot.count.ToString();

            if (organicFertilizerSlot.slotButton != null)
            {
                organicFertilizerSlot.slotButton.onClick.AddListener(UseOrganicFertilizer);
            }
        }

        // 初始化按钮
        if (jiaoShuiButton != null)
            jiaoShuiButton.onClick.AddListener(UseJiaoShui);

        if (shaChongButton != null)
            shaChongButton.onClick.AddListener(UseShaChong);

        if (normalFertilizerButton != null)
            normalFertilizerButton.onClick.AddListener(UseNormalFertilizer);

        if (chanChuButton != null)
            chanChuButton.onClick.AddListener(UseChanChu);
    }

    void UseSeed(int seedId)
    {
        SeedSlot slot = slots.Find(s => s.seedId == seedId);
        if (slot == null) return;

        if (slot.count > 0)
        {
            // 检查玩家
            if (playerPlanting == null)
            {
                Debug.Log("找不到玩家种植脚本！");
                return;
            }

            // 检查是否站在土地上
            if (playerPlanting.m_CurBoZhongTrigger == null)
            {
                Debug.Log("请先站在土地上！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先站在土地上");
                return;
            }

            BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;

            // ★★★ 检查土地是否已开垦 ★★★
            if (currentLand.currentState != LandState.Cultivated)
            {
                Debug.Log($"土地未开垦，不能种植！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地未开垦！请先购买");
                return;  // 直接返回，不扣除种子
            }

            // 检查玩家是否真的站在土地上
            if (!currentLand.IsEnter)
            {
                Debug.Log("玩家没有站在土地上！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先站在土地上");
                return;
            }

            // 检查杂草
            if (currentLand.hasWeed)
            {
                Debug.Log("土地上有杂草，不能种植！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先铲除杂草！");
                return;
            }

            // 检查休耕
            LandManager landManager = currentLand.GetComponent<LandManager>();
            if (landManager != null && landManager.isFallow)
            {
                Debug.Log("土地正在休耕，不能种植！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地正在休耕！");
                return;
            }

            // 所有检查通过，执行播种
            playerPlanting.BoZhong(seedId);

            if (AudioManager1.Instance != null && AudioManager1.Instance.plantSeed != null)
            {
                AudioManager1.Instance.PlaySFX(AudioManager1.Instance.plantSeed);
            }

            slot.count--;
            if (slot.countText != null)
                slot.countText.text = "数量：" + slot.count.ToString();

            Debug.Log($"种植成功，剩余{slot.count}个");
        }
        else
        {
            Debug.Log("没有种子了！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("没有种子了");
        }
    }
    // ★★★ 新增铲除方法 ★★★
    // ★★★ 新增铲除方法 ★★★
    void UseChanChu()
    {
        if (playerPlanting == null)
        {
            Debug.Log("找不到玩家种植脚本！");
            return;
        }

        if (playerPlanting.m_CurBoZhongTrigger == null)
        {
            Debug.Log("请先站在土地上！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
            return;
        }

        BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;

        if (currentLand.hasWeed)
        {
            currentLand.RemoveWeed();
            Debug.Log("铲除杂草成功");
        }
        else
        {
            Debug.Log("这块地没有杂草");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("没有杂草可铲除");
        }
    }

    void UseOrganicFertilizer()
    {
        if (organicFertilizerSlot.count > 0)
        {
            if (playerPlanting != null && playerPlanting.m_CurBoZhongTrigger != null)
            {
                BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;
                if (currentLand.currentState != LandState.Cultivated)
                {
                    Debug.Log("土地未开垦，不能使用肥料！");
                    BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                    if (warning != null)
                        warning.ShowWarning("土地未开垦！");
                    return;
                }

                // 检查是否有杂草
                if (currentLand.hasWeed)
                {
                    Debug.Log("土地上有杂草，不能使用肥料！");
                    BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                    if (warning != null)
                        warning.ShowWarning("请先铲除杂草！");
                    return;
                }

                playerPlanting.UseOrganicFertilizer();

                organicFertilizerSlot.count--;
                if (organicFertilizerSlot.countText != null)
                    organicFertilizerSlot.countText.text = "数量：" + organicFertilizerSlot.count.ToString();

                Debug.Log($"使用有机肥料成功，剩余{organicFertilizerSlot.count}个");
            }
            else
            {
                Debug.Log("请先站在土地上！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先站在土地上");
            }
        }
        else
        {
            Debug.Log("没有有机肥料了！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("没有有机肥料了");
        }
    }

    void UseJiaoShui()
    {
        if (playerPlanting != null && playerPlanting.m_CurBoZhongTrigger != null)
        {
            BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;
            if (currentLand.currentState != LandState.Cultivated)
            {
                Debug.Log("土地未开垦，不能浇水！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地未开垦！");
                return;
            }

            // 检查是否有杂草
            if (currentLand.hasWeed)
            {
                Debug.Log("土地上有杂草，不能浇水！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先铲除杂草！");
                return;
            }

            playerPlanting.JiaoShui();
            Debug.Log("使用浇水");
        }
        else
        {
            Debug.Log("请先站在土地上！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
        }
    }

    void UseShaChong()
    {
        if (playerPlanting != null && playerPlanting.m_CurBoZhongTrigger != null)
        {
            BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;
            if (currentLand.currentState != LandState.Cultivated)
            {
                Debug.Log("土地未开垦，不能杀虫！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地未开垦！");
                return;
            }

            // 检查是否有杂草
            if (currentLand.hasWeed)
            {
                Debug.Log("土地上有杂草，不能杀虫！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先铲除杂草！");
                return;
            }

            playerPlanting.ShaChong();
            Debug.Log("使用杀虫");
        }
        else
        {
            Debug.Log("请先站在土地上！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
        }
    }

    void UseNormalFertilizer()
    {
        if (playerPlanting != null && playerPlanting.m_CurBoZhongTrigger != null)
        {
            BoZhongTrigger currentLand = playerPlanting.m_CurBoZhongTrigger;
            if (currentLand.currentState != LandState.Cultivated)
            {
                Debug.Log("土地未开垦，不能使用肥料！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地未开垦！");
                return;
            }

            // 检查是否有杂草
            if (currentLand.hasWeed)
            {
                Debug.Log("土地上有杂草，不能使用肥料！");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("请先铲除杂草！");
                return;
            }

            playerPlanting.UseNormalFertilizer();
            Debug.Log("使用普通肥料");
        }
        else
        {
            Debug.Log("请先站在土地上！");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
        }
    }

    public void AddSeed(int seedId, int amount = 1)
    {
        SeedSlot slot = slots.Find(s => s.seedId == seedId);
        if (slot != null)
        {
            slot.count += amount;
            if (slot.countText != null)
                slot.countText.text = "数量：" + slot.count.ToString();
            Debug.Log($"获得{amount}个种子，现有{slot.count}个");
        }
    }

    public void AddOrganicFertilizer(int amount = 1)
    {
        if (organicFertilizerSlot != null)
        {
            organicFertilizerSlot.count += amount;
            if (organicFertilizerSlot.countText != null)
                organicFertilizerSlot.countText.text = "数量：" + organicFertilizerSlot.count.ToString();

            Debug.Log($"✅ 获得{amount}个有机肥料，现有{organicFertilizerSlot.count}个");
        }
        else
        {
            Debug.LogError("❌ organicFertilizerSlot 为空！");
        }
    }
    public void GiveGameReward()
    {
        AddSeed(0, 1);  // 草莓
        AddSeed(1, 1);  // 杨桃
        AddSeed(2, 1);  // 无花果

        Debug.Log("✅ 获得游戏奖励：草莓+1，杨桃+1，无花果+1");

        BugWarningUI warning = FindObjectOfType<BugWarningUI>();
        if (warning != null)
            warning.ShowWarning("✨ 获得奖励种子！草莓+1，杨桃+1，无花果+1 ✨");
    }
}