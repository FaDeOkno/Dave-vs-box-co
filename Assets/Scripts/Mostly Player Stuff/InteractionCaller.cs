using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionCaller : MonoBehaviour
{
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private LayerMask interactionLayerMask;
    [SerializeField] private GameEvent onInteract;
    public bool canInteract = true;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (InputBlocker.IsBlocked)
            return;

        if (!canInteract)
            return;

        var cast = Physics2D.Raycast((Vector2)transform.position, Vector2.right * transform.localScale.x, interactionRange, interactionLayerMask);

        if (cast)
        {
            onInteract.Raise(this, cast.collider.gameObject, null);
            Debug.Log("Interaction");
        }
    }

    public void EnableInteraction() => canInteract = true;
    public void DisableInteraction() => canInteract = false;
    public void CooldownInteraction()
    {
        StartCoroutine(CooldownCoroutine(.5f));
    }

    private IEnumerator CooldownCoroutine(float duration)
    {
        DisableInteraction();
        yield return new WaitForSeconds(duration);
        EnableInteraction();
    }
}
