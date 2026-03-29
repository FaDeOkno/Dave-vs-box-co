using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopChoiceUI : MonoBehaviour
{
    public static ShopChoiceUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Button upgradeButton;
    [SerializeField] Button talkButton;
    [SerializeField] Button leaveButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(Action onUpgrade, Action onTalk, Action onLeave)
    {
        panel.SetActive(true);

        upgradeButton.onClick.RemoveAllListeners();
        talkButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();

        upgradeButton.onClick.AddListener(() => { Hide(); onUpgrade?.Invoke(); });
        talkButton.onClick.AddListener(() => { Hide(); onTalk?.Invoke(); });
        leaveButton.onClick.AddListener(() => { Hide(); onLeave?.Invoke(); });
    }

    void Hide()
    {
        panel.SetActive(false);
        DialogueManager.Instance.CloseCanvas();
    }
}
