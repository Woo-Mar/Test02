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
    public Transform customerSpawnPoint;      // 顾客生成点
    public GameObject customerPrefab;          // 顾客预制体

    [Header("顾客管理")]
    public int maxCustomers = 5;               // 最大顾客数量
    public bool canSpawnCustomers = true;      // 是否允许生成新顾客

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

    /// <summary>
    /// 生成新顾客
    /// </summary>
    public void SpawnCustomer()
    {
        // 检查是否达到最大顾客数量限制
        if (waitingCustomers.Count >= maxCustomers)
        {
            Debug.Log($"已达到最大顾客数({maxCustomers})，等待有空位");
            // 等待10秒后重新尝试生成
            Invoke("SpawnCustomer", 10f);
            return;
        }

        // 生成新顾客对象
        Vector3 spawnPos = customerSpawnPoint.position;
        spawnPos.z = 0; // 确保Z轴为0
        GameObject newCustomer = Instantiate(customerPrefab, spawnPos, Quaternion.identity);

        // 确保顾客位置Z轴为0
        Vector3 customerPos = newCustomer.transform.position;
        customerPos.z = 0;
        newCustomer.transform.position = customerPos;

        // 获取顾客脚本组件
        Customer customerScript = newCustomer.GetComponent<Customer>();
        if (customerScript != null)
        {
            // 随机设置顾客订单要求（50%概率要冰咖啡）
            customerScript.wantsIcedCoffee = Random.value > 0.5f;
            waitingCustomers.Add(customerScript);

            Debug.Log($"新顾客到达！想要{(customerScript.wantsIcedCoffee ? "冰咖啡" : "热咖啡")}，当前顾客数: {waitingCustomers.Count}");
        }

        // 根据当前顾客数量决定下次生成时间
        if (canSpawnCustomers && waitingCustomers.Count < maxCustomers)
        {
            // 顾客未满时，20-40秒后生成下一个
            float nextSpawnTime = Random.Range(20f, 40f);
            Invoke("SpawnCustomer", nextSpawnTime);
        }
        else
        {
            // 顾客已满时，10秒后检查
            Invoke("SpawnCustomer", 10f);
        }
    }

    /// <summary>
    /// 完成订单处理
    /// </summary>
    /// <param name="coffee">完成的咖啡对象</param>
    /// <param name="customer">被服务的顾客</param>
    public void CompleteOrder(Coffee coffee, Customer customer)
    {
        if (waitingCustomers.Contains(customer))
        {
            // 从等待列表中移除顾客
            waitingCustomers.Remove(customer);
            Destroy(customer.gameObject); // 销毁顾客对象

            // 发放金币奖励
            GameManager.Instance.AddMoney(coffee.value);

            Debug.Log($"订单完成！获得 {coffee.value} 金币，当前顾客数: {waitingCustomers.Count}");

            // 顾客减少后尝试生成新顾客
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