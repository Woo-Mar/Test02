using System;
using System.Collections.Generic;

public static class EventManager1
{
    // 原有的带参数事件
    private static Dictionary<string, Action<WeatherSystem.Weather>> weatherEventDictionary =
        new Dictionary<string, Action<WeatherSystem.Weather>>();

    // 新增的无参数事件
    private static Dictionary<string, Action> simpleEventDictionary =
        new Dictionary<string, Action>();

    // 原有的带参数监听
    public static void StartListening(string eventName, Action<WeatherSystem.Weather> listener)
    {
        if (!weatherEventDictionary.ContainsKey(eventName))
        {
            weatherEventDictionary.Add(eventName, null);
        }
        weatherEventDictionary[eventName] += listener;
    }

    // 新增的无参数监听
    public static void StartListening(string eventName, Action listener)
    {
        if (!simpleEventDictionary.ContainsKey(eventName))
        {
            simpleEventDictionary.Add(eventName, null);
        }
        simpleEventDictionary[eventName] += listener;
    }

    // 原有的带参数停止监听
    public static void StopListening(string eventName, Action<WeatherSystem.Weather> listener)
    {
        if (weatherEventDictionary.ContainsKey(eventName))
        {
            weatherEventDictionary[eventName] -= listener;
        }
    }

    // 新增的无参数停止监听
    public static void StopListening(string eventName, Action listener)
    {
        if (simpleEventDictionary.ContainsKey(eventName))
        {
            simpleEventDictionary[eventName] -= listener;
        }
    }

    // 原有的带参数触发
    public static void TriggerEvent(string eventName, WeatherSystem.Weather weather)
    {
        if (weatherEventDictionary.ContainsKey(eventName))
        {
            weatherEventDictionary[eventName]?.Invoke(weather);
        }
    }

    // 新增的无参数触发
    public static void TriggerEvent(string eventName)
    {
        if (simpleEventDictionary.ContainsKey(eventName))
        {
            simpleEventDictionary[eventName]?.Invoke();
        }
    }
}