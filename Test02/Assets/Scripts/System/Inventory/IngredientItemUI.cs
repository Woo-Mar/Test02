// IngredientItemUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 原料UI项组件，用于快速访问UI元素
/// </summary>
public class IngredientItemUI : MonoBehaviour
{
    [Header("UI元素引用")]
    public Image iconImage;           // 图标
    public TMP_Text nameText;             // 名称
    public TMP_Text amountText;           // 数量
    public TMP_Text unitText;             // 单位
    public Slider progressSlider;     // 进度条（可选）
    public TMP_Text idText;               // ID（调试用，可隐藏）

    /// <summary>
    /// 更新UI显示
    /// </summary>
    public void UpdateUI(IngredientSystem.Ingredient ingredient)
    {
        if (ingredient == null) return;

        // 设置文本
        if (nameText != null) nameText.text = ingredient.name;
        if (amountText != null) amountText.text = ingredient.currentAmount.ToString();
        if (unitText != null) unitText.text = ingredient.unit;
        if (idText != null) idText.text = ingredient.id;

        // 设置图标
        if (iconImage != null && ingredient.icon != null)
        {
            iconImage.sprite = ingredient.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // 设置进度条
        if (progressSlider != null)
        {
            progressSlider.maxValue = ingredient.maxAmount;
            progressSlider.value = ingredient.currentAmount;

            // 根据库存量设置颜色
            Image fillImage = progressSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                float ratio = (float)ingredient.currentAmount / ingredient.maxAmount;
                if (ratio < 0.2f) fillImage.color = Color.red;
                else if (ratio < 0.5f) fillImage.color = Color.yellow;
                else fillImage.color = Color.green;
            }
        }
    }
}