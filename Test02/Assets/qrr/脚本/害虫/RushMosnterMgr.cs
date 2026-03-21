using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushMosnterMgr : MonoBehaviour
{
    public List<GameObject> m_RushMosntList;  // 两种害虫预制体

    [Header("刷怪点设置")]
    public Transform rightSpawnPoint;      // 右边刷怪点
    public Transform leftSpawnPoint;       // 左边刷怪点
    public Transform rightPath;             // 右边路径
    public Transform leftPath;              // 左边路径

    [Header("生成设置")]
    public float minSpawnTime = 4f;
    public float maxSpawnTime = 10f;
    public int maxMonstersAtSameTime = 8;

    [Header("昼夜控制")]
    public bool isNight = false;            // 是否夜晚
    public bool canSpawn = false;           // 是否可以生成害虫

    private float m_CurTime;
    private List<GameObject> activeMonsters = new List<GameObject>();
    private int totalCropsPlanted = 0;

    void Start()
    {
        // 检查刷怪点设置
        if (rightSpawnPoint == null || leftSpawnPoint == null)
        {
            Debug.LogError("请设置左右刷怪点！");
        }

        if (rightPath == null || leftPath == null)
        {
            Debug.LogError("请设置左右路径！");
        }

        // 订阅种植事件
        EventManager1.StartListening("OnCropPlanted", OnCropPlanted);
    }

    void OnDestroy()
    {
        EventManager1.StopListening("OnCropPlanted", OnCropPlanted);
    }

    void Update()
    {
        // 如果不是夜晚或者不能生成，直接返回
        if (!isNight || !canSpawn)
            return;

        // 清理已销毁的怪物
        activeMonsters.RemoveAll(monster => monster == null);

        // 如果达到最大数量，不生成
        if (activeMonsters.Count >= maxMonstersAtSameTime)
            return;

        m_CurTime += Time.deltaTime;

        float currentSpawnTime = GetCurrentSpawnTime();

        if (m_CurTime >= currentSpawnTime)
        {
            m_CurTime = 0;
            SpawnMonster();
        }
    }

    public void SetNightMode(bool night)
    {
        isNight = night;
        canSpawn = night;  // 只有夜晚才能生成

        if (night)
        {
            Debug.Log("🌙 夜晚模式：开始生成害虫");
        }
        else
        {
            Debug.Log("☀️ 白天模式：停止生成害虫");
            // 白天可以选择是否清除现有害虫
            // ClearAllMonsters(); // 如果需要白天清除害虫，取消注释
        }
    }

    void SpawnMonster()
    {
        // 检查是否有作物存在
        NongZuoWuC[] crops = FindObjectsOfType<NongZuoWuC>();
        if (crops.Length == 0)
        {
            Debug.Log("没有作物，不生成害虫");
            return;
        }

        // 随机选择左边或右边刷怪点
        bool spawnOnRight = Random.value > 0.5f;
        Transform spawnPoint = spawnOnRight ? rightSpawnPoint : leftSpawnPoint;
        Transform path = spawnOnRight ? rightPath : leftPath;

        // 夜晚有概率一次生成两只
        if (Random.value < 0.3f)  // 30%概率双倍生成
        {
            StartCoroutine(SpawnDoubleMonster(spawnPoint, path));
        }
        else
        {
            SpawnSingleMonster(spawnPoint, path);
        }
    }

    void SpawnSingleMonster(Transform spawnPoint, Transform path)
    {
        // 随机选择怪物类型（0或1）
        int monsterType = Random.Range(0, m_RushMosntList.Count);
        GameObject monsterPrefab = m_RushMosntList[monsterType];

        // 生成怪物
        GameObject newMonster = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);

        // 设置怪物的路径
        EnemyPathWalker enemyWalker = newMonster.GetComponent<EnemyPathWalker>();
        if (enemyWalker != null && path != null)
        {
            enemyWalker.InitData(path);
        }

        activeMonsters.Add(newMonster);

        Debug.Log($"🐛 生成害虫 类型{monsterType}，当前数量：{activeMonsters.Count}/{maxMonstersAtSameTime}");
    }

    IEnumerator SpawnDoubleMonster(Transform spawnPoint, Transform path)
    {
        Debug.Log("🌙 双倍生成！");

        // 生成第一只
        SpawnSingleMonster(spawnPoint, path);
        yield return new WaitForSeconds(0.3f);

        // 生成第二只
        SpawnSingleMonster(spawnPoint, path);
    }

    float GetCurrentSpawnTime()
    {
        // 夜晚固定生成时间
        return Random.Range(minSpawnTime, maxSpawnTime);
    }

    void OnCropPlanted()
    {
        totalCropsPlanted++;
        // 种植越多，夜晚害虫越多（可选）
        // maxMonstersAtSameTime = 8 + totalCropsPlanted / 5;
    }

    // 清除所有怪物
    public void ClearAllMonsters()
    {
        foreach (GameObject monster in activeMonsters)
        {
            if (monster != null)
                Destroy(monster);
        }
        activeMonsters.Clear();
        Debug.Log("清除所有害虫");
    }
}