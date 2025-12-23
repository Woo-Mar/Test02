// CoffeeMachine.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 咖啡机控制器 - 管理咖啡制作全流程
/// 处理研磨、萃取、杯子管理等核心功能
/// </summary>
public class CoffeeMachine : MonoBehaviour
{
    [Header("UI Elements")]
    public Button grindButton;        // 研磨按钮
    public Button brewButton;         // 萃取按钮
    public Image coffeePowderUI;     // 咖啡粉UI显示
    public Image brewedCoffeeUI;     // 咖啡液UI显示
    public Transform cupPosition;     // 杯子放置位置

    [Header("States")]
    public bool hasCoffeeBeans = true;    // 是否有咖啡豆
    public bool hasGroundCoffee = false;  // 是否有研磨咖啡粉
    public bool hasBrewedCoffee = false;  // 是否已萃取咖啡

    [Header("Prefabs")]
    public GameObject coffeePowderPrefab;  // 咖啡粉特效预制体
    public GameObject coffeeLiquidPrefab;  // 咖啡液流动特效
    public GameObject cupPrefab;           // 杯子预制体

    [Header("Cup Management")]
    public List<GameObject> availableCups = new List<GameObject>(); // 可用杯子列表
    public int maxCups = 5;                 // 最大杯子数量

    public GameObject currentCup;           // 当前放置在咖啡机上的杯子
    private Coffee currentCoffee = new Coffee(); // 当前制作的咖啡数据

    [Header("杯子容器引用")]
    public CupContainer cupContainer;      // 杯子容器引用

    void Start()
    {
        // 初始化按钮点击事件
        grindButton.onClick.AddListener(GrindCoffee);
        brewButton.onClick.AddListener(BrewCoffee);

        UpdateUI(); // 更新UI状态
    }

    /// <summary>
    /// 生成新杯子到场景中
    /// </summary>
    void SpawnNewCup()
    {
        if (availableCups.Count < maxCups && cupPrefab != null)
        {
            // 直接使用 cupPosition 的位置来生成杯子
            Vector3 spawnPos = cupPosition.position;
            spawnPos.z = 0; // 强制设置Z轴为0（2D场景深度控制）

            GameObject newCup = Instantiate(cupPrefab, spawnPos, Quaternion.identity);

            // 确保实例化的杯子Z轴为0
            Vector3 cupPos = newCup.transform.position;
            cupPos.z = 0;
            newCup.transform.position = cupPos;

            availableCups.Add(newCup);
            Debug.Log($"生成新杯子，当前可用杯子: {availableCups.Count}");
        }
    }

    /// <summary>
    /// 更新咖啡机UI状态
    /// </summary>
    public void UpdateUI()
    {
        // 显示/隐藏咖啡粉和咖啡液UI
        coffeePowderUI.gameObject.SetActive(hasGroundCoffee);
        brewedCoffeeUI.gameObject.SetActive(hasBrewedCoffee);

        // 设置按钮交互状态
        grindButton.interactable = hasCoffeeBeans && !hasGroundCoffee;
        brewButton.interactable = hasGroundCoffee && currentCup != null;
    }

    /// <summary>
    /// 步骤1：研磨咖啡豆
    /// </summary>
    public void GrindCoffee()
    {
        if (hasCoffeeBeans && !hasGroundCoffee)
        {
            hasGroundCoffee = true;
            currentCoffee.hasCoffeePowder = true;

            // 生成咖啡粉视觉效果
            StartCoroutine(SpawnCoffeePowderEffect());

            Debug.Log("咖啡研磨完成！");
            UpdateUI();
        }
    }

