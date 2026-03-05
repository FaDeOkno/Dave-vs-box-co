using UnityEngine;

public class Magnet : MonoBehaviour
{
    [SerializeField] public float pullForce = 10f;
    public bool isActive = false;
    
    void OnTriggerStay2D(Collider2D collision)
    {
        if (isActive)
        {
             if (collision.gameObject.TryGetComponent<PullObject>(out PullObject pullObject))
                {
                 pullObject.SetTarget(transform.parent.position);
                }
        }
    }
}
