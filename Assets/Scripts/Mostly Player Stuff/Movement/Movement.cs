using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Movement : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────
    [Header("References", order = 0)]
    public HeartUI heartUI;
    public Rigidbody2D rb;
    [SerializeField] private BoxCollider2D Collider;

    // ── Animation / Facing ────────────────────────────────────────
    private Animator anim;
    public bool facingRight { get; private set; } = true;

    // ── Shop Upgrades ───────────────────────────────────────────────────
    [Header("Shop Upgrades")]
    public bool hasDoubleJump = false;
    public bool hasDash = false;

    // ── Die and Damage Code ─────────────────────────────────────────────────
    [Header("Damage and Death", order = 6)]
    [SerializeField] Collider2D handLCollider;
    [SerializeField] Collider2D handRCollider;
    [SerializeField] float damageCooldown = .25f;


    bool dontRepeatDamage = false;
    bool isTriggered;
    bool useInputs = true;

    public void Die()
    {

    }

    public IEnumerator DamageCooldown()
    {
        if (dontRepeatDamage)
            yield break;

        dontRepeatDamage = true;
        heartUI.RemoveHeart();
        yield return new WaitForSeconds(damageCooldown);

        Debug.Log("Damage cooldown finished");
        dontRepeatDamage = false;
        Debug.Log("The damage repeat bool should be false");
        yield break;

    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateGrounded();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        UpdateWallHold();
        HandleGravity();

        UpdateMovement();
    }

    // ── Collision ─────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        isTriggered = false;
        Debug.Log($"isTriggered = {isTriggered}");
        if (!isTriggered)
        {
            if (collision.CompareTag("LHand") || collision.CompareTag("RHand"))
            {
                Debug.LogError("Player Damaged");
                Debug.LogError($"Dont repeat damage {dontRepeatDamage} ");
                StartCoroutine(DamageCooldown());
                dontRepeatDamage = true;
                isTriggered = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        isTriggered = false;
    }

    [Serializable]
    private enum WallSide
    {
        None,
        Left,
        Right
    }
}
