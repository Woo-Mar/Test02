// CoffeeOrderManager.cs - 修改后的版本
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 顾客订单管理器 - 控制顾客生成、排队和订单完成
/// 单例模式，全局管理顾客流
/// </summary>
public class CoffeeOrderManager : MonoBehaviour
{
    public static CoffeeOrderManager Instance; // 单例实例

    public List<Customer> waitingCustomers = new List<Customer>(); // 等待中的顾客列表
    // 将原来的单个生成点改为数组
    public Transform[] customerSpawnPoints;      // 三个顾客生成点
    public GameObject customerPrefab;          // 顾客预制体

    [Header("顾客管理")]
    public int maxCustomers = 5;               // 最大顾客数量
    public bool canSpawnCustomers = true;      // 是否允许生成新顾客

    [Header("订单阶段")]
    [Tooltip("1=普通单杯, 2=VIP+1-2杯, 3=急躁+2-3杯")]
    public int orderPhase = 1;

    // 添加字典来跟踪每个顾客生成点
    private Dictionary<Customer, Transform> customerSpawnPointMap = new Dictionary<Customer, Transform>();

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 初始延迟后生成第一个顾客
        Invoke("SpawnCustomer", 2f);
    }

    // 修改：寻找空闲的生成点并返回生成点索引
    public int FindAvailableSpawnPointIndex()
    {
        // 收集当前所有已占用的生成点
        HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>();
        foreach (var kvp in customerSpawnPointMap)
        {
            if (kvp.Key != null) // 确保顾客还未被销毁
            {
                occupiedSpawnPoints.Add(kvp.Value);
            }
        }

        // 寻找空闲的生成点
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < customerSpawnPoints.Length; i++)
        {
            if (!occupiedSpawnPoints.Contains(customerSpawnPoints[i]))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            // 随机选择一个空闲位置
            int randomIndex = Random.Range(0, availableIndices.Count);
            return availableIndices[randomIndex];
        }

        return -1; // 没有空闲位置
    }


    /// <summary>
    /// 生成新顾客
    /// </summary>
    // 修改：生成顾客
    public void SpawnCustomer()
    {
        if (!canSpawnCustomers) return;
        if (waitingCustomers.Count >= maxCustomers)
        {
            Invoke(nameof(SpawnCustomer), 10f);
            return;
        }

        int spawnIndex = FindAvailableSpawnPointIndex();
        if (spawnIndex == -1)
        {
            Invoke(nameof(SpawnCustomer), 5f);
            return;
        }

        Transform spawnPoint = customerSpawnPoints[spawnIndex];

        // 实例化顾客
        GameObject newCustomerObj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        
        // 确保Z轴正确（设置为0，或根据场景调整）
        Vector3 pos = newCustomerObj.transform.position;
        pos.z = 0;
        newCustomerObj.transform.position = pos;

        Customer customer = newCustomerObj.GetComponent<Customer>();

        // 根据阶段确定顾客类型和订单数量
        Customer.CustomerType type;
        int orderCount;

        switch (orderPhase)
        {
            case 1:
                type = Customer.CustomerType.Normal;
                orderCount = 1;
                break;
            case 2:
                // 随机普通或VIP（各50%）
                type = Random.value < 0.5f ? Customer.CustomerType.Normal : Customer.CustomerType.VIP;
                orderCount = Random.Range(1, 3); // 1 或 2
                break;
            case 3:
                // 随机三种类型，概率可调整
                float r = Random.value;
                if (r < 0.4f) type = Customer.CustomerType.Normal;
                else if (r < 0.7f) type = Customer.CustomerType.VIP;
                else type = Customer.CustomerType.Impatient;
                orderCount = Random.Range(2, 4); // 2 或 3
                break;
            default:
                type = Customer.CustomerType.Normal;
                orderCount = 1;
                break;
        }
        Debug.Log($"[SpawnCustomer] 生成顾客类型: {type}, 订单数: {orderCount}");
        // 生成订单列表
        List<Coffee.CoffeeType> orders = new List<Coffee.CoffeeType>();
        for (int i = 0; i < orderCount; i++)
        {
            orders.Add(GetRandomCoffeeType());
        }

        // 初始化顾客
        customer.spawnPoint = spawnPoint;
        customer.InitializeOrders(orders, type);

        // 记录映射
        customerSpawnPointMap[customer] = spawnPoint;
        waitingCustomers.Add(customer);

        Debug.Log($"新顾客生成！类型：{type}，订单：{orderCount}杯，位置：{spawnPoint.name}");

        // 安排下一个顾客
        if (canSpawnCustomers && waitingCustomers.Count < maxCustomers)
        {
            float nextSpawnTime = Random.Range(20f, 40f);
            Invoke(nameof(SpawnCustomer), nextSpawnTime);
        }
    }

    private Coffee.CoffeeType GetRandomCoffeeType()
    {
        float r = Random.value;
        if (r < 0.2f) return Coffee.CoffeeType.HotCoffee;
        else if (r < 0.4f) return Coffee.CoffeeType.IcedCoffee;
        else if (r < 0.6f) return Coffee.CoffeeType.Latte;
        else if (r < 0.75f) return Coffee.CoffeeType.StrawberryLatte;
        else if (r < 0.9f) return Coffee.CoffeeType.CarambolaAmericano;
        else return Coffee.CoffeeType.FigOnly;
    }


    /// <summary>
    /// 完成订单处理
    /// </summary>
    /// <param name="coffee">完成的咖啡对象</param>
    /// <param name="customer">被服务的顾客</param>
    // 修改：完成订单时释放生成点
    public void CompleteOrder(Customer customer, int totalReward)
    {
        if (waitingCustomers.Contains(customer))
        {
            // 释放生成点
            if (customerSpawnPointMap.ContainsKey(customer))
            {
                Transform spawnPoint = customerSpawnPointMap[customer];
                Debug.Log($"释放生成点: {spawnPoint.name}");
                customerSpawnPointMap.Remove(customer);
            }

            waitingCustomers.Remove(customer);

            // 发放金币
            GameManager.Instance.AddMoney(totalReward, $"订单完成:{customer.customerType}");

            // 触发事件
            EventManager.Instance.TriggerOrderCompleted(customer, totalReward);

            Destroy(customer.gameObject);

            // 尝试生成新顾客
            if (canSpawnCustomers && waitingCustomers.Count < maxCustomers)
            {
                float nextSpawnDelay = Random.Range(5f, 15f);
                Invoke(nameof(SpawnCustomer), nextSpawnDelay);
            }
        }
    }

    // 修改：当顾客生气离开时也要释放生成点
    public void CustomerLeftAngry(Customer customer)
    {
        if (waitingCustomers.Contains(customer))
        {
            if (customerSpawnPointMap.ContainsKey(customer))
            {
                Transform spawnPoint = customerSpawnPointMap[customer];
                Debug.Log($"顾客生气离开，释放生成点: {spawnPoint.name}");
                customerSpawnPointMap.Remove(customer);
            }
            waitingCustomers.Remove(customer);
            Destroy(customer.gameObject);

            if (canSpawnCustomers && waitingCustomers.Count < maxCustomers)
            {
                float nextSpawnDelay = Random.Range(5f, 15f);
                Invoke(nameof(SpawnCustomer), nextSpawnDelay);
            }
        }
    }

    /// <summary>
    /// 停止生成新顾客（游戏暂停或结束时调用）
    /// </summary>
    public void StopSpawningCustomers()
    {
        canSpawnCustomers = false;
    }

    /// <summary>
    /// 恢复生成新顾客
    /// </summary>
    public void ResumeSpawningCustomers()
    {
        canSpawnCustomers = true;
        if (waitingCustomers.Count < maxCustomers)
        {
            Invoke("SpawnCustomer", Random.Range(5f, 15f));
        }
    }
}