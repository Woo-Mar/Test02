using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;

        // 如果没有找到主摄像机，报错提示
        if (mainCamera == null)
        {
            Debug.LogError("❌ 找不到Main Camera！请确保场景中有摄像机并Tag设置为MainCamera");
        }
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // 让血条始终面向摄像机
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }
}