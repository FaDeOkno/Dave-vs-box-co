using UnityEngine;
using UnityEngine.XR;

public class SwitchGravity : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    public bool isButPressed = false;

    public bool OnCeiling = false;
    public bool OnLeft = false;
    public bool OnRight = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isButPressed)
        {
            if (collision.gameObject.CompareTag("CeilingTrig"))
            {
                rb.gravityScale = -1;
                OnCeiling = true;
            }
            if (collision.gameObject.CompareTag("LTrig"))
            {
                Physics2D.gravity = new Vector2(-9.81f, 0);
                OnLeft = true;
            }
            if (collision.gameObject.CompareTag("RTrig"))
            {
                Physics2D.gravity = new Vector2(9.81f, 0);
                OnRight = true;
            }
        }
        else { isButPressed = false; }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        rb.gravityScale = 1f;
        OnCeiling = false;
        isButPressed = false;
        OnLeft = false;
        OnRight = false;
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}