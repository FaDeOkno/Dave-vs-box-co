using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 targetPoint = Vector3.zero;

    public Movement player;

    public float moveSpeed = 5f;

    public float lookAheadDistance = 5f, lookAheadSpeed = 3f;

    private float lookOffset;

    private void Start()
    {
        targetPoint = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }

    private void LateUpdate()
    {
        //targetPoint.x = player.transform.position.x;
        //targetPoint.y = player.transform.position.y;

        if (player.isGrounded)
        {
            targetPoint.y = player.transform.position.y;
        }
        
        if(targetPoint.y < 0)
        {
            targetPoint.y = 0;
        }

        if(player.moveInput.x > 0f)
        {
            lookOffset = Mathf.Lerp(lookOffset, lookAheadDistance, lookAheadSpeed * Time.deltaTime);
        }
        else if(player.moveInput.x < 0f)
        {
            lookOffset = Mathf.Lerp(lookOffset, -lookAheadDistance, lookAheadSpeed * Time.deltaTime);
        }

        targetPoint.x = player.transform.position.x + lookOffset;

        transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
    }
}