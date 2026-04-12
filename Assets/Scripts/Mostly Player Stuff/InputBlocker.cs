using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputBlocker
{
    public static List<object> Blockers = new();

    public static bool IsBlocked { get => Blockers.Count > 0; }
}