    /// <summary>
    /// 咖啡粉特效协程
    /// </summary>
    IEnumerator SpawnCoffeePowderEffect()
    {
        GameObject powder = Instantiate(coffeePowderPrefab, transform.position + Vector3.up, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(powder);
    }

    /// <summary>
    /// 步骤2：萃取咖啡
    /// </summary>
    public void BrewCoffee()
    {
        if (hasGroundCoffee && currentCup != null)
        {
            hasGroundCoffee = false;
            hasBrewedCoffee = true;
            currentCoffee.hasBrewedCoffee = true;

            // 在杯子中生成咖啡液体
            if (currentCup.GetComponent<Cup>() != null)
            {
                currentCup.GetComponent<Cup>().FillWithCoffee();
                currentCoffee.isInCup = true;

                // 播放咖啡流动效果
                StartCoroutine(SpawnCoffeeLiquidEffect());
            }

            Debug.Log("咖啡萃取完成！");
            UpdateUI();

            // 延迟重置咖啡机状态
            Invoke("ResetMachine", 1f);
        }
    }

    /// <summary>
    /// 咖啡液流动特效协程
    /// </summary>
    IEnumerator SpawnCoffeeLiquidEffect()
    {
        GameObject liquid = Instantiate(coffeeLiquidPrefab, transform.position, Quaternion.identity);
        liquid.transform.SetParent(currentCup.transform);
        yield return new WaitForSeconds(1.5f);
        Destroy(liquid);
    }

    /// <summary>
    /// 重置咖啡机状态（萃取完成后）
    /// </summary>
    void ResetMachine()
    {
        hasBrewedCoffee = false;
        UpdateUI();
    }

    /// <summary>
    /// 步骤3：放置杯子到咖啡机
    /// </summary>
    public void PlaceCup(GameObject cup)
    {
        Debug.Log($"尝试放置杯子，currentCup: {currentCup}, 传入的cup: {cup}");

        if (currentCup == null && cup != null)
        {
            Cup cupScript = cup.GetComponent<Cup>();
            if (cupScript != null && cupScript.isEmpty)
            {
                currentCup = cup;

                Debug.Log($"设置杯子位置到: {cupPosition.position}");

                // 设置杯子位置和父级变换
                currentCup.transform.position = cupPosition.position;
                currentCup.transform.SetParent(cupPosition);


                // 确保Z轴正确
                Vector3 pos = currentCup.transform.position;
                pos.z = -2f; // 硬编码为-2，或者从Cup脚本获取
                currentCup.transform.position = pos;

                // 从可用杯子列表中移除（已使用）
                if (availableCups.Contains(cup))
                {
                    availableCups.Remove(cup);
                }

                Debug.Log("杯子已放置到咖啡机");
                UpdateUI();
            }
            else
            {
                Debug.Log("杯子脚本为空或杯子不是空的");
            }
        }
        else
        {
            Debug.Log($"无法放置杯子：currentCup不为空({currentCup != null}) 或传入的cup为空({cup == null})");
        }
    }

    /// <summary>
    /// 从杯子容器获取杯子
    /// </summary>
    public void GetCupFromContainer()
    {
        if (currentCup != null)
        {
            Debug.Log("咖啡机上已有杯子");
            return;
        }

        if (cupContainer != null && cupContainer.HasCups())
        {
            cupContainer.TrySpawnCup();
        }
        else
        {
            Debug.Log("没有可用杯子");
        }
    }

    /// <summary>
    /// 检查是否可以取走咖啡
    /// </summary>
    public bool CanTakeCoffee()
    {
        // 只要有杯子且杯子里面有咖啡就可以取走
        return currentCup != null && currentCoffee.hasBrewedCoffee;
    }

    /// <summary>
    /// 取走咖啡（由拖拽系统处理）
    /// </summary>
    public GameObject TakeCoffee()
    {
        if (CanTakeCoffee())
        {
            GameObject coffeeToServe = currentCup;
            currentCup = null;
            currentCoffee.isComplete = true;

            Debug.Log("咖啡已取走");
            UpdateUI();

            return coffeeToServe;
        }
        return null;
    }

    /// <summary>
    /// 回收空杯子
    /// </summary>
    public void ReturnEmptyCup(GameObject cup)
    {
        if (cup != null)
        {
            Cup cupScript = cup.GetComponent<Cup>();
            if (cupScript != null && cupScript.isEmpty)
            {
                // 如果回收的杯子是当前杯子，清空引用
                if (currentCup == cup)
                {
                    Debug.Log("清空咖啡机上的当前杯子引用");
                    currentCup = null;
                }

                // 使用 cupPosition 的位置来重置杯子位置
                cup.transform.position = cupPosition.position;
                cup.transform.SetParent(null);

                // 确保Z轴正确
                Vector3 pos = cup.transform.position;
                pos.z = -2f; // 硬编码为-2
                cup.transform.position = pos;

                // 重新添加到可用杯子列表
                if (!availableCups.Contains(cup))
                {
                    availableCups.Add(cup);
                }

                Debug.Log($"空杯子已回收，当前可用杯子: {availableCups.Count}");
                UpdateUI();
            }
        }
    }

    /// <summary>
    /// 手动清空咖啡机上的杯子（调试用）
    /// </summary>
    public void ClearCurrentCup()
    {
        if (currentCup != null)
        {
            Debug.Log($"手动清空咖啡机上的杯子: {currentCup.name}");

            // 检查杯子是否为空，为空则回收
            Cup cupScript = currentCup.GetComponent<Cup>();
            if (cupScript != null && cupScript.isEmpty)
            {
                ReturnEmptyCup(currentCup);
            }
            else
            {
                // 非空杯子直接清空引用
                currentCup = null;
            }

            UpdateUI();
        }
    }

    /// <summary>
    /// 提供空杯子给咖啡机使用
    /// </summary>
    public void ProvideEmptyCup()
    {
        if (currentCup == null && availableCups.Count > 0)
        {
            GameObject emptyCup = availableCups[0];
            PlaceCup(emptyCup);
        }
        else if (availableCups.Count == 0)
        {
            Debug.Log("没有可用的杯子了！");
        }
    }
}