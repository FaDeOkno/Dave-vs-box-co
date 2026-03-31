using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MagnetController : MonoBehaviour
{
    [Header("References")]
    public InputHander inputHander;
    [SerializeField] PointEffector2D pushPoint;
    [SerializeField] PointEffector2D pullPoint;
    [SerializeField] LineRenderer laserRenderer;

    [Header("3 In 1 Magnet")]
    public MagnetType SelectedMagnet = MagnetType.Pull;
    [SerializeField] float MagCooldown;

    [Space]
    public float pullDuration = 3f;   // how long pull stays active
    public float pushDuration = 0.5f; // how long push stays active

    [SerializeField] float laserLength;
    [SerializeField] LayerMask laserCollisionLayer;
    [SerializeField] CannonShoot cannonShoot;

    public MagnetType activeMagnet { get; private set; } = MagnetType.None;

    private Coroutine pullRoutine;
    private Coroutine pushRoutine;
    private bool laserFading = false;
    private float laserStartWidth;
    private float laserEndWidth;

    bool CanUseMag = true;

    private void Start()
    {
        pushPoint.enabled = false;
        pullPoint.enabled = false;

        laserStartWidth = laserRenderer.startWidth;
        laserEndWidth = laserRenderer.endWidth;

        laserRenderer.positionCount = 2;
        laserRenderer.useWorldSpace = true;
        laserRenderer.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });

        laserRenderer.enabled = false;
    }

    private void Update()
    {
        HandleSelectedMagnet();
        HandlePull();
        HandlePush();

        UpdateLaser();
    }

    // This is the one function that houses all magnet abilities
    public void HandleSelectedMagnet()
    {
        if (inputHander == null || !inputHander.MagnetButPressed())
            return;

        if (activeMagnet != MagnetType.None)
        {
            StopMagnets();
            return;
        }

        // Immediately block further uses so holding the button doesn't restart cooldowns
        if (!CanUseMag)
            return;

        CanUseMag = false;
        Debug.Log(SelectedMagnet);

        switch (SelectedMagnet)
        {
            case MagnetType.Pull:
                activeMagnet = MagnetType.Pull;
                pullPoint.enabled = true;
                pullRoutine = StartCoroutine(PullTimer());
                break;

            case MagnetType.Push:
                activeMagnet = MagnetType.Pull;
                pushPoint.enabled = true;
                pushRoutine = StartCoroutine(PushTimer());
                break;

            case MagnetType.Laser:
                //Debug.Log($"Is Facing right : {facingRight}");
                RaycastThing(transform.right * transform.localScale.x);
                break;
        }

        Invoke(nameof(ResetMagnet), MagCooldown);
    }

    public void StopMagnets()
    {
        if (pullRoutine != null)
        {
            StopCoroutine(pullRoutine);
        }
        if (pushRoutine != null)
        {
            StopCoroutine(pushRoutine);
        }

        EndPull();
        EndPush();
    }

    void ResetMagnet()
    {
        CanUseMag = true;
    }

    #region Pull
    public void HandlePull()
    {
        if (!inputHander.PullObjPressed()) return;

        if (activeMagnet != MagnetType.Pull)
        {
            StopMagnets();
            activeMagnet = MagnetType.Pull;
            pullPoint.enabled = true;
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
        yield return new WaitForSeconds(pullDuration);
        EndPull();
    }

    private void EndPull()
    {
        activeMagnet = MagnetType.None;
        pullPoint.enabled = false;
        pullRoutine = null;
    }
    #endregion

    #region Push
    //  Same pattern: first press activates for pushDuration,
    //  second press cancels early.
    public void HandlePush()
    {
        if (!inputHander.PushObjPressed())
            return;

        if (activeMagnet != MagnetType.Push)
        {
            StopMagnets();
            activeMagnet = MagnetType.Push;
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
        activeMagnet = MagnetType.None;
        pushPoint.enabled = false;
        pushRoutine = null;
    }
    #endregion

    #region Laser
    private void UpdateLaser()
    {
        if (!laserFading)
            return;

        if (laserRenderer.startWidth <= 0f && laserRenderer.endWidth <= 0f)
        {
            laserRenderer.startWidth = laserStartWidth;
            laserRenderer.endWidth = laserEndWidth;

            laserRenderer.enabled = false;
            laserFading = false;
        }
        else
        {
            laserRenderer.startWidth = Mathf.Max(laserRenderer.startWidth - Time.deltaTime * 10f, 0f);
            laserRenderer.endWidth = Mathf.Max(laserRenderer.endWidth - Time.deltaTime * 10f, 0f);
        }
    }

    // Not really a magnet but still related to it
    private void RaycastThing(Vector2 dir)
    {
        Vector2 rayOrigin = (Vector2)transform.position + dir * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, laserLength, laserCollisionLayer);

        Vector3 rayEnd = hit.collider != null ? (Vector3)hit.point : (Vector3)(rayOrigin + dir * laserLength);
        Debug.DrawRay(rayOrigin, rayEnd - (Vector3)rayOrigin, Color.white);

        if (laserRenderer != null)
        {
            laserRenderer.SetPosition(0, rayOrigin);
            laserRenderer.SetPosition(1, rayEnd);
            laserRenderer.enabled = true;
            laserFading = true;
        }

        if (hit.collider != null && hit.collider.CompareTag("cannon"))
        {
            if (hit.collider.TryGetComponent<CannonShoot>(out cannonShoot))
            {
                cannonShoot.AddCharge();
            }
        }
    }
    #endregion

    // Called by the shop to grow the magnet and push colliders.
    public void UpgradeMagnet()
    {
        ScaleCollider(pullPoint.GetComponent<Collider2D>(), 1.5f);
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
        laserStartWidth *= 1.5f;
        laserEndWidth *= 1.5f;
        laserLength *= 1.5f;
        if (laserRenderer != null)
        {
            laserRenderer.startWidth = laserStartWidth;
            laserRenderer.endWidth = laserEndWidth;
        }
    }

    [System.Serializable]
    public enum MagnetType
    {
        None,
        Pull,
        Push,
        Laser
    }
}
