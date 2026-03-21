using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NongZuoWuC : MonoBehaviour
{
    public BoZhongTrigger m_BoZhongTrigger;

    //水分
    public int m_CurWaterPower;
    public int m_MaxWaterPower;
    private float m_WaterPowerTime;
    public Image m_WaterPowerBar;

    //健康值
    public int m_CurJianKangPower;
    public int m_MaxJianKangPower;
    private float m_JianKangPowerTime;
    public Image m_JianKangPowerBar;

    //成长值
    public int m_CurChengZhangPower;
    public int m_MaxChengZhangPower;
    private float m_ChengZhangPowerTime;
    public Image m_ChengZhangPowerBar;

    //成长阶段
    public int GroupUpValue;

    public List<GameObject> NongZuoWuList;

    //面板
    public GameObject MianBanui;
    public GameObject MianBanuiBtn;

    private WeatherSystem weatherSystem;
    private DayNightSystem dayNightSystem;
    public int cropId;

    // 肥料效果
    private bool hasFertilizer = false;
    private float fertilizerTimer = 0f;
    private float fertilizerDuration = 30f;
    private float growthMultiplier = 1f;

    void Start()
    {
        weatherSystem = FindObjectOfType<WeatherSystem>();
        dayNightSystem = FindObjectOfType<DayNightSystem>();

        // 初始满属性
        m_CurWaterPower = m_MaxWaterPower;
        m_CurJianKangPower = m_MaxJianKangPower;

        // ★★★ 新增：初始化显示第0阶段 ★★★
        GroupUpValue = 0;
        for (int i = 0; i < NongZuoWuList.Count; i++)
        {
            NongZuoWuList[i].SetActive(false);
        }
        if (NongZuoWuList.Count > 0 && GroupUpValue < NongZuoWuList.Count)
        {
            NongZuoWuList[GroupUpValue].SetActive(true);
            Debug.Log($"初始化显示第{GroupUpValue}阶段");
        }
    }

    void Update()
    {
        if (GroupUpValue >= 3)
            return;

        // 水分消耗（只有浇水按钮能补充）
        WaterPowerLogic();
        if (m_WaterPowerBar != null)
            m_WaterPowerBar.fillAmount = (float)m_CurWaterPower / m_MaxWaterPower;

        // 健康消耗（只有肥料能补充，害虫攻击会掉）
        JianKangPowerLogic();
        if (m_JianKangPowerBar != null)
            m_JianKangPowerBar.fillAmount = (float)m_CurJianKangPower / m_MaxJianKangPower;

        // 肥料计时
        if (hasFertilizer)
        {
            fertilizerTimer += Time.deltaTime;
            if (fertilizerTimer >= fertilizerDuration)
            {
                hasFertilizer = false;
                growthMultiplier = 1f;
                Debug.Log("肥料效果消失");
            }
        }

        // 成长逻辑
        ChengZhangPowerLogic();
        if (m_ChengZhangPowerBar != null)
            m_ChengZhangPowerBar.fillAmount = (float)m_CurChengZhangPower / m_MaxChengZhangPower;
    }

    public void GroupUp()
    {
        // 先增加阶段
        GroupUpValue++;

        // 隐藏所有阶段
        for (int i = 0; i < NongZuoWuList.Count; i++)
        {
            NongZuoWuList[i].SetActive(false);
        }

        // 显示当前阶段
        if (GroupUpValue < NongZuoWuList.Count)
        {
            NongZuoWuList[GroupUpValue].SetActive(true);
            Debug.Log($"成长到第{GroupUpValue}阶段");
        }

        // 如果已经成熟（第3阶段）
        if (GroupUpValue >= 3)
        {
            MianBanui.SetActive(false);
            MianBanuiBtn.SetActive(false);
            Debug.Log("作物成熟");
        }

        // 重置成长值
        m_CurChengZhangPower = 0;
    }
    // ========== 水分系统 ==========
    public void AddWaterPower(int _value)
    {
        m_CurWaterPower += _value;
        if (m_CurWaterPower >= m_MaxWaterPower)
        {
            m_CurWaterPower = m_MaxWaterPower;
        }
        Debug.Log($"浇水 +{_value}，当前水分：{m_CurWaterPower}/{m_MaxWaterPower}");
    }

    public void CostWaterPower(int _value)
    {
        m_CurWaterPower -= _value;
        if (m_CurWaterPower <= 0)
        {
            m_CurWaterPower = 0;
            Die();
        }
    }

    // 水分消耗（8秒一次）
    public void WaterPowerLogic()
    {
        m_WaterPowerTime += Time.deltaTime;

        float consumeInterval = 5f;

        if (weatherSystem != null)
        {
            switch (weatherSystem.currentWeather)
            {
                case WeatherSystem.Weather.Sunny:
                    consumeInterval = 3f;
                    break;
                case WeatherSystem.Weather.Rainy:
                    consumeInterval = 12f;
                    if (Random.value < 0.1f)
                    {
                        AddWaterPower(1);
                    }
                    break;
                case WeatherSystem.Weather.Cloudy:
                    consumeInterval = 10f;
                    break;
            }
        }

        if (dayNightSystem != null && dayNightSystem.IsNight())
        {
            consumeInterval *= 1.2f;
        }

        if (m_WaterPowerTime >= consumeInterval)
        {
            m_WaterPowerTime = 0;
            int waterCost = Random.Range(5, 11);
            CostWaterPower(waterCost);
        }
    }

    // ========== 健康系统 ==========
    public void AddJianKangPower(int _value)
    {
        m_CurJianKangPower += _value;
        if (m_CurJianKangPower >= m_MaxJianKangPower)
        {
            m_CurJianKangPower = m_MaxJianKangPower;
        }
        Debug.Log($"健康 +{_value}，当前健康：{m_CurJianKangPower}/{m_MaxJianKangPower}");
    }

    public void CostJianKangPower(int _value)
    {
        m_CurJianKangPower -= _value;
        if (m_CurJianKangPower <= 0)
        {
            m_CurJianKangPower = 0;
            Die();
        }
    }

    // 健康消耗（20秒一次）
    public void JianKangPowerLogic()
    {
        m_JianKangPowerTime += Time.deltaTime;

        float consumeInterval = 5f;

        if (weatherSystem != null)
        {
            switch (weatherSystem.currentWeather)
            {
                case WeatherSystem.Weather.Rainy:
                    consumeInterval = 15f;
                    break;
                case WeatherSystem.Weather.Sunny:
                    consumeInterval = 10f;
                    break;
            }
        }

        if (m_JianKangPowerTime >= consumeInterval)
        {
            m_JianKangPowerTime = 0;
            int healthCost = Random.Range(3, 8);
            CostJianKangPower(healthCost);
        }
    }

    // 害虫攻击时调用（只有杀虫剂能预防）
    public void OnPestAttack(int damage)
    {
        int oldHealth = m_CurJianKangPower;
        CostJianKangPower(damage);

        Debug.Log($"害虫攻击：健康 {oldHealth} -> {m_CurJianKangPower}，损失 {damage}");

        StopCoroutine("PauseGrowth");
        StartCoroutine(PauseGrowth(2f));
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }

    IEnumerator PauseGrowth(float seconds)
    {
        float originalMultiplier = growthMultiplier;
        growthMultiplier = 0f;
        yield return new WaitForSeconds(seconds);
        growthMultiplier = originalMultiplier;
    }

    // ========== 肥料系统 ==========
    // 普通肥料（按钮形式，免费无限）- 只补充健康，加速生长
    public void ApplyNormalFertilizer()
    {
        hasFertilizer = true;
        fertilizerTimer = 0f;
        growthMultiplier = 1.3f;
        AddJianKangPower(30);
        Debug.Log("使用普通肥料，生长加速30%，恢复30健康");
    }

    // 有机肥料（格子形式，20元）- 只补充健康，加速生长
    public void ApplyOrganicFertilizer()
    {
        hasFertilizer = true;
        fertilizerTimer = 0f;
        growthMultiplier = 1.5f;
        AddJianKangPower(50);
        Debug.Log("使用有机肥料，生长加速50%，恢复50健康");
    }

    // ========== 成长系统 ==========
    public void ChengZhangPowerLogic()
    {
        if (GroupUpValue >= 3) return;  // 成熟了就不再生长

        m_ChengZhangPowerTime += Time.deltaTime * growthMultiplier;

        float growInterval = Random.Range(0.5f, 1.5f);

        if (dayNightSystem != null && dayNightSystem.IsNight())
        {
            growInterval *= 1.5f;
        }

        if (m_ChengZhangPowerTime >= growInterval)
        {
            m_ChengZhangPowerTime = 0;

            int growValue = Random.Range(1, 4);

            if (weatherSystem != null)
            {
                switch (weatherSystem.currentWeather)
                {
                    case WeatherSystem.Weather.Sunny:
                        growValue = Mathf.RoundToInt(growValue * 2.5f);
                        break;
                    case WeatherSystem.Weather.Rainy:
                        growValue = Mathf.RoundToInt(growValue * 2.0f);
                        break;
                    case WeatherSystem.Weather.Cloudy:
                        growValue = Mathf.RoundToInt(growValue * 0.4f);
                        break;
                }
            }

            if (growValue < 1) growValue = 1;

            AddChengZhangPower(growValue);
        }
    }

    public void AddChengZhangPower(int _Value)
    {
        if (GroupUpValue >= 3) return;

        m_CurChengZhangPower += _Value;

        // 成长值满，进入下一阶段
        if (m_CurChengZhangPower >= m_MaxChengZhangPower)
        {
            m_CurChengZhangPower = m_MaxChengZhangPower;
            GroupUp();  // 调用阶段升级
        }
    }

    public void ReSetData()
    {
        m_CurWaterPower = m_MaxWaterPower;
        m_CurJianKangPower = m_MaxJianKangPower;
        m_CurChengZhangPower = 0;
        GroupUpValue = 0;

        if (m_BoZhongTrigger != null)
        {
            m_BoZhongTrigger.IsBoZhong = false;
        }
    }

    public void Die()
    {
        ReSetData();
        Level1Mgr.GetInstance().AddDZhiWuNums();
        Destroy(gameObject);
    }
}