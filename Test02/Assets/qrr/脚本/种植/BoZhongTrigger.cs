using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LandState
{
    Uncultivated,  // 未开垦
    Cultivated      // 已开垦
}

public class BoZhongTrigger : MonoBehaviour
{
    [Header("UI提示")]
    public GameObject PressEUI;

    [Header("种子预制体")]
    public List<GameObject> m_SeedsPrefabs;

    [Header("土地状态")]
    public LandState currentState = LandState.Uncultivated;

    [Header("颜色设置")]
    public Color uncultivatedColor = new Color(0.5f, 0.3f, 0.1f, 1f);  // 深棕色
    public Color cultivatedColor = Color.white;                          // 正常颜色

    [Header("组件引用")]
    public SpriteRenderer landSprite;
    public Collider2D landCollider;

    [Header("杂草系统")]
    public GameObject weedPrefab;
    public bool hasWeed = false;
    private GameObject currentWeed;

    // 种植相关变量
    public bool IsEnter = false;  // 改为public
    public bool IsBoZhong;
    public NongZuoWuC m_CurNongZuoWu;
    private GameObject currentSeedInstance;

    void Start()
    {
        // 获取组件
        if (landSprite == null)
            landSprite = GetComponent<SpriteRenderer>();

        if (landCollider == null)
            landCollider = GetComponent<Collider2D>();

        if (landCollider == null)
            landCollider = gameObject.AddComponent<BoxCollider2D>();

        // 设置初始状态
        if (gameObject.name == "土地1" || gameObject.name == "Land_1")
        {
            currentState = LandState.Cultivated;
        }
        else
        {
            currentState = LandState.Uncultivated;
        }

        // 初始化土地外观
        UpdateLandAppearance();

        // 隐藏提示UI
        if (PressEUI != null)
            PressEUI.SetActive(false);
        // ★★★ 确保碰撞器正确设置 ★★★
        if (landCollider != null)
        {
            landCollider.isTrigger = true;
            Debug.Log($"土地 {gameObject.name} 碰撞器已启用，isTrigger={landCollider.isTrigger}");
        }
        else
        {
            Debug.LogError($"土地 {gameObject.name} 没有碰撞器！");
        }
    }

    // 更新土地外观和功能
    public void UpdateLandAppearance()
    {
        if (landSprite == null) return;

        if (currentState == LandState.Uncultivated)
        {
            landSprite.color = uncultivatedColor;

            if (landCollider != null)
            {
                landCollider.enabled = true;
                landCollider.isTrigger = true;  // 必须是触发器
            }
        }
        else
        {
            landSprite.color = cultivatedColor;

            if (landCollider != null)
            {
                landCollider.enabled = true;
                landCollider.isTrigger = true;  // 必须是触发器
            }
        }
    }

    // 开垦土地
    public void Cultivate()
    {
        if (currentState == LandState.Uncultivated)
        {
            currentState = LandState.Cultivated;
            UpdateLandAppearance();

            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("土地开垦成功！");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player") return;

        PlayZhongZhiC player = collision.gameObject.GetComponent<PlayZhongZhiC>();
        if (player == null) return;

        // 设置玩家当前土地
        player.m_CurBoZhongTrigger = this;

        if (currentState != LandState.Cultivated)
        {
            // 未开垦土地：显示提示，不设置IsEnter
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("土地未开垦！请先购买开垦");
            return;
        }

        // 已开垦土地：正常处理
        if (PressEUI != null)
            PressEUI.SetActive(true);
        IsEnter = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player") return;

        if (currentState != LandState.Cultivated) return;

        if (PressEUI != null)
            PressEUI.SetActive(false);

        IsEnter = false;

        PlayZhongZhiC player = collision.gameObject.GetComponent<PlayZhongZhiC>();
        if (player != null && player.m_CurBoZhongTrigger == this)
        {
            player.m_CurBoZhongTrigger = null;
        }
    }

    public void BoZhong(int _id)
    {
        // 检查土地是否已开垦
        if (currentState != LandState.Cultivated)
        {
            Debug.Log($"土地 {gameObject.name} 未开垦，不能种植");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("土地未开垦！");
            return;
        }

        // 检查玩家是否站在土地上
        if (!IsEnter)
        {
            Debug.Log("玩家没有站在土地上");
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先站在土地上");
            return;
        }

        if (IsBoZhong)
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("这块地已经有作物了");
            return;
        }

        if (hasWeed)
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("请先铲除杂草！");
            return;
        }

