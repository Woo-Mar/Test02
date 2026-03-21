using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayZhongZhiC : MonoBehaviour
{
    public BoZhongTrigger m_CurBoZhongTrigger;

    void Update()
    {
        // 每帧检测玩家脚下的土地
        DetectCurrentLand();

        // 按F1查看当前土地信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (m_CurBoZhongTrigger != null)
            {
                Debug.Log($"当前站在: {m_CurBoZhongTrigger.gameObject.name}, " +
                         $"状态: {m_CurBoZhongTrigger.currentState}, " +
                         $"有杂草: {m_CurBoZhongTrigger.hasWeed}");
            }
            else
            {
                Debug.Log("当前没有站在任何土地上");
            }
        }
    }

    void DetectCurrentLand()
    {
        Vector2 playerPos = transform.position;

        // ★★★ 增大检测半径 ★★★
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPos, 0.5f);

        BoZhongTrigger foundLand = null;

        // ★★★ 添加调试信息 ★★★
        Debug.Log($"检测到 {colliders.Length} 个碰撞器");

        foreach (var col in colliders)
        {
            Debug.Log($"碰撞器: {col.gameObject.name}, 标签: {col.tag}");

            BoZhongTrigger land = col.GetComponent<BoZhongTrigger>();
            if (land != null)
            {
                foundLand = land;
                Debug.Log($"找到土地: {land.gameObject.name}, 状态: {land.currentState}");
                break;
            }
        }

        if (foundLand != null && foundLand != m_CurBoZhongTrigger)
        {
            m_CurBoZhongTrigger = foundLand;
            Debug.Log($"站在: {foundLand.gameObject.name}, 状态: {foundLand.currentState}");
        }
        else if (foundLand == null && m_CurBoZhongTrigger != null)
        {
            Debug.Log($"离开土地: {m_CurBoZhongTrigger.gameObject.name}");
            m_CurBoZhongTrigger = null;
        }
    }

    public void BoZhong(int _id)
    {
        if (m_CurBoZhongTrigger != null)
        {
            m_CurBoZhongTrigger.BoZhong(_id);
        }
        else
        {
            Debug.Log("还没走到耕种区域");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
        }
    }

    public void GetNongZuoWu(int _id)
    {
        if (m_CurBoZhongTrigger != null)
            m_CurBoZhongTrigger.GetNongZuoWu(_id);
    }

    public void JiaoShui()
    {
        if (m_CurBoZhongTrigger != null)
            m_CurBoZhongTrigger.JiaoShui();
    }

    public void ShaChong()
    {
        if (m_CurBoZhongTrigger != null)
            m_CurBoZhongTrigger.ShaChong();
    }

    public void UseNormalFertilizer()
    {
        if (m_CurBoZhongTrigger != null)
            m_CurBoZhongTrigger.UseNormalFertilizer();
    }

    public void UseOrganicFertilizer()
    {
        if (m_CurBoZhongTrigger != null)
            m_CurBoZhongTrigger.UseOrganicFertilizer();
    }

    public void ChanChu()
    {
        if (m_CurBoZhongTrigger != null)
        {
            m_CurBoZhongTrigger.RemoveWeed();
        }
        else
        {
            Debug.Log("请先站在土地上");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
        }
    }
}