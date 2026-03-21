using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TuDiBackpack : MonoBehaviour
{
    [System.Serializable]
    public class TuDiSlot
    {
        public int landId;              // 土地ID
        public int count;                // 当前数量
        public Button slotButton;         // 购买按钮
        public Text countText;            // 显示数量的文本
    }

    [Header("土地背包格子")]
    public List<TuDiSlot> slots = new List<TuDiSlot>();

    void Start()
    {
        foreach (var slot in slots)
        {
            if (slot.countText != null)
                slot.countText.text = "数量：" + slot.count.ToString();

            if (slot.slotButton != null)
            {
                int id = slot.landId;
                slot.slotButton.onClick.AddListener(() => BuyLand(id));
            }
        }
    }

    void BuyLand(int landId)
    {
        TuDiSlot slot = slots.Find(s => s.landId == landId);
        if (slot == null || slot.count <= 0)
        {
            Debug.Log("没有土地可以购买了");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("没有土地了！");
            return;
        }

        // 调用商店系统的购买方法
        if (ShopSystem.Instance != null)
        {
            // 直接调用商店的购买逻辑
            BoZhongTrigger[] allLands = FindObjectsOfType<BoZhongTrigger>();
            bool landFound = false;

            foreach (BoZhongTrigger land in allLands)
            {
                if (land.currentState == LandState.Uncultivated)
                {
                    land.Cultivate();
                    landFound = true;
                    break;
                }
            }

            if (landFound)
            {
                slot.count--;
                if (slot.countText != null)
                    slot.countText.text = "数量：" + slot.count.ToString();

                Debug.Log($"土地购买成功！剩余{slot.count}块");

                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("土地开垦成功！");
            }
            else
            {
                Debug.Log("没有未开垦的土地了");
                BugWarningUI warning = FindObjectOfType<BugWarningUI>();
                if (warning != null)
                    warning.ShowWarning("所有土地都已开垦！");
            }
        }
    }

    public void AddLand(int landId, int amount = 1)
    {
        TuDiSlot slot = slots.Find(s => s.landId == landId);
        if (slot != null)
        {
            slot.count += amount;
            if (slot.countText != null)
                slot.countText.text = "数量：" + slot.count.ToString();
            Debug.Log($"获得{amount}块土地，现有{slot.count}块");
        }
    }
}