using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player I;
    public int money;

    public Stats stats;
    public float speed = 5f;
    public float jumpForce = 300f;
    public float additionalGravity = 2f;
    private Rigidbody2D rb;
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private InputAction grenadeAction;
    public InputAction dashAction;
    public InputAction jetPackAction;
    public InputAction groundPoundAction;
    
    public Gun gun;
    public Gun grenadeLauncher;
    public BoxCollider2D mainCollider;

    private bool isGrounded;
    private LayerMask groundMask;

    private float lastGroundedTime;
    private float jumpPressTime;
    private int airJumpsLeft;
    public Health health;
    
    private float jetPackFuel;
    
    public float jetPackFuelConsumptionRate = 1f; // Fuel consumed per second while using jetpack
    public float jetPackSpeed = 2f; // Force applied by the jetpack
    private float numDashesLeft = 1;
    public float dashRechargeRate = 1f; // Dashes per second
    public float dashSpeed = 10f; // Speed of the dash
    public float dashDuration = 0.2f; // Duration of the dash in seconds
    private float dashStartTime = -100f;
    private float dashDirection = 1f; // Direction of the dash, 1 for right, -1 for left
    
    private bool isPounding; // Time since the last ground pound action

    private void Awake()
    {
        I = this;
        health = GetComponent<Health>();
    }

    private void Start()
    {
        movementAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        shootAction = InputSystem.actions.FindAction("Attack");
        grenadeAction = InputSystem.actions.FindAction("Grenade");
        dashAction = InputSystem.actions.FindAction("Dash");
        jetPackAction = InputSystem.actions.FindAction("JetPack");
        groundPoundAction = InputSystem.actions.FindAction("GroundPound");

        rb = GetComponent<Rigidbody2D>();
        groundMask = LayerMask.GetMask("Ground");
        Revive();
    }

    private void Update()
    {
        if (health.currentHealth <= 0)
        {
            return;
        }
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

        var isGroundedJump = Time.time - lastGroundedTime < 0.2f;
        if (Time.time <= jumpPressTime + 0.2f && (isGroundedJump || airJumpsLeft > 0))
        {
            rb.linearVelocityY = jumpForce + stats.jumpHeight * 3f;
            if (!isGroundedJump)
            {
                airJumpsLeft--;
            }
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
        if (grenadeAction.IsPressed())
        {
            grenadeLauncher.Shoot();
        }

        if (jetPackAction.IsPressed() && jetPackFuel > 0f)
        {
            jetPackFuel -= Time.deltaTime * jetPackFuelConsumptionRate;
            rb.linearVelocityY = jetPackSpeed;
        }

        numDashesLeft += Time.deltaTime * dashRechargeRate;
        numDashesLeft = Mathf.Min(numDashesLeft, stats.numDashes);
        dashDirection = Mathf.Sign(moveInput.x) == 0f ? dashDirection : Mathf.Sign(moveInput.x);
        if (dashAction.WasPerformedThisFrame() && numDashesLeft > 0)
        {
            numDashesLeft--;
            dashStartTime = Time.time;
        }
        if (Time.time - dashStartTime < dashDuration)
        {
            rb.linearVelocityX = dashDirection * dashSpeed;
            rb.linearVelocityY = 0f; // Reset vertical velocity during dash
            health.isInvulnerable = true; // Make player invulnerable during dash
        }
        else
        {
            health.isInvulnerable = false; // Reset invulnerability after dash
        }
        
        if (groundPoundAction.WasPerformedThisFrame() && !isGrounded && stats.groundPound > 0f && !isPounding)
        {
            rb.linearVelocityY = -jumpForce * 2f; // Increase downward force for ground pound
            isPounding = true;
        }

        if (stats.healthRegen > 0)
        {
            health.Heal(stats.healthRegen * Time.deltaTime * 1f);
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
            airJumpsLeft = stats.numJumps;
            numDashesLeft = stats.numDashes;
            if (isPounding)
            {
                Level.I.Explode(transform.position, 
                    radius:stats.groundPound * 3f, 
                    enemyDamage:stats.groundPound * 2f,
                    groundDamage:stats.groundPound,
                    DamageDealerType.Player);
                isPounding = false;
            }
        }
    }

    public void Revive()
    {
        jetPackFuel = stats.jetPackFuel * 10f;
        numDashesLeft = stats.numDashes;
        health.maxHealth = 30f + stats.health * 10f;
        health.Revive();
    }
}