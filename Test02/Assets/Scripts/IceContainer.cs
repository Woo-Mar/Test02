// IceContainer.cs - 冰块容器
using UnityEngine;

/// <summary>
/// 冰块容器控制器 - 处理加冰功能
/// 点击容器给附近的咖啡杯加冰
/// </summary>
public class IceContainer : MonoBehaviour
{
    public GameObject iceCubePrefab;    // 冰块特效预制体
    public Transform iceSpawnPoint;     // 冰块生成位置

    /// <summary>
    /// 鼠标点击冰块容器事件
    /// </summary>
    void OnMouseDown()
    {
        // 获取场景中所有杯子
        Cup[] cups = FindObjectsOfType<Cup>();

        // 遍历检查每个杯子
        foreach (Cup cup in cups)
        {
            // 检查条件：有咖啡、没加冰、在容器附近
            if (cup.hasCoffee && !cup.hasIce &&
                Vector2.Distance(cup.transform.position, transform.position) < 500f) // 距离判断
            {
                // 步骤4：给杯子加冰
                cup.AddIce();

                // 生成冰块视觉效果
                GameObject ice = Instantiate(iceCubePrefab, iceSpawnPoint.position, Quaternion.identity);

                // 添加自动销毁组件（1秒后消失）
                AutoDestroy autoDestroy = ice.AddComponent<AutoDestroy>();
                autoDestroy.destroyDelay = 1f;
                autoDestroy.fadeOut = true;
                autoDestroy.fadeDuration = 0.5f;

                Debug.Log("冰块已加入咖啡");
                break; // 只处理一个杯子
            }
        }
    }
}