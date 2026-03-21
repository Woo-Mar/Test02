using UnityEngine;
using UnityEngine.UI;

public class WeatherSystem : MonoBehaviour
{
    public enum Weather { Sunny, Rainy, Cloudy }
    public Weather currentWeather;

    [Header("UI显示")]
    public Text weatherText;

    [Header("粒子效果")]
    public GameObject sunnyEffectPrefab;
    public GameObject rainyEffectPrefab;
    public GameObject cloudyEffectPrefab;

    // 当前活动的粒子实例
    private GameObject currentEffectInstance;

    [Header("颜色设置")]
    public Color sunnyColor = new Color(1f, 0.9f, 0.2f);
    public Color rainyColor = new Color(0.2f, 0.6f, 1f);
    public Color cloudyColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("时间设置")]
    public float weatherChangeInterval = 8f;
    private float timer;

    [Header("昼夜控制")]
    public bool randomWeatherEnabled = true;

    void Start()
    {
        currentWeather = (Weather)Random.Range(0, 3);
        timer = weatherChangeInterval;

        CreateWeatherEffect(currentWeather);
        UpdateWeatherUI();

        // ★★★ 初始播放对应天气的环境音 ★★★
        PlayWeatherSound(currentWeather);
    }

    void Update()
    {
        if (randomWeatherEnabled)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                ChangeWeather();
                timer = weatherChangeInterval;
            }
        }
    }

    public void SetWeather(Weather weather)
    {
        Weather oldWeather = currentWeather;
        currentWeather = weather;

        Debug.Log("强制设置天气: " + oldWeather.ToString() + " -> " + currentWeather.ToString());

        DestroyWeatherEffect();
        CreateWeatherEffect(currentWeather);

        // ★★★ 播放对应天气的环境音 ★★★
        PlayWeatherSound(currentWeather);

        WeatherChanged();
    }

    public void EnableRandomWeather(bool enable)
    {
        randomWeatherEnabled = enable;
        Debug.Log($"随机天气: {(enable ? "启用" : "禁用")}");
    }

    private void ChangeWeather()
    {
        Weather oldWeather = currentWeather;

        currentWeather = (Weather)Random.Range(0, 3);

        while (currentWeather == oldWeather)
        {
            currentWeather = (Weather)Random.Range(0, 3);
        }

        Debug.Log("天气变化: " + oldWeather.ToString() + " -> " + currentWeather.ToString());

        DestroyWeatherEffect();
        CreateWeatherEffect(currentWeather);

        // ★★★ 播放对应天气的环境音 ★★★
        PlayWeatherSound(currentWeather);

        WeatherChanged();
    }

    // ★★★ 根据天气播放对应的环境音 ★★★
    private void PlayWeatherSound(Weather weather)
    {
        if (AudioManager1.Instance == null)
        {
            Debug.LogWarning("AudioManager 不存在！");
            return;
        }

        switch (weather)
        {
            case Weather.Sunny:
                // 晴天：停止环境音
                AudioManager1.Instance.StopAmbientSound();
                Debug.Log("停止环境音（晴天）");
                break;
            case Weather.Rainy:
                if (AudioManager1.Instance.rainSound != null)
                {
                    AudioManager1.Instance.PlayAmbientSound(AudioManager1.Instance.rainSound);
                    Debug.Log("播放下雨声");
                }
                else
                {
                    Debug.LogWarning("下雨声音效文件未设置！");
                }
                break;
            case Weather.Cloudy:
                if (AudioManager1.Instance.windSound != null)
                {
                    AudioManager1.Instance.PlayAmbientSound(AudioManager1.Instance.windSound);
                    Debug.Log("播放风声");
                }
                else
                {
                    Debug.LogWarning("风声音效文件未设置！");
                }
                break;
        }
    }

    private void CreateWeatherEffect(Weather weather)
    {
        switch (weather)
        {
            case Weather.Sunny:
                if (sunnyEffectPrefab != null)
                    currentEffectInstance = Instantiate(sunnyEffectPrefab, transform);
                break;
            case Weather.Rainy:
                if (rainyEffectPrefab != null)
                    currentEffectInstance = Instantiate(rainyEffectPrefab, transform);
                break;
            case Weather.Cloudy:
                if (cloudyEffectPrefab != null)
                    currentEffectInstance = Instantiate(cloudyEffectPrefab, transform);
                break;
        }
    }

    private void DestroyWeatherEffect()
    {
        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
            currentEffectInstance = null;
        }
    }

    private void WeatherChanged()
    {
        UpdateWeatherUI();
    }

    private void UpdateWeatherUI()
    {
        if (weatherText != null)
        {
            weatherText.text = "实时天气: " + GetWeatherChineseName(currentWeather);
            weatherText.color = GetWeatherColor(currentWeather);
        }
    }

    private string GetWeatherChineseName(Weather weather)
    {
        switch (weather)
        {
            case Weather.Sunny:
                return "晴天 ☀️";
            case Weather.Rainy:
                return "雨天 🌧️";
            case Weather.Cloudy:
                return "多云 ☁️";
            default:
                return "未知";
        }
    }

    private Color GetWeatherColor(Weather weather)
    {
        switch (weather)
        {
            case Weather.Sunny:
                return sunnyColor;
            case Weather.Rainy:
                return rainyColor;
            case Weather.Cloudy:
                return cloudyColor;
            default:
                return Color.white;
        }
    }

    public Weather GetCurrentWeather()
    {
        return currentWeather;
    }

    void OnDestroy()
    {
        DestroyWeatherEffect();
    }
}