using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] int startingCoins = 0;
    [SerializeField] TextMeshProUGUI hudCoinText; // optional - drag in your HUD coin label

    public int Coins { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Coins = startingCoins;
        RefreshDisplay();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        RefreshDisplay();
    }

    // Returns true and deducts if affordable, false otherwise.
    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        RefreshDisplay();
        return true;
    }

    void RefreshDisplay()
    {
        if (hudCoinText != null)
            hudCoinText.text = $"{Coins}c";
    }
}