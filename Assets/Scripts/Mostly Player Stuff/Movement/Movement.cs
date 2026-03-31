using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Movement : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────
    [Header("References", order = 0)]
    public InputHander inputHander;
    public Magnet magnet;
    public SpriteRenderer spriteRenderer;
    public PointEffector2D pushPoint;
    public GameObject player;
    public HeartUI heartUI;
    public LayerMask DamageLayer;
    public Rigidbody2D rb;

    // ── Animation / Facing ────────────────────────────────────────
    private Animator anim;
    private bool facingRight = true;

    // ── Shop Upgrades ───────────────────────────────────────────────────
    [Header("Shop Upgrades")]
    public bool hasDoubleJump = false;
    public bool hasDash = false;

    // List Of Player collision because it has multiple
    public List<Collider2D> playerColliders;

    // ── Die and Damage Code ─────────────────────────────────────────────────
    [Header("Damage and Death", order = 6)]
    [SerializeField] Collider2D handLCollider;
    [SerializeField] Collider2D handRCollider;
    [SerializeField] Collider2D PlayerCollider;
    [SerializeField] float damageCooldown = .25f;


    bool dontRepeatDamage = false;
    bool isTriggered;
    bool CanUseMag = true;
    bool useInputs = true;

    [Space]
    [SerializeField] float MagCooldown;
    public void Die()
    {

    }

    // Called by the shop to grow the magnet and push colliders.
    public void UpgradeMagnet()
    {
        ScaleCollider(magnet.GetComponent<Collider2D>(), 1.5f);
        ScaleCollider(pushPoint.GetComponent<Collider2D>(), 1.5f);
    }

    void ScaleCollider(Collider2D col, float factor)
    {
        if (col == null) return;
        if (col is CircleCollider2D circle)
            circle.radius *= factor;
        else if (col is BoxCollider2D box)
            box.size *= factor;
    }

    // Called by the shop to widen + lengthen the laser.
    public void UpgradeLaser()
    {
        PlayerLazerWidth *= 1.5f;
        LaserMaxDist *= 1.5f;
        if (LazerPointiere != null)
        {
            LazerPointiere.startWidth = PlayerLazerWidth;
            LazerPointiere.endWidth = PlayerLazerWidth;
        }
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
        PlayerCollider = GetComponent<Collider2D>();

        //Physics2D.IgnoreCollision(PlayerCollider, handLCollider, true);
        //Physics2D.IgnoreCollision(PlayerCollider, handRCollider, true);

        // foreach (Collider2D col in playerColliders)
        // {
        //     Physics2D.IgnoreCollision(col, handLCollider, true);
        //     Physics2D.IgnoreCollision(col, handRCollider, true);
        // }

        if (LazerPointiere != null)
        {
            LazerPointiere.positionCount = 2;
            LazerPointiere.useWorldSpace = true;
            Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
            LazerPointiere.SetPositions(initLaserPositions);
            LazerPointiere.startWidth = PlayerLazerWidth;
            LazerPointiere.endWidth = PlayerLazerWidth;
            LazerPointiere.enabled = false;
        }
    }

    private void Update()
    {
        UpdateGrounded();

        GetInputs();

        UpdateWallHold();

        PlayerPos = transform.position;
    }

    private void GetInputs()
    {
        GatherMoveInput();
        HandleQuickDash();
        HandleFlip();

        UpdateWallHold();
        HandleJump();

        Magnet();
        Pull();
        Push();
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
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
    public enum MagnetType
    {
        Pull,
        Push,
        Lazer
    }

    [Serializable]
    private enum WallSide
    {
        None,
        Left,
        Right
    }
}
