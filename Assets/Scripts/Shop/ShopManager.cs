using UnityEngine;

// ── Shop item data ────────────────────────────────────────────────

public enum ShopItemType { DoubleJump, Dash, ExtraHeart, MoreStamina, BiggerRay }

[System.Serializable]
public class ShopItemData
{
    public string itemName;
    [TextArea(1, 3)] public string description;
    public int price;
    public ShopItemType type;
    // maxPurchases: 1 = one-time upgrade, -1 = buy as many times as you want
    public int maxPurchases = 1;
    [HideInInspector] public int purchaseCount;
}

// ── ShopManager ───────────────────────────────────────────────────
// Put this on the shop NPC GameObject.
// The NPC needs a Collider2D set to Is Trigger so the player can enter its range.

public class ShopManager : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] DialogueData npcDialogue;     // drag a DialogueData asset here for Talk
    [SerializeField] GameObject interactPrompt;    // "Z: Talk / Shop" prompt, starts disabled

    [Header("Player References")]
    [SerializeField] Movement movement;
    [SerializeField] HeartUI heartUI;

    [Header("Shop Items")]
    public ShopItemData[] items;

    bool playerInRange;

    // Set sensible defaults when the component is first added to a GameObject.
    void Reset()
    {
        items = new ShopItemData[]
        {
            new ShopItemData { itemName = "Double Jump",  description = "Jump a second time in mid-air!",        price = 200, type = ShopItemType.DoubleJump,  maxPurchases = 1  },
            new ShopItemData { itemName = "Dash",         description = "Dash forward at lightning speed.",       price = 150, type = ShopItemType.Dash,         maxPurchases = 1  },
            new ShopItemData { itemName = "Extra Heart",  description = "Restore one heart of max health.",       price = 75,  type = ShopItemType.ExtraHeart,  maxPurchases = -1 },
            new ShopItemData { itemName = "More Stamina", description = "Magnet lasts 1.5 seconds longer.",       price = 125, type = ShopItemType.MoreStamina, maxPurchases = -1 },
            new ShopItemData { itemName = "Bigger Ray",   description = "Your laser grows wider and longer.",     price = 175, type = ShopItemType.BiggerRay,   maxPurchases = -1 },
        };
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            OpenShop();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        playerInRange = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        playerInRange = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void OpenShop()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        ShopUI.Instance.Open(this);
    }

    // Called by ShopUI's "Talk" button.
    public void StartTalk()
    {
        if (npcDialogue != null)
            DialogueManager.Instance.StartDialogue(npcDialogue);
    }

     public bool TryBuyItem(int index)
    {
        if (index < 0 || index >= items.Length) return false;

        ShopItemData item = items[index];

        if (item.maxPurchases != -1 && item.purchaseCount >= item.maxPurchases) return false;
        if (!CurrencyManager.Instance.SpendCoins(item.price)) return false;

        item.purchaseCount++;
        ApplyUpgrade(item.type);
        return true;
    }

    void ApplyUpgrade(ShopItemType type)
    {
        switch (type)
        {
            case ShopItemType.DoubleJump:
                movement.hasDoubleJump = true;
                break;
            case ShopItemType.Dash:
                movement.hasDash = true;
                break;
            case ShopItemType.ExtraHeart:
                heartUI.AddHeart();
                break;
            case ShopItemType.MoreStamina:
                movement.magnetDuration += 1.5f;
                break;
            case ShopItemType.BiggerRay:
                movement.UpgradeLaser();
                break;
        }
    }
}