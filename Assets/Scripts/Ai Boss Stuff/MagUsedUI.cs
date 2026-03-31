using UnityEngine;
using UnityEngine.UI;

public class MagUsedUI : MonoBehaviour
{
    [SerializeField] Movement movement;

    [Header("Different Magnet Sprites")]
    [SerializeField] Image UiImage;
    [Space]
    [SerializeField] Sprite PullSpr;
    [SerializeField] Sprite PushSpr;
    [SerializeField] Sprite LaserSpr;

    private void Update()
    {
        switch (movement.WhatMagnetToUse)
        {
            case Movement.MagnetType.Pull:
                UiImage.sprite = PullSpr;
                break;
            case Movement.MagnetType.Push:
                UiImage.sprite = PushSpr;
                break;
            case Movement.MagnetType.Lazer:
                UiImage.sprite = LaserSpr;
                break;
        }
    }
}
