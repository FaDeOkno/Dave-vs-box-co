using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] Transform PlayerTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Vector2 HandMoveTo;
    public void Attack()
    {
        StartCoroutine(HandAttack());
    }

    private void Update()
    {
        HandMoveTo = new Vector2(PlayerTransform.position.x, -1f);
    }

    IEnumerator HandAttack()
    {
        rb.MovePosition(HandMoveTo);
        yield return new WaitForSeconds(5f);
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down);
        float distanceToGround = groundHit.distance;
        Vector2 GoToFloor = new Vector2(transform.position.x, distanceToGround);
        rb.MovePosition(GoToFloor);
    }
}
