using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Put this on each item row GameObject inside your shop items panel.
// You need 5 of these set up in the Canvas, then drag them into ShopUI.itemRows[].
public class ShopItemRow : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText; // text inside the buy button
}