        LandManager landManager = GetComponent<LandManager>();
        if (landManager != null && landManager.isFallow)
        {
            BugWarningUI warning = FindObjectOfType<BugWarningUI>();
            if (warning != null)
                warning.ShowWarning("土地正在休耕！");
            return;
        }

        if (_id < 0 || _id >= m_SeedsPrefabs.Count || m_SeedsPrefabs[_id] == null)
        {
            Debug.LogError($"种子预制体ID:{_id}不存在！");
            return;
        }

        if (currentSeedInstance != null)
            Destroy(currentSeedInstance);

        Vector3 spawnPosition = transform.position;
        spawnPosition.z = -1;

        currentSeedInstance = Instantiate(m_SeedsPrefabs[_id], spawnPosition, Quaternion.identity, transform);
        currentSeedInstance.SetActive(true);

        if (PressEUI != null)
            PressEUI.SetActive(false);

        IsBoZhong = true;

        m_CurNongZuoWu = currentSeedInstance.GetComponent<NongZuoWuC>();
        if (m_CurNongZuoWu != null)
        {
            m_CurNongZuoWu.m_BoZhongTrigger = this;
        }

        EventManager1.TriggerEvent("OnCropPlanted");
    }

    public void GetNongZuoWu(int _id)
    {
        if (m_CurNongZuoWu == null) return;

        if (AudioManager1.Instance != null && AudioManager1.Instance.harvest != null)
            AudioManager1.Instance.PlaySFX(AudioManager1.Instance.harvest);

        if (CafeSystem.Instance != null)
            CafeSystem.Instance.AddCrop(_id);

        if (currentSeedInstance != null)
            Destroy(currentSeedInstance);

        IsBoZhong = false;
        m_CurNongZuoWu = null;

        SpawnWeed();

        if (PressEUI != null && IsEnter)
            PressEUI.SetActive(true);

        if (_id == 0) Level1Mgr.GetInstance().AddCaoMeiNums();
        else if (_id == 1) Level1Mgr.GetInstance().AddYangMeiNums();
        else if (_id == 2) Level1Mgr.GetInstance().AddWuHuaGuoNums();
    }

    void SpawnWeed()
    {
        if (weedPrefab == null) return;

        if (currentWeed != null)
            Destroy(currentWeed);

        Vector3 spawnPos = transform.position;
        spawnPos.z = -1;
        currentWeed = Instantiate(weedPrefab, spawnPos, Quaternion.identity, transform);
        hasWeed = true;
    }

    public void RemoveWeed()
    {
        if (!hasWeed) return;

        if (currentWeed != null)
            Destroy(currentWeed);
        currentWeed = null;

        hasWeed = false;

        LandManager landManager = GetComponent<LandManager>();
        if (landManager != null)
            landManager.StartFallow();

        BugWarningUI warning = FindObjectOfType<BugWarningUI>();
        if (warning != null)
            warning.ShowWarning("杂草已铲除，开始休耕");
    }

    public void JiaoShui()
    {
        if (m_CurNongZuoWu != null)
            m_CurNongZuoWu.AddWaterPower(30);
    }

    public void ShaChong()
    {
        if (m_CurNongZuoWu != null) { }
    }

    public void UseNormalFertilizer()
    {
        if (m_CurNongZuoWu != null)
            m_CurNongZuoWu.ApplyNormalFertilizer();
    }

    public void UseOrganicFertilizer()
    {
        if (m_CurNongZuoWu != null)
            m_CurNongZuoWu.ApplyOrganicFertilizer();
    }

    void Update()
    {
        if (currentState != LandState.Cultivated) return;

        if (IsEnter && IsBoZhong && m_CurNongZuoWu != null && m_CurNongZuoWu.GroupUpValue >= 3)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GetNongZuoWu(m_CurNongZuoWu.cropId);
            }
        }
    }
}