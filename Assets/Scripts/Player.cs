using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player I;
    
    public SpriteAnimator.Animation walkAnimation;
    public SpriteAnimator.Animation idleAnimation;
    public Stats stats;
    public float speed = 5f;
    public float jumpForce = 300f;
    public float additionalGravity = 2f;
    public Rigidbody2D rb;
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
    public SpriteAnimator animator;
    
    public ParticleSystem jumpParticleSystem;
    public ParticleSystem dashParticleSystem;
    public ParticleSystem jetPackParticleSystem;
    public Image[] dashIndicators;
    public Image jetFuelIndicator;
    private float jetFuelMult = 10f;
    
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioSource jetAudio;
    public AudioClip groundPoundSound;
    public AudioClip groundPoundStartSound;
    public GameObject groundPoundEffectPrefab;
    
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
        for (int i = 0; i < dashIndicators.Length; i++)
        {
            var isActive = i < stats.numDashes;
            dashIndicators[i].gameObject.SetActive(isActive);
            if (!isActive)
            {
                continue;
            }
            dashIndicators[i].fillAmount = Mathf.Clamp01(numDashesLeft - i);
            dashIndicators[i].color = numDashesLeft + Mathf.Epsilon >= i+1 ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
        }
        if (health.currentHealth <= 0 || Level.I.isLevelTransition)
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
            AnimateJump();
        }

        var gunVector = Mouse.current.position.ReadValue() -
                        (Vector2)Camera.main.WorldToScreenPoint(gun.transform.position);

        gun.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        gun.animator.spriteRenderer.flipY = gunVector.x < 0f;
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
            if (!jetPackParticleSystem.isPlaying)
            {
                jetPackParticleSystem.Play();
            }
            jetPackFuel -= Time.deltaTime * jetPackFuelConsumptionRate;
            rb.linearVelocityY = jetPackSpeed;
            if (!jetAudio.isPlaying)
            {
                jetAudio.Play();
            }
        }
        else
        {
            jetPackParticleSystem.Stop();
            if (jetAudio.isPlaying)
            {
                jetAudio.Stop();
            }
        }
        jetFuelIndicator.fillAmount = jetPackFuel / (stats.jetPackFuel * jetFuelMult);
        jetFuelIndicator.rectTransform.sizeDelta = new Vector2(jetFuelIndicator.rectTransform.sizeDelta.x,  stats.jetPackFuel * 150f);

        numDashesLeft += Time.deltaTime * dashRechargeRate;
        numDashesLeft = Mathf.Min(numDashesLeft, stats.numDashes);
        dashDirection = Mathf.Abs(moveInput.x) <= 0.01f ? dashDirection : Mathf.Sign(moveInput.x);
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
            AnimateDash();
        }
        else
        {
            health.isInvulnerable = false; // Reset invulnerability after dash
        }
        
        if (groundPoundAction.WasPerformedThisFrame() && !isGrounded && stats.groundPound > 0 && !isPounding)
        {
            rb.linearVelocityY = -jumpForce * 2f; // Increase downward force for ground pound
            isPounding = true;
            SoundManager.I.PlaySfx(groundPoundStartSound, transform.position);
        }

        if (stats.healthRegen > 0)
        {
            health.Heal(stats.healthRegen * Time.deltaTime * 2f);
        }

        if (rb.linearVelocityX != 0f)
        {
            animator.animation = walkAnimation;
            animator.spriteRenderer.flipX = rb.linearVelocityX < 0f;
        }
        else
        {
            animator.animation = idleAnimation;
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
                    enemyDamage:stats.groundPound * 20f,
                    groundDamage:stats.groundPound * 20f,
                    DamageDealerType.Player);
                Destroy(Instantiate(groundPoundEffectPrefab, transform.position, Quaternion.identity, Level.I.spawnedObjectsParent), 2f);
                isPounding = false;
                SoundManager.I.PlaySfx(groundPoundSound, transform.position, 10f);
            }
        }
    }

    public void Revive()
    {
        jetPackFuel = stats.jetPackFuel * jetFuelMult;
        numDashesLeft = stats.numDashes;
        health.maxHealth = 300f + stats.health * 100f;
        health.Revive();
    }

    private void AnimateJump()
    {
        SoundManager.I.PlaySfx(jumpSound, transform.position);
        var main = jumpParticleSystem.main;
        main.startSpeedMultiplier = -5f * (stats.jumpHeight + 1.1f);
        jumpParticleSystem.Play();
        transform.DOScale(0.8f, 0.1f)
            .OnComplete(() => transform.DOScale(1.2f, 0.3f))
            .OnComplete(() => transform.DOScale(1f, 0.1f));
    }
    
    private void AnimateDash()
    {
        SoundManager.I.PlaySfx(dashSound, transform.position);
        dashParticleSystem.transform.parent.localRotation = Quaternion.Euler(0f, 0f, dashDirection <= 0f ? 90f : -90f);
        dashParticleSystem.Play();
    }
}