using System;
using UnityEngine;
using UnityEngine.UI;

public class LouiseHealPrompt : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button acceptButton;
    [SerializeField] Button denyButton;

    public void Show(Action onAccept, Action onDeny)
    {
        panel.SetActive(true);

        acceptButton.onClick.RemoveAllListeners();
        denyButton.onClick.RemoveAllListeners();

        acceptButton.onClick.AddListener(() => { Hide(); onAccept?.Invoke(); });
        denyButton.onClick.AddListener(() => { Hide(); onDeny?.Invoke(); });
    }

    void Hide()
    {
        panel.SetActive(false);
    }
}
