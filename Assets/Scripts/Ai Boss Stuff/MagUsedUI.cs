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
        if(movement.WhatMagnetToUse == "pull")
        {
            UiImage.sprite = PullSpr;
        }
        else if(movement.WhatMagnetToUse == "push")
        {
            UiImage.sprite = PushSpr;
        }
        else if(movement.WhatMagnetToUse == "lazer")
        {
            UiImage.sprite = LaserSpr;
        }
    }
}
