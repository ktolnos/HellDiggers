using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 300f;
    public float additionalGravity = 2f;
    private Rigidbody2D rb;
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    public Gun gun;
    public BoxCollider2D mainCollider;

    private bool isGrounded;
    private LayerMask groundMask;

    private float lastGroundedTime;
    private float jumpPressTime;

    private void Start()
    {
        movementAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        shootAction = InputSystem.actions.FindAction("Attack");

        rb = GetComponent<Rigidbody2D>();
        groundMask = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        Vector2 moveInput = movementAction.ReadValue<Vector2>();
        Vector2 moveVelocity = moveInput * speed;
        rb.linearVelocityX = moveVelocity.x;

        if (jumpAction.WasPerformedThisFrame())
        {
            jumpPressTime = Time.time;
        }

        if (!isGrounded && rb.linearVelocityY < 0)
        {
            rb.linearVelocityY -= additionalGravity * Time.deltaTime;
        }

        if (Time.time <= jumpPressTime + 0.2f && Time.time - lastGroundedTime < 0.2f)
        {
            rb.linearVelocityY = jumpForce;
            isGrounded = false;
            jumpPressTime = -100f;
        }

        var gunVector = Mouse.current.position.ReadValue() -
                        (Vector2)Camera.main.WorldToScreenPoint(gun.transform.position);

        gun.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        if (shootAction.IsPressed())
        {
            gun.Shoot();
        }
    }

    private void FixedUpdate()
    {
        var bounds = mainCollider.bounds;
        var bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        var bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        var topLeft = new Vector2(bounds.min.x, bounds.max.y);
        var topRight = new Vector2(bounds.max.x, bounds.max.y);
        
        var rightHit = Physics2D.Raycast(topRight, Vector2.right, 0.1f, groundMask) ||
                       Physics2D.Raycast(bottomRight, Vector2.right, 0.1f, groundMask);
        var leftHit = Physics2D.Raycast(topLeft, Vector2.left, 0.1f, groundMask) ||
                      Physics2D.Raycast(bottomLeft, Vector2.left, 0.1f, groundMask);


        if (rightHit && rb.linearVelocityX > 0 ||
            leftHit && rb.linearVelocityX < 0)
        {
            rb.linearVelocityX = 0;
        }

        isGrounded = Physics2D.Raycast(bottomLeft, Vector2.down, 0.1f, groundMask) ||
                     Physics2D.Raycast(bottomRight, Vector2.down, 0.1f, groundMask);
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
}