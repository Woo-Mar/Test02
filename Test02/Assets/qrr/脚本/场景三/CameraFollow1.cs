using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;              // 要跟随的目标（玩家）

    [Header("跟随设置")]
    public float smoothSpeed = 0.125f;     // 跟随平滑度
    public Vector3 offset = new Vector3(0, 0, -10);  // 摄像机偏移

    [Header("边界限制（长方形场景）")]
    public bool useBoundaries = true;
    public float minX = -10f;    // 左边边界
    public float maxX = 10f;     // 右边边界
    public float minY = -5f;     // 下边界
    public float maxY = 5f;      // 上边界

    void LateUpdate()
    {
        if (target == null)
        {
            // 如果没有指定目标，自动查找
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                return;
        }

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 边界限制
        if (useBoundaries)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // 平滑跟随
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // 设置新目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 在Scene视图中显示边界（方便调试）
    void OnDrawGizmosSelected()
    {
        if (useBoundaries)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}