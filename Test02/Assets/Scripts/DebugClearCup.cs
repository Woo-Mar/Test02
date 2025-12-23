// DebugClearCup.cs - 调试脚本
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 调试清除杯子工具 - 用于开发测试
/// 提供快速清空咖啡机上杯子的功能
/// </summary>
public class DebugClearCup : MonoBehaviour
{
    public CoffeeMachine coffeeMachine; // 咖啡机引用

    void Start()
    {
        // 获取按钮组件并添加点击事件
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ClearCup);
        }
    }

    /// <summary>
    /// 清除咖啡机上的杯子（调试用）
    /// </summary>
    void ClearCup()
    {
        if (coffeeMachine != null)
        {
            coffeeMachine.ClearCurrentCup(); // 调用咖啡机的清空方法
        }
    }
}