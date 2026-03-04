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
    private Rigidbody2D rb;
    [SerializeField] private float JumpForce;
    private Vector2 moveInput;
    //Below is for pumpkin destroying
    private bool EPressed;
    public float PumpkinsDestroyed = 0;
    [SerializeField] private TextMeshProUGUI PumpkinText;
    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip Ding;
    //For cutscenes
    public bool CanMove = true;

    //Below is for Player Animation
    Animator anim;
    private Vector2 lastMoveDirection;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        ProcessInputs();
        //Animate();

        //aPumpkinText.text = $"Pumpkins: {PumpkinsDestroyed}";


    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        Vector2 movevelocity = new Vector2(moveInput.x * moveSpeed, velocity.y);
        rb.linearVelocity = movevelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (CanMove)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (EPressed)
        {
            if (collision.gameObject.CompareTag("Pumpkin"))
            {
                Destroy(collision.gameObject);
                PumpkinsDestroyed++;

                if (PumpkinsDestroyed == 6)
                {
                    //doorunlock.UnlockDoor();
                    Sfx.PlayOneShot(Ding, 0.7f);
                }

            }
        }
    }

    public void Pickup()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            EPressed = true;
        }
        else
        {
            EPressed = false;
        }
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

    void Animate()
    {
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        anim.SetFloat("MoveMagnitude", moveInput.magnitude);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }
}