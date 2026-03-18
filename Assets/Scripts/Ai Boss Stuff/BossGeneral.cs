using System.Collections;
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

        bool leftDead = leftHand.IsDead;
        bool rightDead = rightHand.IsDead;

        if (leftDead && rightDead)
        {
            Phase1Finished = true;
            Phase1Attacking = false;
            Phase2Attacking = true;
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

            bool leftDead = leftHand.IsDead;
            bool rightDead = rightHand.IsDead;
            int HandUsed = -1;

            if (!leftDead && !rightDead)
            {
                HandUsed = Random.Range(0, 2);
            }
            else if (leftDead && !rightDead)
            {
                HandUsed = 1;
            }
            else if (!leftDead && rightDead)
            {
                HandUsed = 0;
            }
            else
            {
                Phase1Finished = true;
                break;
            }

            Hand selectedHand = (HandUsed == 0) ? leftHand : rightHand;
            if (selectedHand != null && !selectedHand.IsDead)
            {
                Debug.Log(HandUsed);
                selectedHand.AttackPhase1();
                Phase1Attacking = true;
            }

            break;
        }

        phase1CoroutineRunning = false;
    }
}
