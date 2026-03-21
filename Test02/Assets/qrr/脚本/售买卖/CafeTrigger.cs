using UnityEngine;

public class CafeTrigger : MonoBehaviour
{
    [Header("咖啡馆系统")]
    public CafeSystem cafeSystem;

    void Start()
    {
        if (cafeSystem == null)
            cafeSystem = FindObjectOfType<CafeSystem>();
    }

    void OnMouseDown()
    {
        Debug.Log("点击了咖啡馆");

        if (cafeSystem != null)
        {
            cafeSystem.OpenCafe();
        }
        else
        {
            Debug.LogError("找不到CafeSystem！");
        }
    }
}