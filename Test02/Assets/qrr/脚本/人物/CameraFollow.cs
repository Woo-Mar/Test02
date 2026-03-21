using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;              // 要跟随的目标（新玩家）

    [Header("跟随设置")]
    public float smoothSpeed = 0.125f;     // 跟随平滑度
    public Vector3 offset = new Vector3(0, 0, -10);  // 摄像机偏移

    [Header("边界限制")]
    public bool useBoundaries = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

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
}