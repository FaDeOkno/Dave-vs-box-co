using System.Security;
using UnityEngine;

public class BasicNPC : MonoBehaviour
{
    [Header("Ground Checks")]
    [SerializeField] GameObject Check1;
    [SerializeField] GameObject Check2;

    [Header("Colliders")]
    [SerializeField] Collider2D damagePlayer;
    [SerializeField] Collider2D damageNPC;

    [Header("Player stuff")]
    [SerializeField] Movement movement;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //damages player
            StartCoroutine(movement.DamageCooldown());
        }
    }
}
