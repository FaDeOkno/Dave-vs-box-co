using UnityEngine;

public partial class Movement
{
    [Header("Jump Stuff", order = 2)]
    [SerializeField] float jumpduration;
    [SerializeField] float maxjumpduration;
    [SerializeField] private float jumpForce = 10f;

    public bool isGrounded;
    public bool allowJump = true;
    public LayerMask groundLayerMask;
    public LayerMask groundNoJumpLayerMask;
    private bool usedDoubleJump = false;

    private void HandleJump()
    {
        if (!inputHander.JumpPressed())
            return;

        if (isGrounded && allowJump)
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

        if (hasDoubleJump && !usedDoubleJump && allowJump)
        {
            usedDoubleJump = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateGrounded()
    {
        var rayOrigin = (Vector2)transform.position + Collider.offset + Vector2.down * Collider.size.y;

        isGrounded = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, groundLayerMask);
        allowJump = !Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, groundNoJumpLayerMask);

        if (isGrounded)
        {
            cancelWallHold = false;
            usedDoubleJump = false;
        }
    }
}
