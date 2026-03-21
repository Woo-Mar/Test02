using UnityEngine;
using UnityEngine.UI;

public class DayNightSystem : MonoBehaviour
{
    public enum TimeOfDay { Day, Night }
    public TimeOfDay currentTime = TimeOfDay.Day;

    [Header("时间设置")]
    public float dayDuration = 60f;     // 白天60秒
    public float nightDuration = 30f;    // 夜晚30秒
    private float timer;

    [Header("UI")]
    public Text timeText;                // 显示时间
    public Image nightOverlay;            // 夜晚遮罩

    [Header("系统引用")]
    public RushMosnterMgr bugSpawner;     // 害虫生成器
    public WeatherSystem weatherSystem;    // 天气系统

    void Start()
    {
        timer = dayDuration;
        StartDay();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SwitchTime();
        }

        // 更新UI
        if (timeText != null)
        {
            string timeName = currentTime == TimeOfDay.Day ? "白天" : "夜晚";
            timeText.text = $"{timeName} {Mathf.Ceil(timer)}秒";
        }
    }

    void SwitchTime()
    {
        if (currentTime == TimeOfDay.Day)
        {
            StartNight();
        }
        else
        {
            StartDay();
        }
    }

    void StartDay()
    {
        currentTime = TimeOfDay.Day;
        timer = dayDuration;

        // 关闭夜晚遮罩
        if (nightOverlay != null)
            nightOverlay.gameObject.SetActive(false);

        // 关闭害虫生成器（白天不生成害虫）
        if (bugSpawner != null)
            bugSpawner.SetNightMode(false);

        // 恢复随机天气
        if (weatherSystem != null)
            weatherSystem.EnableRandomWeather(true);

        Debug.Log("☀️ 白天来临，没有害虫");
    }

    void StartNight()
    {
        currentTime = TimeOfDay.Night;
        timer = nightDuration;

        // 开启夜晚遮罩
        if (nightOverlay != null)
            nightOverlay.gameObject.SetActive(true);

        // 开启害虫生成器（夜晚生成害虫）
        if (bugSpawner != null)
            bugSpawner.SetNightMode(true);

        // 夜晚强制设置为阴天（没有晴天）
        if (weatherSystem != null)
        {
            weatherSystem.EnableRandomWeather(false);
            weatherSystem.SetWeather(WeatherSystem.Weather.Cloudy); // 阴天
        }

        Debug.Log("🌙 夜晚来临，害虫出没！");
    }

    public bool IsNight()
    {
        return currentTime == TimeOfDay.Night;
    }
}