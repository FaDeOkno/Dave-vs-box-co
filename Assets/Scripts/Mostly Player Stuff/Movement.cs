using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public Rigidbody2D rb;
    public Vector2 moveInput;
    public bool CanMove = true;

    // ── Jump stuff ───────────────────────────────────────────────
    [Header("Jump Stuff")]
    [SerializeField] float jumpduration;
    [SerializeField] float maxjumpduration;
    [SerializeField] Transform groundCheck;

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
    [SerializeField] Transform wallCheck;

    private WallSide whichWallWasTouched = WallSide.None;
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

    // ── Shop Upgrades ───────────────────────────────────────────────────
    [Header("Shop Upgrades")]
    public bool hasDoubleJump = false;
    public bool hasDash = false;
    private bool usedDoubleJump = false;

    // List Of Player collision because it has multiple
    public List<Collider2D> playerColliders;

    // ── Die and Damage Code ─────────────────────────────────────────────────
    [Header("Damage and Death")]
    [SerializeField] Collider2D handLCollider;
    [SerializeField] Collider2D handRCollider;
    [SerializeField] Collider2D PlayerCollider;
    [SerializeField] float damageCooldown = .25f;

    // ── 3 in 1 magnet stuff
    [Header("3 In 1 Magnet")]
    public MagnetType WhatMagnetToUse = MagnetType.Pull;

    [Space]
    [SerializeField] float PlayerLazerWidth;
    [SerializeField] LineRenderer LazerPointiere;
    Vector3 PlayerPos;
    [SerializeField] CannonShoot cannonShoot;
    [SerializeField] GameObject BossAI;
    [SerializeField] float LaserMaxDist;
    [SerializeField] LayerMask IgnorePlayerLayer;

    bool dontRepeatDamage = false;
    bool isTriggered;
    bool CanUseMag = true;
    bool useInputs = true;

    [Space]
    [SerializeField] float MagCooldown;
    public void Die()
    {

    }

    // Called by the shop to grow the magnet and push colliders.
    public void UpgradeMagnet()
    {
        ScaleCollider(magnet.GetComponent<Collider2D>(), 1.5f);
        ScaleCollider(pushPoint.GetComponent<Collider2D>(), 1.5f);
    }

    void ScaleCollider(Collider2D col, float factor)
    {
        if (col == null) return;
        if (col is CircleCollider2D circle)
            circle.radius *= factor;
        else if (col is BoxCollider2D box)
            box.size *= factor;
    }

    // Called by the shop to widen + lengthen the laser.
    public void UpgradeLaser()
    {
        PlayerLazerWidth *= 1.5f;
        LaserMaxDist *= 1.5f;
        if (LazerPointiere != null)
        {
            LazerPointiere.startWidth = PlayerLazerWidth;
            LazerPointiere.endWidth = PlayerLazerWidth;
        }
    }

    public IEnumerator DamageCooldown()
    {
        if (!dontRepeatDamage)
        {
            dontRepeatDamage = true;
            heartUI.RemoveHeart();
            yield return new WaitForSeconds(damageCooldown);
            Debug.LogError("Damage cooldown finished");
            dontRepeatDamage = false;
            Debug.LogError("The damage repeat bool should be false");
            yield break;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        PlayerCollider = GetComponent<Collider2D>();

        //Physics2D.IgnoreCollision(PlayerCollider, handLCollider, true);
        //Physics2D.IgnoreCollision(PlayerCollider, handRCollider, true);

        // foreach (Collider2D col in playerColliders)
        // {
        //     Physics2D.IgnoreCollision(col, handLCollider, true);
        //     Physics2D.IgnoreCollision(col, handRCollider, true);
        // }

        WhatMagnetToUse = MagnetType.Pull;

        if (LazerPointiere != null)
        {
            LazerPointiere.positionCount = 2;
            LazerPointiere.useWorldSpace = true;
            Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
            LazerPointiere.SetPositions(initLaserPositions);
            LazerPointiere.startWidth = PlayerLazerWidth;
            LazerPointiere.endWidth = PlayerLazerWidth;
            LazerPointiere.enabled = false;
        }
    }

    private void Update()
    {
        UpdateGrounded();

        GetInputs();

        UpdateWallHold();

        PlayerPos = transform.position;
    }

    private void GetInputs()
    {
        GatherMoveInput();
        HandleQuickDash();
        HandleFlip();

        UpdateWallHold();
        HandleJump();

        Magnet();
        Pull();
        Push();
    }

    // ── Input Callbacks ──────────────────────────────────────────

    private void GatherMoveInput()
    {
        if (CanMove && !isOnWall)
            moveInput = inputHander.GetHorizontalInput();
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        HandleGravity();


        UpdateMovement();
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

        else if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }

        else
        {
            rb.AddForceX(moveInput.x * moveSpeed * 5, ForceMode2D.Force);
            rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed), rb.linearVelocity.y);
        }
    }

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

    #region Quck Dash
    public void HandleQuickDash()
    {
        if (!inputHander.QuickDashPressed())
            return;

        if (!hasDash)
            return;

        if (isQuickDashing)
            return;

        if (CanDash)
            StartCoroutine(QuickDashRoutine());
    }

    private void UpdateQuickDash()
    {
        if (!isQuickDashing)
            return;

        // Maintain dash velocity during quick dash so FixedUpdate doesn't override it.
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * quickDashForce, 0f);
        return;
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

    // Some obsolete code?
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
    #endregion

    #region Magnet
    // This is the one function that houses all magnet abilities
    public void Magnet()
    {
        if (inputHander == null || !inputHander.MagnetButPressed())
            return;

        // Immediately block further uses so holding the button doesn't restart cooldowns
        if (!CanUseMag)
            return;

        CanUseMag = false;
        Debug.Log(WhatMagnetToUse);

        switch (WhatMagnetToUse)
        {
            case MagnetType.Pull:
                if (!isPulling)
                {
                    isPulling = true;
                    magnet.isActive = true;
                    pullRoutine = StartCoroutine(PullTimer());
                }
                else if (pullRoutine != null)
                {
                    StopCoroutine(pullRoutine);
                    EndPull();
                }
                break;

            case MagnetType.Push:
                if (!isPushing)
                {
                    isPushing = true;
                    pushPoint.enabled = true;
                    pushRoutine = StartCoroutine(PushTimer());
                }
                else if (pushRoutine != null)
                {
                    StopCoroutine(pushRoutine);
                    EndPush();
                }
                break;

            case MagnetType.Lazer:
                Debug.Log($"Is Facing right : {facingRight}");
                RaycastThing(facingRight ? Vector2.right : Vector2.left);
                break;
        }

        Invoke(nameof(ResetMagnet), MagCooldown);
    }

    // ── Pull ─────────────────────────────────────────────────────
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

    // ── Push (toggle + auto-off timer) ───────────────────────────
    //
    //  Same pattern: first press activates for pushDuration,
    //  second press cancels early.

    public void Push()
    {
        if (!inputHander.PushObjPressed())
            return;

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

    // Not really a magnet but still related to it
    private void RaycastThing(Vector2 dir)
    {
        Vector2 rayOrigin = (Vector2)transform.position + dir * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, LaserMaxDist, ~IgnorePlayerLayer);

        Vector3 rayEnd = hit.collider != null ? (Vector3)hit.point : (Vector3)(rayOrigin + dir * LaserMaxDist);
        Debug.DrawRay(rayOrigin, rayEnd - (Vector3)rayOrigin, Color.white);

        if (LazerPointiere != null)
        {
            LazerPointiere.SetPosition(0, rayOrigin);
            LazerPointiere.SetPosition(1, rayEnd);
            Invoke(nameof(DisableLazerPointer), .2f);
        }

        if (hit.collider != null && hit.collider.CompareTag("cannon"))
        {
            if (hit.collider.TryGetComponent<CannonShoot>(out cannonShoot))
            {
                cannonShoot.AddCharge();
            }
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
        pullRoutine = null;
        magnet.isActive = false;
    }

    private IEnumerator PushTimer()
    {
        yield return new WaitForSeconds(pushDuration);
        EndPush();
    }

    private void EndPush()
    {
        isPushing = false;
        pushRoutine = null;
        pushPoint.enabled = false;
    }

    void ResetMagnet()
    {
        CanUseMag = true;
    }

    void DisableLazerPointer()
    {
        LazerPointiere.enabled = false;
    }
    #endregion

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

    // ── Collision ─────────────────────────────────────────────────

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        isOnWall = false;
        whichWallWasTouched = WallSide.None;
        canJumpOffWall = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isTriggered = false;
        Debug.Log($"isTriggered = {isTriggered}");
        if (!isTriggered)
        {
            if (collision.CompareTag("LHand") || collision.CompareTag("RHand"))
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

    [Serializable]
    public enum MagnetType
    {
        Pull,
        Push,
        Lazer
    }

    [Serializable]
    private enum WallSide
    {
        None,
        Left,
        Right
    }
}
