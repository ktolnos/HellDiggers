using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 300f;
    private Rigidbody2D rb;
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    public Gun gun;

    private bool isGrounded;

    private void Start()
    {
        movementAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        shootAction = InputSystem.actions.FindAction("Attack");
        
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        Vector2 moveInput = movementAction.ReadValue<Vector2>();
        Vector2 moveVelocity = moveInput * speed;
        rb.linearVelocityX = moveVelocity.x;

        if (jumpAction.WasPerformedThisFrame() && isGrounded)
        {
            rb.linearVelocityY = jumpForce;
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
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, 
            LayerMask.GetMask("Ground"));
    }
}
