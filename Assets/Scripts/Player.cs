using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
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
    public bool IsAlive => health.currentHealth > 0f;
    
    public string currentGunId;
    public string secondaryGunId;
    public Gun gun;
    public Gun secondaryGun;
    public BoxCollider2D mainCollider;

    private bool isGrounded;

    private float lastGroundedTime;
    private float jumpPressTime;
    private int airJumpsLeft;
    public Health health;
    
    [HideInInspector] public float jetPackFuel;
    
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
    [HideInInspector] public float jetFuelMult = 10f;
    
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioSource jetAudio;
    public AudioClip groundPoundSound;
    public AudioClip groundPoundStartSound;
    public GameObject groundPoundEffectPrefab;
    private bool isOnIce;
    private bool isInMud;
    private float lastBounceTime = -100f;
    private float lastContactDamageTime = -100f;
    [FormerlySerializedAs("reloadIndicator")] public Image secondaryReloadIndicator;
    public Image primaryReloadIndicator;
    public GameObject gunParent;
    public GameObject secondaryGunParent;
    
    private void Awake()
    {
        I = this;
        health = GetComponent<Health>();
        secondaryReloadIndicator.gameObject.SetActive(false);
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
        SaveManager.I.LoadGame();
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
        if (health.currentHealth <= 0 || Level.I.isLevelTransition || GM.IsUIOpen)
        {
            jetAudio.Stop();
            jetPackParticleSystem.Stop();
            return;
        }
        Vector2 moveInput = movementAction.ReadValue<Vector2>();
        var movementSpeed = isInMud ? 0.1f * speed : speed;
        Vector2 moveVelocity = moveInput * movementSpeed;
        var isDashing = Time.time - dashStartTime < dashDuration;
        if (!isDashing)
        {
            dashDirection = Mathf.Abs(moveInput.x) <= 0.01f ? dashDirection : Mathf.Sign(moveInput.x);
        }
        if (isOnIce)
        {
            rb.linearVelocityX = rb.linearVelocityX == 0f
                ? dashDirection * movementSpeed
                : movementSpeed * Mathf.Sign(rb.linearVelocityX);
        }
        else
        {
            rb.linearVelocityX = moveVelocity.x;
        }

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
            var jumpHeight = jumpForce + stats.jumpHeight * 3f;
            if (isInMud)
            {
                jumpHeight *= 0.5f;
            }

            rb.linearVelocityY = jumpHeight;
            if (!isGroundedJump)
            {
                airJumpsLeft--;
            }
            isGrounded = false;
            jumpPressTime = -100f;
            AnimateJump();
        }
        
        var gunVector = VirtualMouseController.I.mousePosition -
                        (Vector2)Camera.main.WorldToScreenPoint(gunParent.transform.position);

        gunParent.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        gun.animator.spriteRenderer.flipY = gunVector.x < 0f;
        if (shootAction.IsPressed())
        {
            gun.Shoot();
        }

        if (secondaryGun != null)
        {
            var secondaryGunVector = VirtualMouseController.I.mousePosition -
                            (Vector2)Camera.main.WorldToScreenPoint(secondaryGunParent.transform.position);
            secondaryGunParent.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(secondaryGunVector.x, secondaryGunVector.y) * Mathf.Rad2Deg - 90f);
            secondaryGun.animator.spriteRenderer.flipY = gunVector.x < 0f;
            if (grenadeAction.IsPressed())
            {
                secondaryGun.Shoot();
            }
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

        numDashesLeft += Time.deltaTime * dashRechargeRate;
        numDashesLeft = Mathf.Min(numDashesLeft, stats.numDashes);
        if (dashAction.WasPerformedThisFrame() && numDashesLeft >= 1)
        {
            numDashesLeft--;
            dashStartTime = Time.time;
        }
        
        if (isDashing)
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
            rb.linearVelocityY = -jumpForce * 5f; // Increase downward force for ground pound
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

        var topRightTile = Level.I.GetTileInfo(topRight + Vector2.right * 0.1f);
        var bottomRightTile = Level.I.GetTileInfo(bottomRight + Vector2.right * 0.1f);
        var topLeftTile = Level.I.GetTileInfo(topLeft + Vector2.left * 0.1f);
        var bottomLeftTile = Level.I.GetTileInfo(bottomLeft + Vector2.left * 0.1f);
        var leftTopTile = Level.I.GetTileInfo(topLeft + Vector2.up * 0.1f);
        var rightTopTile = Level.I.GetTileInfo(topRight + Vector2.up * 0.1f);
        var leftBottomTile = Level.I.GetTileInfo(bottomLeft + Vector2.down * 0.1f);
        var rightBottomTile = Level.I.GetTileInfo(bottomRight + Vector2.down * 0.1f);
        var allContacts = new[]{ topRightTile, bottomRightTile, topLeftTile, bottomLeftTile,
            leftBottomTile, rightBottomTile, leftTopTile, rightTopTile }.Where(x => x != null);
        foreach (var contact in allContacts)
        {
            HandleContact(contact);
        }

        var rightHit = bottomRightTile != null || topRightTile != null;
        var leftHit = bottomLeftTile != null || topLeftTile != null;

        if (rightHit && rb.linearVelocityX > 0 ||
            leftHit && rb.linearVelocityX < 0)
        {
            rb.linearVelocityX = 0;
        }

        isGrounded = leftBottomTile != null || rightBottomTile != null;
        isOnIce = leftBottomTile?.tileData.isSlippery == true || rightBottomTile?.tileData.isSlippery == true;
        isInMud = leftBottomTile?.tileData.isMud == true || rightBottomTile?.tileData.isMud == true;
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            airJumpsLeft = stats.numJumps;
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
        gun?.Reset();
        secondaryGun?.Reset();
    }

    private void AnimateJump()
    {
        SoundManager.I.PlaySfx(jumpSound, transform.position);
        var main = jumpParticleSystem.main;
        main.startSpeedMultiplier = -5f * (stats.jumpHeight + 1.1f);
        jumpParticleSystem.Play();
        transform.DOScaleY(transform.localScale.z * 0.8f, 0.1f)
            .OnComplete(() => transform.DOScaleY(transform.localScale.z * 1.2f, 0.3f))
            .OnComplete(() => transform.DOScaleY(transform.localScale.z * 1f, 0.1f));
    }
    
    private void AnimateDash()
    {
        SoundManager.I.PlaySfx(dashSound, transform.position);
        dashParticleSystem.transform.parent.localRotation = Quaternion.Euler(0f, 0f, dashDirection <= 0f ? 90f : -90f);
        dashParticleSystem.Play();
    }

    private void HandleContact(Level.TileInfo tile)
    {
        var isWithinInteractionRange = 
            Vector2.Distance(Level.I.grid.GetCellCenterWorld(tile.pos) , transform.position + (Vector3)mainCollider.offset) < 0.9f;
        if (!isWithinInteractionRange)
        {
            return;
        }
        if (tile.tileData.contactDamage != 0 && Time.time - lastContactDamageTime > 0.5f)
        {
            health.Damage(tile.tileData.contactDamage, DamageDealerType.Environment);
            lastContactDamageTime = Time.time;
        }

        if (tile.tileData.outForce != 0f && Time.time - lastBounceTime > 0.5f)
        {
            var diff = transform.position - Level.I.grid.GetCellCenterWorld(tile.pos);
            diff.z = 0;
            if (diff.y >= 0.25f)
            {
                diff.y = 1f;
                diff.x = 0f;
            } else if (diff.y < -0.5f)
            {
                diff.y = -1f;
                diff.x = 0f;
            } 
            else
            {
                diff.x = Mathf.Sign(diff.x);
                diff.y = 0f;
            }
            
            rb.linearVelocity = diff.normalized * tile.tileData.outForce;
            lastBounceTime = Time.time;
        }
    }

    public void SetGun(Gun newGun)
    {
        if (newGun == null)
            return;
        var gunGunStation = newGun.isSecondary ? secondaryGun?.gunStation : gun?.gunStation;
        if (gunGunStation != null)
        {
            gunGunStation.ResetGun();
        } else
        {
            if (newGun.isSecondary && secondaryGun != null) {
                Destroy(secondaryGun.gameObject);
            } else if (!newGun.isSecondary && gun != null) {
                Destroy(gun.gameObject);
            }
        }

        if (newGun.isSecondary)
        {
            secondaryGun = newGun;
            secondaryGun.transform.parent = secondaryGunParent.transform;
            secondaryGun.transform.localPosition = Vector3.zero;
            secondaryGun.transform.localRotation = Quaternion.identity;
            secondaryGun.reloadIndicator = secondaryReloadIndicator;
            secondaryReloadIndicator.gameObject.SetActive(true);
            secondaryGunId = newGun.id;
        }
        else
        {
            gun = newGun;
            gun.transform.parent = gunParent.transform;
            gun.transform.localPosition = Vector3.zero;
            gun.transform.localRotation = Quaternion.identity;
            gun.reloadIndicator = primaryReloadIndicator;
            currentGunId = newGun.id;
        }
        newGun.Reset();
    }
}