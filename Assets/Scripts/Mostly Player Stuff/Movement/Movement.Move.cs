using UnityEngine;

public partial class Movement
{
    // ── Movement ─────────────────────────────────────────────────
    [Header("Movement", order = 1)]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    public Vector2 moveInput;
    public bool CanMove = true;

    private void GatherMoveInput()
    {
        if (CanMove && !isOnWall)
            moveInput = inputHander.GetHorizontalInput();
    }

    private void UpdateMovement()
    {
        if (!useInputs)
        {
            return;
        }

        if (isOnWall)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);

            if (rb.linearVelocity.sqrMagnitude < 0.01f)
                rb.linearVelocity = Vector2.zero;
        }

        else if (isQuickDashing)
        {
            UpdateQuickDash();
            return;
        }

        else
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }


    #region Flip
    private void HandleFlip()
    {
        if (moveInput.x > 0 && !facingRight) Flip();
        else if (moveInput.x < 0 && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion
}
