using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Movement : MonoBehaviour
{
    //[SerializeField] private DoorUnlock doorunlock;
    //Below is for movement
    [SerializeField] private float moveSpeed = 5f;
    public Rigidbody2D rb;
    [SerializeField] private float JumpForce;
    public Vector2 moveInput;

    public bool CanMove = true;

    //Below is for Player Animation
    Animator anim;
    private Vector2 lastMoveDirection;

    private Vector3 lookdirection;
    public LayerMask ropeLayerMask;
    public float distance = 90.0f;
    float horizontalmove;

    public Magnet magnet;

    public float MagnetDuration;
    public SpriteRenderer spriteRenderer;

    public PointEffector2D pushPoint;
    public float pushduration;
    public SwitchGravity switchGravity;
    private bool m_FacingRight = true;

    public GameObject player;

    public PlayerInput playerInput;

    public float PlayerVelX;
    public float PlayerVelY;

    public bool isGrounded;
    public LayerMask groundLayerMask;
    public float GroundRaycastNum;

    [Header("Dash Stuff")]
    [SerializeField] private float dashforce = 25f;
    [SerializeField] private int dashcounter = 0;
    [SerializeField] private float dashtime = 0.2f;

    [Header("Wall Sliding")]
    [SerializeField] private bool CanHoldWall = false;
    [SerializeField] private bool CancelMovement = false;
    [SerializeField] private bool IsOnWall = false;

    [SerializeField] private float WallJumpForce;

    [SerializeField] private string WhichWallWasTouched;
    [SerializeField] private bool CancelWallHold = false;

    private bool CanJumpOnWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        ProcessInputs();
        //Animate();

        if (moveInput.x > 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (moveInput.x < 0 && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }

        PlayerVelX = rb.linearVelocity.x;
        PlayerVelY = rb.linearVelocity.y;


        var WallHold = playerInput.actions["Hold On Wall"];
        if (WallHold.IsPressed() && !CancelWallHold)
        {
            CanHoldWall = true;
        }
        else
        {
            CanHoldWall = false;
        }


        if (CanHoldWall && IsOnWall && !CanJumpOnWall)
        {
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOnWall)
        {
            Vector2 velocity = rb.linearVelocity;
            Vector2 movevelocity = new Vector2(moveInput.x * moveSpeed, velocity.y);
            rb.linearVelocity = movevelocity;
        }
        else if (IsOnWall)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (CanMove && !IsOnWall)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        horizontalmove = UnityEngine.Input.GetAxisRaw("Horizontal");
    }

    public void Pull()
    {
        StartCoroutine(PullTimer());
    }

    private IEnumerator PullTimer(){
        magnet.isActive = true;
        Debug.Log("Pulling");
        yield return new WaitForSeconds(MagnetDuration);
        magnet.isActive = false;
        Debug.Log("Stopped Pulling");
        StopCoroutine(PullTimer());
    }

    public void Push()
    {
        StartCoroutine(PushThing());
    }

    private IEnumerator PushThing()
    {
        pushPoint.enabled = true;
        yield return new WaitForSeconds(pushduration);
        pushPoint.enabled = false;
        StopCoroutine(PushThing());
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);   
        }

        if(WhichWallWasTouched == "L")
        {
            CanJumpOnWall = true;
            CanHoldWall = false;
            IsOnWall = false;
            CancelWallHold = true;
            rb.gravityScale = 1f;
            rb.AddForce(Vector2.right * WallJumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * WallJumpForce, ForceMode2D.Impulse);
        }
        else if(WhichWallWasTouched == "R")
        {
            CanJumpOnWall = true;
            CanHoldWall = false;
            IsOnWall = false;
            CancelWallHold = true;
            rb.gravityScale = 1f;
            rb.AddForce(Vector2.left * WallJumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * WallJumpForce, ForceMode2D.Impulse);
        }
    }
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    public void SwitchOrientation()
    {

    }
    public void Dash()
    {
         dashcounter++;
         CanMove = false;
         rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
         Vector2 dashDirection = new Vector2(moveInput.x * dashforce, 0);
         rb.linearVelocity = dashDirection;
         Invoke("EndDash",dashtime); 
    }

    private void EndDash()
    {
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        CanMove = true;
    }

    public void MagnetMovement()
    {
        lookdirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookdirection, distance, ropeLayerMask);

        Vector2 MoveDir = hit.point;

        rb.MovePosition(MoveDir);
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if ((moveX == 0 && moveY == 0))
        {
            lastMoveDirection.x = moveInput.x;
            lastMoveDirection.y = moveInput.y;
        }
        //Debug.Log($"MoveX is {moveX}");
        //Debug.Log($"MoveY is {moveY}");
        //Debug.Log($"LastMoveDir.x is {lastMoveDirection.x}");
        //Debug.Log($"LastMoveDir.y is {lastMoveDirection.y}");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer != 8)
        {
            isGrounded = true;
            CancelWallHold = false;
        }
        else if(collision.gameObject.layer == 8)
        {
            if (CanHoldWall)
            {
                IsOnWall = true;
                isGrounded = false;

                if (collision.gameObject.CompareTag("LWall"))
                {
                    WhichWallWasTouched = "L";
                }
                else if (collision.gameObject.CompareTag("RWall"))
                {
                    WhichWallWasTouched = "R";
                }
            }
            else
            {
                IsOnWall = false;
                isGrounded = false;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    

    private void OnCollisionExit2D(Collision2D collision)
    {
      isGrounded = false;
      IsOnWall = false;
      WhichWallWasTouched = null;
      CanJumpOnWall = false;
    }
}