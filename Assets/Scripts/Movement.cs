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

    // Gets componenets
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //If player faces the left but moves right they flip sides
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

        //Checks if the wall holding button is pressed and if so, sets the bool to true
        var WallHold = playerInput.actions["Hold On Wall"];
        if (WallHold.IsPressed() && !CancelWallHold)
        {
            CanHoldWall = true;
        }
        else
        {
            CanHoldWall = false;
        }

        //If the wall hold button is pressed and player is on wall and player hasn't pressed jump yet, then they hang on wall
        //Aka: zero gravity
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
        //If they aren't on wall, movement is as usual
        if (!IsOnWall)
        {
            Vector2 velocity = rb.linearVelocity;
            Vector2 movevelocity = new Vector2(moveInput.x * moveSpeed, velocity.y);
            rb.linearVelocity = movevelocity;
        }
        //if they are on wall, the movement is zero.
        else if (IsOnWall)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        //if they can move (not in cutscene or whatever) and arent on a wall then unity will record the movement input
        if (CanMove && !IsOnWall)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    //Magnet pull mechanic
    //This starts a coroutine timer
    public void Pull()
    {
        StartCoroutine(PullTimer());
    }

    //This is the pull coroutine timer, It activates pull, then deactivates
    private IEnumerator PullTimer(){
        magnet.isActive = true;
        Debug.Log("Pulling");
        yield return new WaitForSeconds(MagnetDuration);
        magnet.isActive = false;
        Debug.Log("Stopped Pulling");
        StopCoroutine(PullTimer());
    }

    //This is the push coroutime timer, once again it activates a timer
    public void Push()
    {
        StartCoroutine(PushThing());
    }

    //and this is the timer to push objects
    private IEnumerator PushThing()
    {
        pushPoint.enabled = true;
        yield return new WaitForSeconds(pushduration);
        pushPoint.enabled = false;
        StopCoroutine(PushThing());
    }

    //Jump mechanic
    public void Jump(InputAction.CallbackContext context)
    {
        //If player is on ground then they can jump, if not, then they cant. Prevents double jumping
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);   
        }

        //If player is on left wall they jump up and to the right, well suppoosedly because that doesnt work for some reason.
        if(WhichWallWasTouched == "L")
        {
            CanJumpOnWall = true;
            CanHoldWall = false;
            IsOnWall = false;
            CancelWallHold = true;
            rb.gravityScale = 1f;
            rb.AddForce(Vector2.right * WallJumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * WallJumpForce, ForceMode2D.Impulse);
            CancelWallHold = false;
            //CanJumpOnWall = false;
        }
        //Same here, but with right wall, again the left jump wont work for some reason.
        else if(WhichWallWasTouched == "R")
        {
            CanJumpOnWall = true;
            CanHoldWall = false;
            IsOnWall = false;
            CancelWallHold = true;
            rb.gravityScale = 1f;
            rb.AddForce(Vector2.left * WallJumpForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * WallJumpForce, ForceMode2D.Impulse);
            CancelWallHold = false;
            //CanJumpOnWall = false;
        }
    }
    //This is the function that flips the player when they turn left or right
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    //This is a dash mechanic that doesnt work well so its kinda abandoned and i'll work on it later
    public void Dash()
    {
         dashcounter++;
         CanMove = false;
         rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
         Vector2 dashDirection = new Vector2(moveInput.x * dashforce, 0);
         rb.linearVelocity = dashDirection;
         Invoke("EndDash",dashtime); 
    }

    //This is in conjunction with the dash mechanic, again its abandoned
    private void EndDash()
    {
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        CanMove = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //If player is on anything but wall they can jump normally
        if (collision.gameObject.layer != 8)
        {
            isGrounded = true;
            CancelWallHold = false;
        }
        //if they are on a wall it starts some veriable stuff that makes wall jumping work
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

    //Once they leave wall or ground or whatever they just can't jump.
    private void OnCollisionExit2D(Collision2D collision)
    {
      isGrounded = false;
      IsOnWall = false;
      WhichWallWasTouched = null;
      CanJumpOnWall = false;
    }
}