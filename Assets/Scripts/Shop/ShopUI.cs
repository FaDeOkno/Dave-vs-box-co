using UnityEngine;
using TMPro;

// Put this on a persistent Canvas GameObject (same scene as the shop NPC).
// UI hierarchy to build in the Canvas:
//
//  ShopRoot (GameObject, starts disabled)
//  ├── MainMenuPanel
//  │   ├── Button "Talk"   → OnClick → ShopUI.OnTalkPressed()
//  │   ├── Button "Buy"    → OnClick → ShopUI.OnBuyPressed()
//  │   └── Button "Leave"  → OnClick → ShopUI.OnLeavePressed()
//  └── ItemsPanel
//      ├── CoinText (TMP)
//      ├── ItemRow0  (ShopItemRow component)
//      ├── ItemRow1  (ShopItemRow component)
//      ├── ItemRow2  (ShopItemRow component)
//      ├── ItemRow3  (ShopItemRow component)
//      ├── ItemRow4  (ShopItemRow component)
//      └── Button "Back"   → OnClick → ShopUI.OnBackPressed()

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] GameObject shopRoot;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject itemsPanel;

    [Header("Coin Display (inside ItemsPanel)")]
    [SerializeField] TextMeshProUGUI coinText;

    [Header("Item Rows — drag all 5 ShopItemRow objects here")]
    [SerializeField] ShopItemRow[] itemRows;

    ShopManager currentShop;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        shopRoot.SetActive(false);
    }

    // Called by ShopManager when the player opens the shop.
    public void Open(ShopManager shop)
    {
        currentShop = shop;
        shopRoot.SetActive(true);
        ShowMainMenu();
        InputHander.Instance.DisableInputs();
    }

    public void Close()
    {
        shopRoot.SetActive(false);
        InputHander.Instance.EnableInputs();
    }

    // ── Button callbacks (wire these in the Inspector) ────────────

    public void OnTalkPressed()
    {
        Close();
        currentShop.StartTalk();
    }

    public void OnBuyPressed()
    {
        mainMenuPanel.SetActive(false);
        itemsPanel.SetActive(true);
        RefreshItems();
    }

    public void OnLeavePressed() => Close();

    public void OnBackPressed() => ShowMainMenu();

    // Pass the row index from the buy button's OnClick UnityEvent (0-4).
    public void OnBuyItem(int index)
    {
        currentShop.TryBuyItem(index);
        RefreshItems();
    }

    // ── Internal ──────────────────────────────────────────────────

    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        itemsPanel.SetActive(false);
        UpdateCoinDisplay();
    }

    void RefreshItems()
    {
        UpdateCoinDisplay();
        ShopItemData[] items = currentShop.items;

        for (int i = 0; i < itemRows.Length; i++)
        {
            if (i >= items.Length)
            {
                itemRows[i].gameObject.SetActive(false);
                continue;
            }

            ShopItemData item = items[i];
            bool soldOut = item.maxPurchases != -1 && item.purchaseCount >= item.maxPurchases;
            bool canAfford = CurrencyManager.Instance.Coins >= item.price;
            int capturedIndex = i; // capture for lambda

            itemRows[i].gameObject.SetActive(true);
            itemRows[i].nameText.text = item.itemName;
            itemRows[i].priceText.text = $"{item.price}c";
            itemRows[i].buyButton.onClick.RemoveAllListeners();
            itemRows[i].buyButton.onClick.AddListener(() => OnBuyItem(capturedIndex));
            itemRows[i].buyButton.interactable = !soldOut && canAfford;
            itemRows[i].buyButtonText.text = soldOut ? "Owned" : "Buy";
        }
    }

    void UpdateCoinDisplay()
    {
        if (coinText != null)
            coinText.text = $"Coins: {CurrencyManager.Instance.Coins}";
    }
}