using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────
    public InputHander inputHander;
    public Magnet magnet;
    public SpriteRenderer spriteRenderer;
    public PointEffector2D pushPoint;
    public GameObject player;

    // ── Movement ─────────────────────────────────────────────────
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    public Rigidbody2D rb { get; private set; }
    public Vector2 moveInput;
    public bool CanMove = true;

    // ── Ground Check ─────────────────────────────────────────────
    public bool isGrounded;
    public LayerMask groundLayerMask;

    // ── Magnet / Push ─────────────────────────────────────────────
    public float magnetDuration;
    public float pushDuration;

    // ── Dash ──────────────────────────────────────────────────────
    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashTime = 0.2f;

    // ── Wall Sliding ──────────────────────────────────────────────
    [Header("Wall Sliding")]
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] private bool isOnWall = false;
    [SerializeField] private bool canHoldWall = false;
    [SerializeField] private bool cancelWallHold = false;

    private string whichWallWasTouched;
    private bool canJumpOffWall;

    // ── Animation / Facing ────────────────────────────────────────
    private Animator anim;
    private bool facingRight = true;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
        HandleFlip();
        Jump();
        HandleWallHold();
        HandleGravity();
    }

    private void FixedUpdate()
    {
        if (!isOnWall)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ── Input Callbacks ──────────────────────────────────────────

    public void Move()
    {
        if (CanMove && !isOnWall)
            moveInput = inputHander.GetHorizontalInput();
    }

    public void Jump()
    {
        if (!inputHander.JumpPressed()) return;

        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            return;
        }

        if (whichWallWasTouched == "L")
            WallJump(Vector2.right);
        else if (whichWallWasTouched == "R")
            WallJump(Vector2.left);
    }

    // ── Wall Jump ─────────────────────────────────────────────────

    private void WallJump(Vector2 horizontalDir)
    {
        canJumpOffWall = true;
        canHoldWall = false;
        isOnWall = false;
        cancelWallHold = true;
        rb.gravityScale = 1f;
        rb.AddForce(horizontalDir * wallJumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * wallJumpForce, ForceMode2D.Impulse);
        cancelWallHold = false;
    }

    // ── Wall Hold Helpers ─────────────────────────────────────────

    private void HandleWallHold()
    {
        // InputHander drives the wall-hold button check
        bool holdPressed = inputHander != null && inputHander.WallHoldPressed();

        if (holdPressed && !cancelWallHold)
            canHoldWall = true;
        else
            canHoldWall = false;
    }

    private void HandleGravity()
    {
        rb.gravityScale = (canHoldWall && isOnWall && !canJumpOffWall) ? 0f : 1f;
    }

    // ── Flip ──────────────────────────────────────────────────────

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

    // ── Dash ──────────────────────────────────────────────────────

    public void Dash()
    {
        CanMove = false;
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
        rb.linearVelocity = new Vector2(moveInput.x * dashForce, 0f);
        Invoke(nameof(EndDash), dashTime);
    }

    private void EndDash()
    {
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        CanMove = true;
    }

    // ── Magnet / Push ─────────────────────────────────────────────

    public void Pull() => StartCoroutine(PullTimer());

    private IEnumerator PullTimer()
    {
        magnet.isActive = true;
        yield return new WaitForSeconds(magnetDuration);
        magnet.isActive = false;
    }

    public void Push() => StartCoroutine(PushTimer());

    private IEnumerator PushTimer()
    {
        pushPoint.enabled = true;
        yield return new WaitForSeconds(pushDuration);
        pushPoint.enabled = false;
    }

    // ── Collision ─────────────────────────────────────────────────

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer != 8)
        {
            isGrounded = true;
            cancelWallHold = false;
        }
        else
        {
            if (canHoldWall)
            {
                isOnWall = true;
                isGrounded = false;

                if (collision.gameObject.CompareTag("LWall"))
                    whichWallWasTouched = "L";
                else if (collision.gameObject.CompareTag("RWall"))
                    whichWallWasTouched = "R";
            }
            else
            {
                isOnWall = false;
                isGrounded = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        isOnWall = false;
        whichWallWasTouched = null;
        canJumpOffWall = false;
    }
}