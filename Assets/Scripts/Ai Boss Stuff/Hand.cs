using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Hand : MonoBehaviour
{
    [SerializeField] Transform PlayerTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Vector3 HandMoveTo;

    [SerializeField] LayerMask GroundLayer;

    [SerializeField] float HandDownForce;
    [SerializeField] float GoBackForce;

    [Header("Hand Down Durations")]
    [SerializeField] float Duration1;
    [SerializeField] float Duration2;
    [SerializeField] float Duration3;
    [SerializeField] float HandUpDuration;

    [Header("Hand Slider Durations")]
    [SerializeField] float SlideDur1;
    [SerializeField] float SlideDur2;
    [SerializeField] float SlideDur3;

    [SerializeField] float AttackDelay;

    [Header("Hand Slider GoToPos")]
    [SerializeField] Transform StartHandLGo;
    [SerializeField] Transform StartHandRGo;
    [Space]
    [SerializeField] Transform HandLGoTo;
    [SerializeField] Transform HandRGoTo;
    

    //Make sure both hands are dead
    [Header("Hand Death")]
    public float HandHealth;
    [SerializeField] bool HandIsDead;
    bool hasStartedDeath;

    //List Of all Hand Colliders
    //MAKE SURE THIS ONLY CONTAINS DAMAGING COLLIDERS!! NOT THE MAIN COLLIDER OR ELSE HAND WILL FALL THROUGH FLOOR
    public List<Collider2D> Colliders;

    Vector3 initialPosition;

    public BossGeneral bossGeneral;

    public bool IsDead => HandIsDead;

    public void AttackPhase1()
    {
        if (HandIsDead || bossGeneral == null || bossGeneral.Phase1Finished) return;
        //StartCoroutine(HandAttack(Duration1, Duration2, Duration3, HandUpDuration));
        StartCoroutine(SlideAttack(SlideDur1, SlideDur2, SlideDur3));
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (PlayerTransform == null) return;
        HandMoveTo = new Vector3(PlayerTransform.position.x, -1f, 0);

        if (HandHealth <= 0f) HandIsDead = true;

        if (HandIsDead)
        {
            if (!hasStartedDeath)
            {
                StartHandDeath();
                hasStartedDeath = true;
                if (gameObject.CompareTag("LHand"))
                {
                    Debug.Log("Left Hand Died");
                }
                else if (gameObject.CompareTag("RHand"))
                {
                    Debug.Log("Right Hand Died");
                }
            }
            return;
        }

    }

    IEnumerator HandAttack(float duration1, float duration2, float duration3, float handupduration)
    {
        while (!HandIsDead)
        {
            float elapsed1 = 0f;

            //Follows Player
            while (elapsed1 < duration1)
            {

                rb.MovePosition(HandMoveTo);

                elapsed1 += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            //Moves hand up a lil to indicate its attack.

            float elapsed4 = 0f;
            while (elapsed4 < handupduration)
            {
                Vector3 targetPosition = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y + 0.5f, 0), HandDownForce * Time.fixedDeltaTime);
                rb.MovePosition(targetPosition);
                elapsed4 += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            float elapsed2 = 0f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, GroundLayer);
            float GroundDistance = hit.distance;

            Vector3 FloorHit = new Vector3(transform.position.x, -GroundDistance + transform.position.y + 1f, 0);

            //Strikes down
            while (elapsed2 < duration2)
            {
                Vector3 targetPosition = Vector3.MoveTowards(transform.position, FloorHit, HandDownForce * Time.fixedDeltaTime);
                rb.MovePosition(targetPosition);
                //rb.MovePosition(FloorHit);
                elapsed2 += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            float elapsed3 = 0f;
            //Goes back to OG position
            while (elapsed3 < duration3)
            {
                Vector3 targetPosition = Vector3.MoveTowards(transform.position, initialPosition, GoBackForce * Time.fixedDeltaTime);

                rb.MovePosition(targetPosition);
                elapsed3 += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

        }
        bossGeneral.Phase1Attacking = false;
        yield break;
    }
    IEnumerator SlideAttack(float dur1, float dur2, float dur3)
    {
        float elapsed1 = 0f;
        Vector2 OgPos = transform.position;
        var startPos1 = transform.position;

        bool attackCooldown = false;
        while (elapsed1 < dur1)
        {
            
            //Brings Hands to where to start off screen
            if (gameObject.CompareTag("LHand"))
            {
                elapsed1 += Time.deltaTime;
                Vector2 MoveTo = new Vector2(StartHandLGo.position.x, StartHandLGo.position.y);
                transform.position = Vector2.Lerp(startPos1, MoveTo, elapsed1 / dur1);
                yield return new WaitForFixedUpdate();
            }
            else if (gameObject.CompareTag("RHand"))
            {
                elapsed1 += Time.deltaTime;
                Vector2 MoveTo = new Vector2(StartHandRGo.position.x, StartHandRGo.position.y);
                transform.position = Vector2.Lerp(startPos1, MoveTo, elapsed1 / dur1);
                yield return new WaitForFixedUpdate();
            }
        }

        if (!attackCooldown)
        {
            yield return new WaitForSeconds(AttackDelay);
            attackCooldown = true;
        }

        float elapsed2 = 0f;
        var startPos2 = transform.position;
        while (elapsed2 < dur2)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0,0, 90));
            
            if (gameObject.CompareTag("LHand"))
            {
                elapsed2 += Time.deltaTime;
                Vector2 MoveTo = new Vector2(HandLGoTo.position.x, HandLGoTo.position.y);
                transform.position = Vector2.Lerp(startPos2, MoveTo, elapsed2 / dur2);
                yield return new WaitForFixedUpdate();
            }
            else if (gameObject.CompareTag("RHand"))
            {
                elapsed2 += Time.deltaTime;
                Vector2 MoveTo = new Vector2(HandRGoTo.position.x, HandRGoTo.position.y);
                transform.position = Vector2.Lerp(startPos2, MoveTo, elapsed2 / dur2);
                yield return new WaitForFixedUpdate();
            }
        }
        attackCooldown = false;

        float elapsed3 = 0f;
        var startPos3 = transform.position;
        while (elapsed3 < dur3)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            elapsed3 += Time.deltaTime;
            Vector2 MoveTo = new Vector2(HandRGoTo.position.x, HandRGoTo.position.y);
            transform.position = Vector2.Lerp(startPos3, OgPos, elapsed3 / dur3);
            yield return new WaitForFixedUpdate();
        }
    }
    void StartHandDeath()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        foreach (var collision in Colliders)
        {
            collision.enabled = false;
        }
    }
}
