using UnityEngine;

public partial class Movement
{
    // ── Movement ─────────────────────────────────────────────────
    [Header("Movement", order = 1)]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float midAirAcceleration = 10f;
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
            var movingUp = rb.linearVelocity.y > 0;
            rb.linearVelocity = rb.linearVelocity - (rb.linearVelocity * Time.fixedDeltaTime * 5f);

            if (movingUp != rb.linearVelocity.y > 0)
                rb.linearVelocity = Vector2.zero;
        }

        else if (isQuickDashing)
        {
            UpdateQuickDash();
            return;
        }

        else if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.AddForceX(moveInput.x * midAirAcceleration, ForceMode2D.Force);
            rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed), rb.linearVelocity.y);
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
