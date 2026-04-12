using UnityEngine;
using UnityEngine.InputSystem;

public partial class Movement
{
    // ── Wall Sliding ──────────────────────────────────────────────
    [Header("Wall Sliding", order = 5)]
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] private bool isOnWall = false;
    [SerializeField] private bool canHoldWall = false;
    [SerializeField] private bool cancelWallHold = false;

    private WallSide whichWallWasTouched = WallSide.None;
    private bool isWallHolding = false;

    private void WallJump(Vector2 horizontalDir)
    {
        canHoldWall = false;
        isOnWall = false;
        cancelWallHold = true;
        rb.gravityScale = 1f;

        Debug.Log($"Wall Jump! Horizontal dir: {horizontalDir}");
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(horizontalDir * wallJumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * wallJumpForce, ForceMode2D.Impulse);
        Flip();

        useInputs = false;
        Invoke(nameof(EnableInputs), 0.2f);
    }

    void EnableInputs()
    {
        useInputs = true;
        cancelWallHold = false;
    }

    // ── Wall Hold Helpers ─────────────────────────────────────────

    public void OnWallHold(InputAction.CallbackContext context)
    {
        if (context.performed && !InputBlocker.IsBlocked)
            isWallHolding = true;
        else
            isWallHolding = false;
    }

    private void HandleGravity()
    {
        rb.gravityScale = (canHoldWall && isOnWall) ? 0f : 1f;
    }

    private void UpdateWallHold()
    {
        canHoldWall = isWallHolding && !cancelWallHold;

        var facingDir = facingRight ? Vector2.right : Vector2.left;
        var rayOrigin = (Vector2)transform.position + (facingDir * Collider.size.x / 2) + (facingRight ? Collider.offset : -Collider.offset);

        var ray = Physics2D.Raycast(rayOrigin, facingDir, 0.1f, groundLayerMask);
        if (!ray || !canHoldWall)
        {
            isOnWall = false;
            whichWallWasTouched = WallSide.None;
            return;
        }

        isOnWall = true;
        whichWallWasTouched = ray.collider != null && ray.normal.x > 0 ? WallSide.Left : WallSide.Right;
    }
}
