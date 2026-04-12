using UnityEngine;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] GameObject shopRoot;
    [SerializeField] GameObject itemsPanel;

    [Header("Coin Display (inside ItemsPanel)")]
    [SerializeField] TextMeshProUGUI coinText;

    [Header("Item Rows — drag all ShopItemRow objects here")]
    [SerializeField] ShopItemRow[] itemRows;

    ShopManager currentShop;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        shopRoot.SetActive(false);
    }

    void OnDisable()
    {
        InputBlocker.Blockers.Remove(this);
    }

    public void OpenItems(ShopManager shop)
    {
        currentShop = shop;
        shopRoot.SetActive(true);
        itemsPanel.SetActive(true);
        UpdateCoinDisplay();
        RefreshItems();
        InputBlocker.Blockers.Add(this);
    }

    public void Close()
    {
        shopRoot.SetActive(false);
        InputBlocker.Blockers.Remove(this);
    }

    public void OnBackPressed() => Close();

    public void OnBuyItem(int index)
    {
        if (currentShop.TryBuyItem(index))
        {
            Close();
            currentShop.PlayRandomPurchaseDialogue();
        }
        else
        {
            RefreshItems();
        }
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
            int capturedIndex = i;

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
