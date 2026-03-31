using UnityEngine;

public class SelectMagToUse : MonoBehaviour
{
    public Movement movement;
    public InputHander inputHander;

    void Update()
    {
        if(inputHander.Mag1Pressed())
        {
            movement.WhatMagnetToUse = Movement.MagnetType.Pull;
            Debug.Log("Pull Magnet activated");
        }
        else if(inputHander.Mag2Pressed())
        {
            movement.WhatMagnetToUse = Movement.MagnetType.Push;
            Debug.Log("Push Magnet activated");
        }
        else if(inputHander.Mag3Pressed())
        {
            movement.WhatMagnetToUse = Movement.MagnetType.Lazer;
            Debug.Log("Lazer Magnet activated");
        }
    }
}
