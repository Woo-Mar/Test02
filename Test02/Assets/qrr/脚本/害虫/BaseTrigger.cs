using UnityEngine;
using UnityEngine.UI;

public class BaseTrigger : MonoBehaviour
{
    [Header("伤害设置")]
    public int enemyDamage = 5;  // 敌人对作物的伤害

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("敌人碰到作物");

        if (collision.gameObject.tag == "Enemy")
        {
            NongZuoWuC crop = GetComponent<NongZuoWuC>();
            if (crop != null)
            {
                crop.CostJianKangPower(enemyDamage);
                Debug.Log($"💥 害虫碰到作物，造成{enemyDamage}伤害");
            }
        }
    }

    // 保留但废弃
    public void CostHp(int _value)
    {
        Debug.LogWarning("BaseTrigger.CostHp 已废弃");
    }
}