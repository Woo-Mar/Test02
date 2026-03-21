using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("商店系统")]
    public ShopSystem shopSystem;

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (shopSystem == null)
            shopSystem = FindObjectOfType<ShopSystem>();

        Debug.Log("商店触发器初始化完成");
    }

    void OnMouseDown()
    {
        Debug.Log("点击了商店");

        if (shopSystem != null)
        {
            shopSystem.OpenShop();
            Debug.Log("打开商店面板");
        }
        else
        {
            Debug.LogError("找不到ShopSystem！");
        }
    }
}