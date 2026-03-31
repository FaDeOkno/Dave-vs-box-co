using System.Collections;
using UnityEngine;

public partial class Movement
{
    // ── 3 in 1 magnet stuff
    [Header("3 In 1 Magnet", order = 7)]
    public MagnetType WhatMagnetToUse = MagnetType.Pull;

    [Space]
    public float magnetDuration = 3f;   // how long pull stays active
    public float pushDuration = 0.5f; // how long push stays active

    [SerializeField] float PlayerLazerWidth;
    [SerializeField] LineRenderer LazerPointiere;
    [SerializeField] CannonShoot cannonShoot;
    [SerializeField] GameObject BossAI;
    [SerializeField] float LaserMaxDist;
    [SerializeField] LayerMask IgnorePlayerLayer;

    private bool isPulling = false;
    private bool isPushing = false;
    private Coroutine pullRoutine;
    private Coroutine pushRoutine;

    Vector3 PlayerPos;

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

    void ResetMagnet()
    {
        CanUseMag = true;
    }

    #region Pull
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
        pullRoutine = null;
        magnet.isActive = false;
    }
    #endregion

    #region Push
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
    #endregion

    #region Laser
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
            LazerPointiere.enabled = true;
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

    void DisableLazerPointer()
    {
        LazerPointiere.enabled = false;
    }
    #endregion

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
}
