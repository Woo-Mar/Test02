using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchaseItemUI : MonoBehaviour
{
    [Header("UI引用")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text amountText;
    public Button buyButton;

    private string ingredientId;
    private int price;
    private int amount;
    private PurchaseManager manager;

    public void Setup(string id, string name, int p, int amt, Sprite icon, PurchaseManager mngr)
    {
        ingredientId = id;
        nameText.text = name;
        price = p;
        amount = amt;
        amountText.text = "份量: " + amt + (id == "milk" ? "ml" : (id == "coffee" || id == "strawberry" ? "g" : "个"));
        priceText.text = p + " 金币";
        if (icon != null) iconImage.sprite = icon;
        manager = mngr;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => manager.TryPurchase(ingredientId, price, amount));
    }
}
