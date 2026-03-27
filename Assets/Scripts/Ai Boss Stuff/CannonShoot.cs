using System.Collections;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CannonShoot : MonoBehaviour
{
    
    [Header("Stuff")]
    public float ChargeMeter;
    public float MeterSubBy;
    public float MeterSubCooldown;
    [Space]
    public LineRenderer CannonLine;
    public float LineWidth;

    [Header("Transfrom crap")]
    public Transform StartLine;
    public Transform AITransform;
    [Header("Useful Stuff")]
    public BossGeneral bossGeneral;
    private bool isCouStarted = false;

    void Start()
    {
        CannonLine.positionCount = 2;
        CannonLine.useWorldSpace = true;
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        CannonLine.SetPositions(initLaserPositions);
        CannonLine.startWidth = LineWidth;
        CannonLine.endWidth = LineWidth;
        CannonLine.enabled = false;
    }

    IEnumerator ResetMeter()
    {
        if(ChargeMeter <= 0) {ChargeMeter = 0; isCouStarted = false; yield break;}
        ChargeMeter -= MeterSubBy;
        yield return new WaitForSeconds(MeterSubCooldown);
    }

    public void AddCharge()
    {
        ChargeMeter++;
        if (!isCouStarted)
        {
            StartCoroutine(ResetMeter());
            isCouStarted = true;
        }
    }
    IEnumerator ShootCannon()
    {
        CannonLine.enabled = true;
        CannonLine.SetPosition(0, StartLine.position);
        CannonLine.SetPosition(1, AITransform.position);

        yield return new WaitForSeconds(1f);
        CannonLine.enabled = false;
        bossGeneral.health--;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && ChargeMeter >= 3)
        {
            if (bossGeneral.Phase2Attacking)
            {
                StartCoroutine(ShootCannon());
                ChargeMeter = 0;
            }
        }
    }
}
