using UnityEngine;

public class EnablesMovementBecauseItKeepsGettingDisabledIdk : MonoBehaviour
{
    public Movement movement;
    private void Start()
    {
        movement.enabled = true;
    }
}
