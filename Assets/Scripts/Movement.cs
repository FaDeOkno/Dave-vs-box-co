using System.Collections;
using UnityEditor.MPE;
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
    public HeartUI heartUI;
    public LayerMask DamageLayer;

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
    public float magnetDuration = 3f;   // how long pull stays active
    public float pushDuration = 0.5f; // how long push stays active

    private bool isPulling = false;
    private bool isPushing = false;
    private Coroutine pullRoutine;
    private Coroutine pushRoutine;

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

    // ── Quick Dash ──────────────────────────────────────────────────────
    [Header("Quick Dash")]
    [SerializeField] private float quickDashForce = 15f;
    [SerializeField] private float quickDashDuration = 0.12f;
    [SerializeField] float DashCooldownNum;
    private bool isQuickDashing = false;
    bool CanDash = true;

    // ─────────────────────────────────────────────────────────────

    // ── Die and Damage Code ─────────────────────────────────────────────────
    [Header("Damage and Death")]
    [SerializeField] Collider2D handLCollider;
    [SerializeField] Collider2D handRCollider;
    [SerializeField] Collider2D PlayerCollider;
    [SerializeField] float damageCooldown = .25f;

    bool gotDamaged = true;
    bool dontRepeatDamage = false;
    bool isTriggered;
    public void Die()
    {

    }

    IEnumerator DamageCooldown()
    {
        if (!dontRepeatDamage)
        {
            gotDamaged = true;
            dontRepeatDamage = true;
            heartUI.RemoveHeart();
            yield return new WaitForSeconds(damageCooldown);
            Debug.LogError("Damage cooldown finished");
            dontRepeatDamage = false;
            Debug.LogError("The damage repeat bool should be false");
            gotDamaged = false;
            yield break;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        PlayerCollider = GetComponent<Collider2D>();
        handLCollider = GameObject.FindGameObjectWithTag("LHand").GetComponent<Collider2D>();
        handRCollider = GameObject.FindGameObjectWithTag("RHand").GetComponent<Collider2D>();
    }

    private void Update()
    {
        GetInputs();
        /* So theres a bug where players dont get damaged
         * and this is the easiest solution
         * AKA: fucking up the variable names to not make sense
         * sorry...*/
        if (!gotDamaged)
        {
            Physics2D.IgnoreCollision(handLCollider, PlayerCollider, true);
            Physics2D.IgnoreCollision(handRCollider, PlayerCollider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(handLCollider, PlayerCollider, false);
            Physics2D.IgnoreCollision(handRCollider, PlayerCollider, false);
        }
    }
    private void GetInputs()
    {
        Move();
        HandleFlip();
        Jump();
        HandleWallHold();
        HandleGravity();
        Pull();
        Push();
        QuickDash();
    }
    private void FixedUpdate()
    {
        if (isQuickDashing)
        {
            // Maintain dash velocity during quick dash so FixedUpdate doesn't override it.
            float dir = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * quickDashForce, 0f);
            return;
        }

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

    public void QuickDash()
    {
        if (!inputHander.QuickDashPressed()) return;
        if (isQuickDashing) return;
        if (CanDash) StartCoroutine(QuickDashRoutine());
    }

    private IEnumerator QuickDashRoutine()
    {
        isQuickDashing = true;
        CanMove = false;
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;

        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * quickDashForce, 0f);

        yield return new WaitForSeconds(quickDashDuration);

        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        CanMove = true;
        isQuickDashing = false;
        StartCoroutine(DashCooldown());
    }
    IEnumerator DashCooldown()
    {
        CanDash = false;
        yield return new WaitForSeconds(DashCooldownNum);
        CanDash = true;
        yield break;
    }

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

    public void Pull()
    {
        if (!inputHander.PullObjPressed()) return;

        if (!isPulling)
        {
            // Activate
            isPulling = true;
            magnet.isActive = true;
            pullRoutine = StartCoroutine(PullTimer());
        }
        else
        {
            // Cancel early (toggle off)
            StopCoroutine(pullRoutine);
            EndPull();
        }
    }

    private IEnumerator PullTimer()
    {
        yield return new WaitForSeconds(magnetDuration);
        EndPull();
    }

    private void EndPull()
    {
        isPulling = false;
        magnet.isActive = false;
    }

    // ── Push (toggle + auto-off timer) ───────────────────────────
    //
    //  Same pattern: first press activates for pushDuration,
    //  second press cancels early.

    public void Push()
    {
        if (!inputHander.PushObjPressed()) return;

        if (!isPushing)
        {
            isPushing = true;
            pushPoint.enabled = true;
            pushRoutine = StartCoroutine(PushTimer());
        }
        else
        {
            StopCoroutine(pushRoutine);
            EndPush();
        }
    }

    private IEnumerator PushTimer()
    {
        yield return new WaitForSeconds(pushDuration);
        EndPush();
    }

    private void EndPush()
    {
        isPushing = false;
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
        /*else if(collision.gameObject.layer == DamageLayer)
        {
            Debug.LogError("Player Damaged");
            heartUI.RemoveHeart();
            StartCoroutine(DamageCooldown());
        }*/
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isTriggered = false;
        if (!isTriggered)
        {
            if(collision.gameObject.layer == 10)
            {
            Debug.LogError("Player Damaged");
            Debug.LogError($"Dont repeat damage {dontRepeatDamage} ");
            StartCoroutine(DamageCooldown());
            dontRepeatDamage = true;
            isTriggered = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        isTriggered = false;
    }
}