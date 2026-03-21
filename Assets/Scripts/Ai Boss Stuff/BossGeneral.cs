using System.Collections;
using System.Security;
using UnityEditor.EditorTools;
using UnityEngine;

public class BossGeneral : MonoBehaviour
{
    public int Phase = 1;

    public GameObject LHand;
    public GameObject RHand;

    [SerializeField] Hand leftHand;
    [SerializeField] Hand rightHand;

    public bool startBoss;

    public bool Phase1Attacking;
    public bool Phase1Finished;


    public bool Phase2Attacking;

    public bool P1CouStoped = false;
    bool phase1CoroutineRunning = false;

    [Header("Trying to do lasers")]
    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.1f;
    public float laserMaxLength = 5f;
    [Space]
    public bool AiLaserAttack;
    public bool LaserStopFollowing;
    // follow the player
    [Tooltip("Optional - if empty the boss will try to find the object tagged 'Player' on Start")]
    [SerializeField] private Transform player;

    public string whatAttackuse;

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

        // try to find player if not assigned
        if (player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
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

        if (AiLaserAttack && laserLineRenderer != null && player != null)
        {
            // Aim the laser at the player's current position so it follows the player.
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // If LaserStopFollowing is false, update laser positions based on a raycast toward the player.
            if (!LaserStopFollowing)
            {
                ShootLaserFromTargetPosition(transform.position, directionToPlayer, distanceToPlayer);
            }

            laserLineRenderer.enabled = true;
        }
        else if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }

        bool leftDead = leftHand != null && leftHand.IsDead;
        bool rightDead = rightHand != null && rightHand.IsDead;

        if (leftDead && rightDead)
        {
            Phase1Finished = true;
            Phase1Attacking = false;
            Phase2Attacking = true;

            Phase = 2;
        }
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
    IEnumerator Phase2()
    {
        // Trying to cool animation where its locks on to you



        yield return null;
    }

    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        // Cast toward the given direction and set the line renderer to end at the hit point or max length.
             if (laserLineRenderer == null) return;

        Vector3 endPosition = targetPosition + (length * direction);

        RaycastHit hit;
        // Raycast up to 'length' so we detect obstacles or the player in between.
        if (Physics.Raycast(targetPosition, direction, out hit, length))
        {
            endPosition = hit.point;

            if (hit.collider != null)
            {
                Debug.Log("Player Hit Laser");
                // TODO: apply damage or trigger player-hit behavior here
            }
            else if(hit.collider == null)
            {
                Debug.Log("Nothing got hit");
            }
            else
            {
                Debug.Log("ATP idk man...");
            }
        }
        else
        {
            Debug.Log("Damm thing wont work");
        }
            Debug.DrawLine(targetPosition, endPosition, color: Color.green);

        laserLineRenderer.SetPosition(0, targetPosition);
        laserLineRenderer.SetPosition(1, endPosition);
    }
}


