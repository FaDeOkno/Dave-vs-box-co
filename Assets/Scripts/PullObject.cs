using UnityEngine;

public class PullObject : MonoBehaviour
{
    public Rigidbody2D rb;

    [SerializeField] private Magnet magnet;
    bool hasTarget;
    Vector3 targetPosition;

    void FixedUpdate()
    {
        if(hasTarget)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x, direction.y) * magnet.pullForce; // Adjust the speed as needed
            hasTarget = false; // Reset target after applying force
        }
    }

    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
    }
}
