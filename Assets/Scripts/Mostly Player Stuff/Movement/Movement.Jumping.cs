using UnityEngine;

public partial class Movement
{
    [Header("Jump Stuff", order = 2)]
    [SerializeField] float jumpduration;
    [SerializeField] float maxjumpduration;
    [SerializeField] Transform groundCheck;

    public bool isGrounded;
    public LayerMask groundLayerMask;
    private bool usedDoubleJump = false;

    #region Jumping
    private void HandleJump()
    {
        if (!inputHander.JumpPressed())
            return;

        if (isGrounded)
        {
            usedDoubleJump = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            return;
        }

        if (whichWallWasTouched != WallSide.None)
        {
            WallJump(whichWallWasTouched == WallSide.Left ? Vector2.right : Vector2.left);
            return;
        }

        if (hasDoubleJump && !usedDoubleJump)
        {
            usedDoubleJump = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayerMask);

        if (isGrounded)
        {
            cancelWallHold = false;
            usedDoubleJump = false;
        }
    }
    #endregion
}
