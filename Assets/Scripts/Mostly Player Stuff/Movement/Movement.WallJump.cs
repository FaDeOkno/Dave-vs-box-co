using UnityEngine;

public partial class Movement
{
    // ── Wall Sliding ──────────────────────────────────────────────
    [Header("Wall Sliding", order = 5)]
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] private bool isOnWall = false;
    [SerializeField] private bool canHoldWall = false;
    [SerializeField] private bool cancelWallHold = false;
    [SerializeField] Transform wallCheck;

    private WallSide whichWallWasTouched = WallSide.None;
    private bool canJumpOffWall;

    #region Wall Jumping
    private void WallJump(Vector2 horizontalDir)
    {
        canJumpOffWall = true;
        canHoldWall = false;
        isOnWall = false;
        cancelWallHold = true;
        rb.gravityScale = 1f;

        Debug.Log($"Wall Jump! Horizontal dir: {horizontalDir}");
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(horizontalDir * wallJumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * wallJumpForce, ForceMode2D.Impulse);

        useInputs = false;
        Invoke(nameof(EnableInputs), 0.2f);
    }

    void EnableInputs()
    {
        useInputs = true;
        cancelWallHold = false;
    }

    // ── Wall Hold Helpers ─────────────────────────────────────────

    private bool CanHoldWall()
    {
        // InputHander drives the wall-hold button check
        bool holdPressed = inputHander != null && inputHander.WallHoldPressed();

        return holdPressed && !cancelWallHold;
    }

    private void HandleGravity()
    {
        rb.gravityScale = (canHoldWall && isOnWall && !canJumpOffWall) ? 0f : 1f;
    }

    private void UpdateWallHold()
    {
        canHoldWall = CanHoldWall();

        var ray = Physics2D.Raycast(wallCheck.position, facingRight ? Vector2.right : Vector2.left, 0.1f, groundLayerMask);
        if (!ray || !canHoldWall)
        {
            isOnWall = false;
            whichWallWasTouched = WallSide.None;
            return;
        }

        isOnWall = true;
        whichWallWasTouched = ray.collider != null && ray.normal.x > 0 ? WallSide.Left : WallSide.Right;
    }
    #endregion
}
