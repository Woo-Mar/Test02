// ZLayerManager.cs - 全局Z轴层级管理器
using UnityEngine;

public class ZLayerManager : MonoBehaviour
{
    public static ZLayerManager Instance;

    [Header("Z轴层级设置")]
    public float backgroundZ = 10f;      // 背景
    public float tableZ = 0f;           // 桌子
    public float interactiveItemZ = -1f; // 交互物品
    public float cupZ = -2f;            // 杯子（最前面）
    public float uiZ = -5f;             // UI元素
    public float effectZ = -3f;         // 特效

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCupZ(GameObject cup)
    {
        if (cup != null)
        {
            Vector3 pos = cup.transform.position;
            pos.z = cupZ;
            cup.transform.position = pos;
        }
    }
}