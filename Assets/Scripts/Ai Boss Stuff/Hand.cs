using System.Collections;
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

    [SerializeField] float Duration1;
    [SerializeField] float Duration2;
    [SerializeField] float Duration3;
    [SerializeField] float HandUpDuration;

    Vector3 initialPosition;

    public BossGeneral bossGeneral;
    public void Attack()
    {
        StartCoroutine(HandAttack(Duration1, Duration2, Duration3, HandUpDuration));
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        HandMoveTo = new Vector3(PlayerTransform.position.x, -1f, 0);
    }

    IEnumerator HandAttack(float duration1, float duration2, float duration3, float handupduration)
    {
        float elapsed1 = 0f;

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
        Debug.Log("Ground Distance: " + GroundDistance);

        Vector3 FloorHit = new Vector3(transform.position.x, -GroundDistance, 0);
        Debug.Log($"FloorHit Vector: {FloorHit}");


        while (elapsed2 < duration2)
        {
            Vector3 targetPosition = Vector3.MoveTowards(transform.position, FloorHit, HandDownForce * Time.fixedDeltaTime);
            rb.MovePosition(targetPosition);
            Debug.Log($"Current Position: {transform.position}, Target Position: {targetPosition}");
            //rb.MovePosition(FloorHit);
            elapsed2 += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        float elapsed3 = 0f;
        while (elapsed3 < duration3)
        {
            Vector3 targetPosition = Vector3.MoveTowards(transform.position, initialPosition, GoBackForce * Time.fixedDeltaTime);

            rb.MovePosition(targetPosition);
            elapsed3 += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Hand Attack Finished");

        bossGeneral.Phase1Attacking = false;
    }
}
