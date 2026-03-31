using System.Collections;
using UnityEngine;

public partial class Movement
{
    // ── Dash ──────────────────────────────────────────────────────
    [Header("Dash", order = 4)]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashTime = 0.2f;

    // ── Quick Dash ──────────────────────────────────────────────────────
    [Header("Quick Dash", order = 3)]
    [SerializeField] private float quickDashForce = 15f;
    [SerializeField] private float quickDashDuration = 0.12f;
    [SerializeField] float DashCooldownNum;
    private bool isQuickDashing = false;
    bool CanDash = true;

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
}
