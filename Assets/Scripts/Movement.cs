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
    //For cutscenes
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

    [Header("Dash Stuff")]
    [SerializeField] private float dashforce = 25f;
    [SerializeField] private int dashcounter = 0;
    [SerializeField] private float dashtime = 0.2f;
    
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

    public void Jump(InputAction.CallbackContext context)
    {
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
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
}