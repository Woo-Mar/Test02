// EventManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // 事件类型
    [Header("事件调试")]
    public bool logEvents = true;

    // ===== 咖啡制作事件 =====
    public event Action<CoffeeMachine> OnCoffeeMachineInitialized;
    public event Action<string> OnCoffeeGrinded;  // 咖啡已研磨
    public event Action<Coffee, Cup> OnCoffeeBrewed;  // 咖啡已萃取
    public event Action<CoffeeMachine> OnCoffeeMachineReset;

    // ===== 原料添加事件 =====
    public event Action<string, Coffee, Cup> OnIngredientAdded;  // 原料名称, 咖啡数据, 杯子
    public event Action<Cup, string> OnExtraIngredientAdded;  // 杯子, 原料名称

    // ===== 订单事件 =====
    public event Action<Customer, Coffee.CoffeeType> OnCustomerArrived;  // 顾客到达
    public event Action<Customer, Coffee> OnOrderCompleted;  // 订单完成（正确）
    public event Action<Customer> OnOrderIncorrect;  // 订单错误
    public event Action<Customer> OnCustomerLeftAngry;  // 顾客生气离开
    public event Action<Customer, Coffee> OnFigTeaServed;  // 无花果茶服务

    // ===== 经济事件 =====
    public event Action<int, string> OnMoneyEarned;  // 金额, 来源
    public event Action<int, string> OnMoneySpent;  // 金额, 用途

    // ===== 杯子事件 =====
    public event Action<Cup, bool> OnCupStateChanged;  // 杯子, 是否为空
    public event Action<Cup> OnCupFilledWithCoffee;
    public event Action<Cup> OnCupIceAdded;
    public event Action<Cup> OnCupServed;
    public event Action<Cup> OnCupDiscarded;

    // ===== 游戏状态事件 =====
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    // ===== 错误/警告事件 =====
    public event Action<string, LogType> OnGameLog;  // 消息, 日志类型

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

    // ===== 事件触发方法 =====
    public void TriggerCoffeeMachineInitialized(CoffeeMachine coffeeMachine)
    {
        OnCoffeeMachineInitialized?.Invoke(coffeeMachine);
        LogEvent($"咖啡机初始化: {coffeeMachine.name}");
    }

    public void TriggerCoffeeGrinded(string coffeeType)
    {
        OnCoffeeGrinded?.Invoke(coffeeType);
        LogEvent($"咖啡研磨完成: {coffeeType}");
    }

    public void TriggerCoffeeBrewed(Coffee coffee, Cup cup)
    {
        OnCoffeeBrewed?.Invoke(coffee, cup);
        LogEvent($"咖啡萃取完成: {coffee.type}");
    }

    public void TriggerIngredientAdded(string ingredient, Coffee coffee, Cup cup)
    {
        OnIngredientAdded?.Invoke(ingredient, coffee, cup);
        LogEvent($"原料添加: {ingredient} 到 {coffee.type}");
    }

    public void TriggerExtraIngredientAdded(Cup cup, string ingredient)
    {
        OnExtraIngredientAdded?.Invoke(cup, ingredient);
        LogEvent($"额外原料添加: {ingredient} 到杯子");
    }

    public void TriggerCustomerArrived(Customer customer, Coffee.CoffeeType orderType)
    {
        OnCustomerArrived?.Invoke(customer, orderType);
        LogEvent($"顾客到达，订单: {orderType}");
    }

    public void TriggerOrderCompleted(Customer customer, Coffee coffee)
    {
        OnOrderCompleted?.Invoke(customer, coffee);
        LogEvent($"订单完成: {coffee.type}，奖励: {coffee.value}金币");
    }

    public void TriggerOrderIncorrect(Customer customer)
    {
        OnOrderIncorrect?.Invoke(customer);
        LogEvent($"订单错误，顾客不满意");
    }

    public void TriggerCustomerLeftAngry(Customer customer)
    {
        OnCustomerLeftAngry?.Invoke(customer);
        LogEvent($"顾客生气离开");
    }

    public void TriggerFigTeaServed(Customer customer, Coffee coffee)
    {
        OnFigTeaServed?.Invoke(customer, coffee);
        LogEvent($"无花果茶服务完成");
    }

    public void TriggerMoneyEarned(int amount, string source)
    {
        OnMoneyEarned?.Invoke(amount, source);
        LogEvent($"获得金币: {amount}，来源: {source}");
    }

    public void TriggerMoneySpent(int amount, string purpose)
    {
        OnMoneySpent?.Invoke(amount, purpose);
        LogEvent($"消费金币: {amount}，用途: {purpose}");
    }

    public void TriggerCupFilledWithCoffee(Cup cup)
    {
        OnCupFilledWithCoffee?.Invoke(cup);
        LogEvent($"杯子装满咖啡");
    }

    public void TriggerCupIceAdded(Cup cup)
    {
        OnCupIceAdded?.Invoke(cup);
        LogEvent($"杯子加冰");
    }

    public void TriggerCupServed(Cup cup)
    {
        OnCupServed?.Invoke(cup);
        LogEvent($"杯子已服务");
    }

    public void TriggerCupDiscarded(Cup cup)
    {
        OnCupDiscarded?.Invoke(cup);
        LogEvent($"杯子已丢弃");
    }

    public void TriggerGameStarted()
    {
        OnGameStarted?.Invoke();
        LogEvent($"游戏开始");
    }

    public void TriggerGamePaused()
    {
        OnGamePaused?.Invoke();
        LogEvent($"游戏暂停");
    }

    public void TriggerGameResumed()
    {
        OnGameResumed?.Invoke();
        LogEvent($"游戏恢复");
    }

    public void TriggerGameLog(string message, LogType logType = LogType.Log)
    {
        OnGameLog?.Invoke(message, logType);

        // 同时输出到Unity控制台
        switch (logType)
        {
            case LogType.Log:
                Debug.Log(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Error:
                Debug.LogError(message);
                break;
        }
    }

    // ===== 辅助方法 =====
    private void LogEvent(string message)
    {
        if (logEvents)
        {
            Debug.Log($"[事件] {message}");
        }
    }
}