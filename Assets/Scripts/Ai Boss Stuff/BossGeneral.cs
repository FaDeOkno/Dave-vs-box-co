using System.Collections;
using System.Security;
using System.Timers;
using UnityEditor.EditorTools;
using UnityEngine;

public class BossGeneral : MonoBehaviour
{
    public int Phase = 1;

    public GameObject LHand;
    public GameObject RHand;
    public Movement movement;

    [SerializeField] Hand leftHand;
    [SerializeField] Hand rightHand;

    public bool startBoss;

    public bool Phase1Attacking;
    public bool Phase1Finished;


    public bool Phase2Attacking;

    public bool P1CouStoped = false;
    bool phase1CoroutineRunning = false;

    public HeartUI heartUI;
    bool InvokeMethOnce = false;

    [Header("Boss Health")]
    public float health;

    [Header("Trying to do lasers")]
    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.1f;
    public float laserMaxLength = 5f;
    [Space]
    public bool AiLaserAttack;
    public bool LaserStopFollowing;

    public bool StrikePlayer;
    // follow the player
    [Tooltip("Optional - if empty the boss will try to find the object tagged 'Player' on Start")]
    [SerializeField] private Transform player;

    public string whatAttackuse;

    RaycastHit2D hit;

    private void Start()
    {
        if (laserLineRenderer != null)
        {
            laserLineRenderer.positionCount = 2;
            laserLineRenderer.useWorldSpace = true;
            Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
            laserLineRenderer.SetPositions(initLaserPositions);
            laserLineRenderer.startWidth = laserWidth;
            laserLineRenderer.endWidth = laserWidth;
            laserLineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (startBoss && !phase1CoroutineRunning && !Phase1Finished)
        {
            StartCoroutine(Phase1Count());
        }

        if (P1CouStoped && !phase1CoroutineRunning && !Phase1Finished)
        {
            P1CouStoped = false;
            StartCoroutine(Phase1Count());
        }

        if(AiLaserAttack && !Phase2Attacking){
            StartCoroutine(Phase2(3, .3f, .1f, 2));
        }
        /*else if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }*/

        bool leftDead = leftHand != null && leftHand.IsDead;
        bool rightDead = rightHand != null && rightHand.IsDead;

        if (leftDead && rightDead)
        {
            Phase1Finished = true;
            Phase1Attacking = false;
            Phase2Attacking = true;
            AiLaserAttack = true;



            if(!InvokeMethOnce) {
                StartCoroutine(Phase2(3, .3f, .1f, 2)); 
                InvokeMethOnce = true;
            }

            Phase = 2;
        }

        if (health <= 0) {AiDeath();}
    }

    IEnumerator Phase1Count()
    {
        phase1CoroutineRunning = true;

        if (Phase1Finished)
        {
            phase1CoroutineRunning = false;
            yield break;
        }

        while (!Phase1Attacking && !Phase1Finished)
        {
            int AttackChance = Random.Range(-1, 2);
            if (AttackChance == 0)
            {
                yield return new WaitForSeconds(1f);
                P1CouStoped = true;
                break;
            }

            bool leftDead = leftHand != null && leftHand.IsDead;
            bool rightDead = rightHand != null && rightHand.IsDead;
            int HandUsed = -1;

            //Attempting to randomly generate a phase to use
            int WhatAttackUseInt = Random.Range(0, 5);
            Debug.Log($"Attack Int: {WhatAttackUseInt}");

            if (WhatAttackUseInt == 0 || WhatAttackUseInt == 1)
            {
                if (!leftDead && !rightDead)
                {
                    HandUsed = Random.Range(0, 2);
                    whatAttackuse = "slam";
                }
                else if (leftDead && !rightDead)
                {
                    HandUsed = 1;
                    whatAttackuse = "slam";
                }
                else if (!leftDead && rightDead)
                {
                    HandUsed = 0;
                    whatAttackuse = "slam";
                }
                else
                {
                    Phase1Finished = true;
                    break;
                }
            }
            else if (WhatAttackUseInt == 2 || WhatAttackUseInt == 3 || WhatAttackUseInt == 4)
            {
                if (!leftDead && !rightDead)
                {
                    HandUsed = 3;
                    whatAttackuse = "slide";
                }
                else if (leftDead && !rightDead)
                {
                    HandUsed = 1;
                    whatAttackuse = "slide";
                }
                else if (!leftDead && rightDead)
                {
                    HandUsed = 0;
                    whatAttackuse = "slide";
                }
                else
                {
                    Phase1Finished = true;
                    break;
                }
            }

            Hand selectedHand = null;
            Hand additionalHand = null;
            if (HandUsed == 0) { selectedHand = leftHand; }
            else if (HandUsed == 1) { selectedHand = rightHand; }
            else if (HandUsed == 3) { selectedHand = leftHand; additionalHand = rightHand; }
            if (selectedHand != null && !selectedHand.IsDead)
            {
                Debug.Log(HandUsed);
                if (whatAttackuse == "slam")
                {
                    selectedHand.AttackPhase1();
                    if (additionalHand != null) { additionalHand.AttackPhase1(); }
                }
                else if (whatAttackuse == "slide")
                {
                    selectedHand.SlideAttack();
                    if (additionalHand != null) { additionalHand.SlideAttack(); }
                }
                Phase1Attacking = true;
            }

            break;
        }

        phase1CoroutineRunning = false;
    }
    IEnumerator Phase2(float dur1, float dur2, float dur3, float StartDelay = 0f)
    {
        yield return new WaitForSeconds (.5f);

        if(StartDelay != 0){yield return new WaitForSeconds(StartDelay); }

        laserLineRenderer.enabled = true;
        LaserStopFollowing = false;
        
        float elapsed = 0f;
        Phase2Attacking = true;
        while(!LaserStopFollowing && elapsed < dur1){    
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            ShootLaserFromTargetPosition(transform.position, directionToPlayer, distanceToPlayer);

            laserLineRenderer.enabled = true;

            elapsed += Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }

        float elapsed2 = 0f;
        while(elapsed2 < dur2){
            LaserStopFollowing = true;
            elapsed2 += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        float elapsed3 = 0f;
        while(elapsed3 < dur3){
            if (hit.collider.gameObject.CompareTag("Player")){
                Debug.LogError("Hit Player");
                StartCoroutine(movement.DamageCooldown()); 
            }
            elapsed3 += Time.deltaTime;
            laserLineRenderer.enabled = false;
            yield return new WaitForFixedUpdate();
        }
        LaserStopFollowing = false;

        Phase2Attacking = false;
        yield break;
    }

    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        Vector2 endPosition = targetPosition + direction * length;

        Vector2 origin2D = targetPosition;
        Vector2 dir2D = direction.normalized;

        hit = Physics2D.Raycast(origin2D, dir2D, length);
        Debug.LogWarning($"RayCast Hit Pos: {hit.point}");
        Debug.LogWarning($"Player Pos > {player.position}");
        Debug.DrawRay(targetPosition, dir2D * length, Color.white);

        if (hit.collider != null)
        {
            // preserve the ON-SCREEN z-plane of the laser start point
            var hitPoint3D = new Vector3(hit.point.x, hit.point.y, 0);
            //endPosition = hitPoint3D;
        }
        /*if (hit.collider.gameObject.CompareTag("Player")&& StrikePlayer == true){
            Debug.LogError("Hit Player");
            StartCoroutine(movement.DamageCooldown());
            
        }*/
        laserLineRenderer.enabled = true;
        laserLineRenderer.SetPosition(0, targetPosition);
        laserLineRenderer.SetPosition(1, endPosition);
    }

    public void AiDeath()
    {
        Debug.Log("AI BOSS HAS DIED");
        startBoss = false;
        StopAllCoroutines();

        //Start Ending sequence
    }
}