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
        // 添加检查：如果游戏未开始，不生成顾客
        if (!canSpawnCustomers)
        {
            // Debug.Log("游戏未开始，暂停生成顾客");
            return;
        }

        // 检查是否达到最大顾客数量
        if (waitingCustomers.Count >= maxCustomers)
        {
            Debug.Log($"已达到最大顾客数({maxCustomers})，等待有空位");
            Invoke("SpawnCustomer", 10f);
            return;
        }

        // 检查是否有空闲的生成点
        int spawnPointIndex = FindAvailableSpawnPointIndex();
        if (spawnPointIndex == -1)
        {
            Debug.Log("所有生成点都被占用，等待有空位");
            Invoke("SpawnCustomer", 5f);
            return;
        }

        Transform spawnPoint = customerSpawnPoints[spawnPointIndex];

        // 生成顾客
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.z = 0;
        GameObject newCustomer = Instantiate(customerPrefab, spawnPos, Quaternion.identity);

        Vector3 customerPos = newCustomer.transform.position;
        customerPos.z = 0;
        newCustomer.transform.position = customerPos;

        Customer customerScript = newCustomer.GetComponent<Customer>();
        if (customerScript != null)
        {
            // 设置顾客的生成点
            customerScript.spawnPoint = spawnPoint;

            // 记录顾客和生成点的映射
            customerSpawnPointMap[customerScript] = spawnPoint;

            // 随机设置订单类型
            customerScript.wantsIcedCoffee = Random.value > 0.5f;
            waitingCustomers.Add(customerScript);

            Debug.Log($"新顾客到达！在位置{spawnPoint.name}，想要{(customerScript.wantsIcedCoffee ? "冰咖啡" : "热咖啡")}");
        }

        // 安排下一个顾客生成
        if (canSpawnCustomers && waitingCustomers.Count < maxCustomers)
        {
            float nextSpawnTime = Random.Range(20f, 40f);
            Invoke("SpawnCustomer", nextSpawnTime);
        }
        else
        {
            Invoke("SpawnCustomer", 10f);
        }
    }

    /// <summary>
    /// 完成订单处理
    /// </summary>
    /// <param name="coffee">完成的咖啡对象</param>
    /// <param name="customer">被服务的顾客</param>
    // 修改：完成订单时释放生成点
    public void CompleteOrder(Coffee coffee, Customer customer)
    {
        // 添加检查：如果游戏未开始，不处理订单
        if (!canSpawnCustomers)
        {
            return;
        }

        if (waitingCustomers.Contains(customer))
        {
            // 释放顾客占用的生成点
            if (customerSpawnPointMap.ContainsKey(customer))
            {
                Transform spawnPoint = customerSpawnPointMap[customer];
                Debug.Log($"释放生成点: {spawnPoint.name}");
                customerSpawnPointMap.Remove(customer);
            }

            // 从等待列表中移除
            waitingCustomers.Remove(customer);
            Destroy(customer.gameObject);

            // 发放金币
            GameManager.Instance.AddMoney(coffee.value);

            Debug.Log($"订单完成！获得 {coffee.value} 金币，当前顾客数: {waitingCustomers.Count}");

            // 尝试生成新顾客
            if (waitingCustomers.Count < maxCustomers && canSpawnCustomers)
            {
                float nextSpawnDelay = Random.Range(5f, 15f);
                Invoke("SpawnCustomer", nextSpawnDelay);
            }
        }
        else
        {
            Debug.LogWarning("尝试完成不存在的顾客订单");
        }
    }

    // 修改：当顾客生气离开时也要释放生成点
    public void CustomerLeftAngry(Customer customer)
    {
        if (waitingCustomers.Contains(customer))
        {
            // 释放顾客占用的生成点
            if (customerSpawnPointMap.ContainsKey(customer))
            {
                Transform spawnPoint = customerSpawnPointMap[customer];
                Debug.Log($"顾客生气离开，释放生成点: {spawnPoint.name}");
                customerSpawnPointMap.Remove(customer);
            }

            // 从等待列表中移除
            waitingCustomers.Remove(customer);
            Destroy(customer.gameObject);

            Debug.Log("顾客生气离开，当前顾客数: " + waitingCustomers.Count);

            // 尝试生成新顾客
            if (waitingCustomers.Count < maxCustomers && canSpawnCustomers)
            {
                float nextSpawnDelay = Random.Range(5f, 15f);
                Invoke("SpawnCustomer", nextSpawnDelay);
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