using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    #region Variables
    public Animator m_CameraStateAnimator;
    private Vector3 m_LastDirection;
    // Spawn variables
    public Transform m_SpawnPoint;
    public static Transform m_BossArenaSpawnPoint;

    [Header("Player variables")]
    private Rigidbody2D m_Rigidbody2D;
    public Vector2 m_Movement;
    public bool m_ActivateBossFight;
    public static bool m_HasTriggeredBossFight;

    public float m_DefaultSpeed = 5.0f;
    private float m_CurrentSpeed;
    public float m_RunSpeedMultiplier = 1.5f;

    public float m_DefaultJumpForce = 800.0f;
    private float m_CurrentJumpForce;
    private int m_RemainingExtraJumps;
    public int m_DefaultMaxExtraJumps = 1;
    private int m_CurrentMaxExtraJumps;

    public float m_KnockbackForce;
    private float m_CurrentKnockbackForce;
    private bool m_CanMove;
    private bool m_IsDead;
    public bool m_ReviveTriggered;
    private bool m_NewAbilityUnlocked;

    private Vector2 m_Aim;

    private Vector3 m_MousePosition;
    private Animator m_Animator;
    public Animator m_LanternActionAnimator;
    public Animator m_ScreenTransitionAnimator;
    public SpriteRenderer m_PlayerRenderer;
    public GameObject m_JumpParticlesPrefab;
    public GameObject m_JumpParticlesSpawn;

    [Header("Life and UI")]
    public int m_MaxLifePoints = 5;
    public int m_LifePoints;
    public bool m_InvencibleAfterHit;
    public float m_InvencibleAfterHitDuration;
    private float m_RemainingInvencibleAfterHitDuration;
    private bool m_NoControlAfterHit;
    private float m_NoControlAfterHitDuration;
    private float m_RemainingNoControlAfterHitDuration;
    private GameObject m_CurrentColorIndicator;
    private GameObject m_NextColorIndicator;
    private GameObject m_PreviousColorIndicator;
    private Image m_CurrentColorIndicatorImage;
    private Image m_NextColorIndicatorImage;
    private Image m_PreviousColorIndicatorImage;

    [Header("Ground check variables")]
    public Transform m_GroundCheck;
    public LayerMask m_GroundLayer;
    private bool m_IsGrounded;

    [Header("Input variables")]
    [SerializeField] private float m_DeadZone = 0.4f;
    [HideInInspector] public PlayerControls m_PlayerControls;
    private bool m_IsGamepad;

    private bool m_RunPressed;
    private bool m_JumpPressed;
    private bool m_SitPressed;
    private bool m_SummonLanternPressed;
    private bool m_LeftClickPressed;
    private bool m_RightClickPressed;
    private bool m_MouseWheelPressed;
    private float m_MouseWheel;
    private bool m_AttackingWithMouseWheel;
    private Vector3 m_AimDirection;
    private Vector2 m_ColorSelectInstant;

    [Header("Lantern variables")]
    public GameObject m_Lantern;
    public Transform m_LanternHinge; // The hinge that the lantern will rotate around
    public SpriteRenderer m_LanternRenderer; // The lantern's main renderer
    public float m_DefaultActionCooldown = 1.5f;
    private float m_CurrentActionCooldown;
    public bool m_CanPerformLanternAction;

    [Header("Lantern light variables")]
    private PolygonCollider2D m_LightCollider;
    private LightDamageScript m_LightDamageScript;
    public Color[] m_LanternColors; // Set of colors that the lantern can have
    public GameObject m_LightObject1;
    public GameObject m_LightObject2;

    private int m_CurrentColorIndex; // Index of the current color in the array
    public int m_UnlockedColors; // Number of colors that the player has unlocked
    private bool m_LanternActive; 
    private UnityEngine.Rendering.Universal.Light2D m_Light1;
    private UnityEngine.Rendering.Universal.Light2D m_Light2;

    // Direction variables
    private bool m_GoingRight;
    public bool GoingRight
    {
        get { return m_GoingRight; }
        set 
        { 
            // Flip player and children
            if (m_GoingRight != value)
            {
                transform.localScale = new Vector2
                    (transform.localScale.x * -1, 
                    transform.localScale.y);
            }
            m_GoingRight = value; 
        }
    }

    public float m_CoyoteTime = 0.2f; // Duration of coyote time
    private float m_CoyoteTimeCounter;
    #endregion

    #region Main Methods

    private void OnEnable()
    {
        m_PlayerControls.Enable();
    }

    private void OnDisable()
    {
        m_PlayerControls.Disable();
    }

    public void OnDeviceChange(PlayerInput playerInput)
    {
        m_IsGamepad = playerInput.currentControlScheme.Equals("Controller");
    }

    void Awake()
    {
        m_PlayerControls = new PlayerControls();

        if (m_HasTriggeredBossFight)
        {
            m_HasTriggeredBossFight = false;
            if (GameObject.FindGameObjectWithTag("BossTrigger") != null)
            {
                m_SpawnPoint = GameObject.FindGameObjectWithTag("BossTrigger").transform;
            }
            else
            {
                m_SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
            }
            m_UnlockedColors++;
        }
        else 
        {
            m_SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
        }
        transform.position = m_SpawnPoint.position;
    }

    IEnumerator Start()
    {
        m_LastDirection = transform.position;
        m_LifePoints = m_MaxLifePoints;
        m_CanMove = false;
        Invoke("CanMoveAgain", 1);
        m_NoControlAfterHit = false;
        m_IsDead = false;
        m_GoingRight = true;
        m_CurrentMaxExtraJumps = m_DefaultMaxExtraJumps;
        m_CanPerformLanternAction = true;
        m_InvencibleAfterHit = false;
        m_RemainingInvencibleAfterHitDuration = m_InvencibleAfterHitDuration;
        m_NoControlAfterHitDuration = m_InvencibleAfterHitDuration * 0.75f;
        m_CurrentKnockbackForce = m_KnockbackForce;
        
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_PlayerRenderer = GetComponent<SpriteRenderer>();
        m_LightDamageScript = m_Lantern.GetComponentInChildren<LightDamageScript>();
        m_LightCollider = m_Lantern.GetComponentInChildren<PolygonCollider2D>();

        m_CurrentColorIndicator = GameObject.FindGameObjectWithTag("ColorIndicator");
        m_NextColorIndicator = GameObject.FindGameObjectWithTag("NextColor");
        m_PreviousColorIndicator = GameObject.FindGameObjectWithTag("PreviousColor");

        m_Light1 = m_LightObject1.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        m_Light2 = m_LightObject2.GetComponent<UnityEngine.Rendering.Universal.Light2D>();

        m_CurrentColorIndicatorImage = m_CurrentColorIndicator.GetComponent<Image>();
        m_NextColorIndicatorImage = m_NextColorIndicator.GetComponent<Image>();
        m_PreviousColorIndicatorImage = m_PreviousColorIndicator.GetComponent<Image>();
        
        if (m_UnlockedColors > 1) SetFrameColors();

        yield return new WaitForSeconds(1f);
        ReceiveDamage(0, 0, 0); // This is so that the player knockback works properly.
    }

    void Update()
    {
        HandleLife();
        if (!m_IsDead)
        {
            HandleInputs();
            HandleMovement();
            HandleJump();
            HandleAnimations();
            HandleInvincibility();
            HandlePlayerBenefits();

            if (m_LanternActive)
            {
                AimLantern();
            }

            if (m_CanPerformLanternAction && m_UnlockedColors > 1)
            {
                HandleColorSelection();
                SwitchPlayerColor();
                SwitchLanternColor();
            }
        }
    }

    #endregion

    #region Player Inputs
    // --- PLAYER METHODS ---

    private void HandleInputs()
    {
        if (!m_NoControlAfterHit)
        {
            m_Movement = m_PlayerControls.Player.Movement.ReadValue<Vector2>();

            if (m_Movement.x > m_DeadZone)
            {
                m_Movement.x = 1f;
            }
            else if (m_Movement.x < -m_DeadZone)
            {
                m_Movement.x = -1f;
            }
            else
            {
                m_Movement.x = 0f;
            }

            m_SitPressed = m_PlayerControls.Player.Movement.ReadValue<Vector2>().y < -m_DeadZone;

            m_RunPressed = m_PlayerControls.Player.Run.ReadValue<float>() > 0;

            m_JumpPressed = m_PlayerControls.Player.Jump.triggered;

            m_SummonLanternPressed = m_PlayerControls.Player.Summon.triggered;
            m_LeftClickPressed = m_PlayerControls.Player.Act.triggered;
            m_RightClickPressed = m_PlayerControls.Player.Reset.triggered;
            m_MouseWheelPressed = m_PlayerControls.Player.Attack.triggered;

            m_Aim = m_PlayerControls.Player.Aim.ReadValue<Vector2>();

            if (m_IsGamepad)
            {
                if (m_PlayerControls.Player.ChangeColorNext.triggered)
                {
                    m_MouseWheel = -1;
                }
                else if (m_PlayerControls.Player.ChangeColorPrev.triggered)
                {
                    m_MouseWheel = 1;
                }
                else 
                {
                    m_MouseWheel = 0;
                }
            }
            else
            {
                m_MouseWheel = m_PlayerControls.Player.ChangeColor.ReadValue<float>();
            }

            m_ColorSelectInstant = m_PlayerControls.Player.ColorSelect.ReadValue<Vector2>();
        }
    }
    #endregion

    #region Player Movement
    public void ToggleCanMove()
    {
        m_CanMove = false;
        Invoke("CanMoveAgain", 0.4f);
    }

    private void CanMoveAgain()
    {
        m_CanMove = true;
    }

    private void HandleMovement()
    {
        if (m_CanMove)
        {
            m_IsGrounded = Physics2D.OverlapBox(
                m_GroundCheck.position, 
                new Vector2(0.8f, 0.35f), 
                0, 
                m_GroundLayer); // Create a temporal square box to check if the player is grounded
        }
        if (m_NoControlAfterHit && m_CanPerformLanternAction) // This is basically just for when the player uses the blue ability
        {
            m_Animator.SetTrigger("ForceIdle");
            return;
        }
        
        // Determine the speed multiplier based on whether the player is running or not
        float speedMultiplier;
        if (m_RunPressed && !m_LanternActive)
        {
            speedMultiplier = m_CurrentSpeed * m_RunSpeedMultiplier;
        }
        else
        {
            speedMultiplier = m_CurrentSpeed;
        }

        // Move the player by setting the velocity of the Rigidbody only if it is able to move

        if (m_CanMove)
        {
            Vector2 resultingVelocity = new Vector2(
                m_Movement.x * speedMultiplier * m_CurrentSpeed, 
                m_Rigidbody2D.velocity.y);
            
            m_Rigidbody2D.velocity = resultingVelocity;
        }
        else // If the player can't move, the velocity is set to 0
        {
            m_Rigidbody2D.velocity = Vector2.zero;
        }
        
        // Flip the player's sprite based on the mouse position only if the player is not running
        if (m_IsGamepad)
        {
            m_AimDirection = Vector3.right * m_Aim.x + Vector3.up * m_Aim.y;
            if (m_AimDirection.sqrMagnitude > 0f && (Mathf.Abs(m_Aim.x) > m_DeadZone || Mathf.Abs(m_Aim.y) > m_DeadZone))
            {
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.forward);
                m_MousePosition = rotation * m_AimDirection + transform.position;
            }
        }
        else
        {
            m_MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        m_MousePosition.z = transform.position.z;

        if (m_LanternActive) // The player will flip based on the mouse position only if the lantern is active
        {
            if (m_LastDirection.x + transform.position.x > transform.position.x)
            {
                GoingRight = true;
            }
            else
            {
                GoingRight = false;
            }
        }

        else // The player will flip based on the movement direction if the lantern is not active
        {
            if (m_Movement.x > 0)
            {
                GoingRight = true;
            }
            else if (m_Movement.x < 0)
            {
                GoingRight = false;
            }
        }
    }

    private void HandleJump()
    {
        // Handle jump
        if (m_IsGrounded)
        {
            m_RemainingExtraJumps = m_CurrentMaxExtraJumps;
            m_Animator.ResetTrigger("JumpPressed");
            m_CoyoteTimeCounter = m_CoyoteTime;
        }
        else
        {
            m_CoyoteTimeCounter -= Time.deltaTime;
        }

        // We handle the animation here since it is related to the jump and would require extra checks in the HandleAnimations method
        if ((m_CoyoteTimeCounter > 0) && m_JumpPressed && m_CanMove)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            m_Rigidbody2D.AddForce(Vector2.up * m_CurrentJumpForce);
            m_Animator.SetTrigger("JumpPressed");
            m_CoyoteTimeCounter = 0; // Reset coyote time counter after jump
        }
        else if (!m_IsGrounded && m_JumpPressed && m_CanMove && m_RemainingExtraJumps > 0)
        {
            // Set vertical velocity to 0 and then add the jump force
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            m_Rigidbody2D.AddForce(Vector2.up * m_CurrentJumpForce);
            m_RemainingExtraJumps--;
            SummonAirJumpParticles();
            m_Animator.SetTrigger("JumpPressed");
        }
    }

    private void SummonAirJumpParticles()
    {
        // Instantiate the jump particles prefab rotated 90 degrees
        Instantiate(m_JumpParticlesPrefab, m_JumpParticlesSpawn.transform.position, Quaternion.Euler(0, 0, 90));        
    }
    #endregion

    #region Player Animations
    private void HandleAnimations()
    {
        if (m_Rigidbody2D.velocity.x == 0 && m_Movement.x == 0)
        {
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("IsRunning", false);
        }
        else if (m_Rigidbody2D.velocity.x != 0 && !m_RunPressed || m_Rigidbody2D.velocity.x != 0 && m_RunPressed && m_LanternActive)
        {
            m_Animator.SetBool("IsWalking", true);
            m_Animator.SetBool("IsRunning", false);
        }
        else if (m_Rigidbody2D.velocity.x != 0 && m_RunPressed && !m_LanternActive)
        { 
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("IsRunning", true);
        }
        if (m_SitPressed && m_IsGrounded && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            m_Animator.SetTrigger("SitPressed");
        }

        if (m_SummonLanternPressed && m_UnlockedColors > 0) 
        {
            m_Animator.SetTrigger("ActiveLantern");
        }

        if (m_LeftClickPressed && m_CanPerformLanternAction && m_LanternActive)
        {
            m_Animator.SetTrigger("LanternAction");
        }
        else if (m_LeftClickPressed && m_CanPerformLanternAction && !m_LanternActive)
        {
            if (m_PlayerRenderer.material.color != m_LanternColors[0] && m_PlayerRenderer.material.color != m_LanternColors[1])
            {
                m_Animator.SetTrigger("LanternAction");
            }
        }
        else if (m_MouseWheelPressed && m_CanPerformLanternAction && m_LanternActive)
        {
            m_Animator.SetTrigger("LanternAction");
            m_LanternActionAnimator.SetTrigger("0");
            m_AttackingWithMouseWheel = true;
        }
    }
    #endregion

    #region Player Colors
    private bool CompareColors(Color color1, Color color2, float tolerance = 0.1f)
    {
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
            Mathf.Abs(color1.g - color2.g) < tolerance &&
            Mathf.Abs(color1.b - color2.b) < tolerance;
    }

    private void HandleColorSelection()
    {
        if (m_ColorSelectInstant != Vector2.zero)
        {
            if (Mathf.Abs(m_ColorSelectInstant.x) > Mathf.Abs(m_ColorSelectInstant.y))
            {
                if (m_ColorSelectInstant.x > 0) // Derecha
                {
                    ChangeColorByIndex(2);
                }
                else // Izquierda
                {
                    ChangeColorByIndex(4);
                }
            }
            else
            {
                if (m_ColorSelectInstant.y > 0) // Arriba
                {
                    ChangeColorByIndex(1);
                }
                else // Abajo
                {
                    ChangeColorByIndex(3);
                }
            }
        }
    }

    private void ChangeColorByIndex(int index)
    {
        // Asegurarse de que el índice esté dentro del rango de colores desbloqueados
        if (index > 0 && index < m_UnlockedColors)
        {
            m_CurrentColorIndex = index;
            m_LanternRenderer.material.color = m_LanternColors[m_CurrentColorIndex];
            SetFrameColors();

            // Actualizar el color de las luces
            switch (m_CurrentColorIndex)
            {
                case 1: // Rojo
                    m_Light1.color = new Color(1, 0, 0, 1);
                    m_Light2.color = new Color(1, 0, 0, 1);
                    break;
                case 2: // Azul
                    m_Light1.color = new Color(0, 0, 1, 1);
                    m_Light2.color = new Color(0, 0, 1, 1);
                    break;
                case 3: // Verde
                    m_Light1.color = new Color(0, 1, 0, 1);
                    m_Light2.color = new Color(0, 1, 0, 1);
                    break;
                case 4: // Amarillo
                    m_Light1.color = new Color(1, 0.92f, 0.016f, 1);
                    m_Light2.color = new Color(1, 0.92f, 0.016f, 1);
                    break;
            }
        }
    }

    public void SwitchPlayerColor()
    {
         m_PlayerRenderer.material.color = m_LanternRenderer.material.color;

        if (!m_AttackingWithMouseWheel)
        {
            if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[0])) // White icon
            {
                m_LanternActionAnimator.SetTrigger("0");
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[1])) // Red icon
            {
                m_LanternActionAnimator.SetTrigger("1");
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[2])) // Blue  
            {
                m_LanternActionAnimator.SetTrigger("2");
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[3])) // Green icon
            {
                m_LanternActionAnimator.SetTrigger("3");
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[4])) // Yellow icon
            {
                m_LanternActionAnimator.SetTrigger("4");
            }
        }
        else 
        {
            m_LanternActionAnimator.SetTrigger("0");
        }
    }

    private void HandlePlayerBenefits()
    {
        if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[1])) // Red
        {
            m_CurrentSpeed = m_DefaultSpeed;
            m_CurrentJumpForce = m_DefaultJumpForce;
        }
        else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[2])) // Blue
        {
            m_CurrentSpeed = m_DefaultSpeed;
            m_CurrentJumpForce = m_DefaultJumpForce;
        }
        else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[3])) // Green
        {
            m_CurrentSpeed = m_DefaultSpeed * 0.95f;
            m_CurrentJumpForce = m_DefaultJumpForce * 1.05f;
        }
        else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[4])) // Yellow
        {
            m_CurrentSpeed = m_DefaultSpeed * 1.05f;
            m_CurrentJumpForce = m_DefaultJumpForce * 0.95f;
        }
        else 
        {
            m_CurrentSpeed = m_DefaultSpeed;
            m_CurrentJumpForce = m_DefaultJumpForce;
        }
    }
    #endregion

    #region Player Health
    private void HandleLife()
    {
        if (m_LifePoints <= 0 && !m_IsDead)
        {
            Invoke("FadeInScreen", 0.8f);
            m_IsDead = true;
            m_CanMove = false;
            m_Animator.SetTrigger("Die");
        }
    }

    public void TriggerDeath()
    {
        m_LifePoints = 0;
    }
    
    public void ReceiveDamage(int damage, float enemyXPos, float knockback = -1)
    {
        if (knockback == -1)
        {
            knockback = m_CurrentKnockbackForce;
        }

        if (!m_InvencibleAfterHit && !m_IsDead)
        {
            m_LifePoints -= damage;
            m_InvencibleAfterHit = true;
            m_NoControlAfterHit = true;

            if (enemyXPos < transform.position.x)
            {
                m_Rigidbody2D.AddForce((Vector2.right + Vector2.up) * knockback);
            }
            else
            {
                m_Rigidbody2D.AddForce((Vector2.left + Vector2.up) * knockback);
            }
        }
    }

    private void HandleInvincibility()
    {
        if (m_NoControlAfterHit)
        {
            if (m_RemainingNoControlAfterHitDuration > Mathf.Epsilon) // This is basically > 0
            {
                m_RemainingNoControlAfterHitDuration -= Time.deltaTime;
            }
            else
            {
                m_RemainingNoControlAfterHitDuration = m_NoControlAfterHitDuration;
                m_NoControlAfterHit = false;
            }
        }

        if (m_InvencibleAfterHit)
        {
            if (m_RemainingInvencibleAfterHitDuration > Mathf.Epsilon) // This is basically > 0
            {
                m_RemainingInvencibleAfterHitDuration -= Time.deltaTime;
            }
            else
            {
                m_RemainingInvencibleAfterHitDuration = m_InvencibleAfterHitDuration;
                m_InvencibleAfterHit = false;
            }
        }
    }

    private void FadeInScreen()
    {
        m_ScreenTransitionAnimator.SetTrigger("FadeIN");
        if (m_Rigidbody2D.gravityScale < 0)
        {
            m_Rigidbody2D.gravityScale *= -1;
            transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y * -1);
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            m_Lantern.transform.localScale = new Vector2(m_Lantern.transform.localScale.x, m_Lantern.transform.localScale.y * -1);
            m_LanternRenderer.flipY = !m_LanternRenderer.flipY;
            m_DefaultJumpForce *= -1;

        }
    }
    
    public void TriggerRevival() // Called during death animation
    {
        m_InvencibleAfterHit = false;
        m_LanternActive = false;
        m_Lantern.SetActive(false);
        m_Animator.SetTrigger("Revive");
        m_Rigidbody2D.velocity = Vector2.zero;
        transform.position = m_SpawnPoint.position;
        GoingRight = true;
        if (m_HasTriggeredBossFight)
        {
            Invoke("StartRevival", 0.5f);
        }
        else 
        {
            Invoke("StartRevival", 1);
        }
    }

    public void StartRevival() // Called during revival animation REMOVE
    {
        m_LifePoints = m_MaxLifePoints;
        m_IsDead = false;
        m_CanMove = true;
        m_ScreenTransitionAnimator.SetTrigger("FadeOUT");
        // Reload the scene
        if (m_HasTriggeredBossFight)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
    }
    #endregion

    #region Lantern Methods
    // --- LANTERN METHODS ---

    public void SummonLantern() // Used in the animation event
    {
        if (!m_LanternActive)
        {
            m_LanternActive = true;
            m_Lantern.SetActive(true);
        }
        else
        {
            m_LanternActive = false;
            m_Lantern.SetActive(false);
        }
    }

    private void AimLantern()
    {
        Vector3 direction;

        if (m_IsGamepad)
        {
            Vector2 aimInput = m_PlayerControls.Player.Aim.ReadValue<Vector2>();
            if (aimInput.sqrMagnitude > 0f)
            {
                direction = new Vector3(aimInput.x, aimInput.y, 0).normalized;
                m_LastDirection = direction;
                m_MousePosition = transform.position + direction;
            }
            else
            {
                direction = m_LastDirection; // Use the last direction if no input
            }
        }
        else
        {
            m_MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_MousePosition.z = transform.position.z;
            direction = (m_MousePosition - transform.position).normalized;
            m_LastDirection = direction;
        }

        // Rotate the lantern hinge smoothly towards the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the angle based on the player's direction
        if (!GoingRight)
        {
            angle += 180;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        m_LanternHinge.rotation = Quaternion.Lerp(m_LanternHinge.rotation, targetRotation, Time.deltaTime * 100);
    }

    private void SwitchLanternColor()
    {
        // Extra checks to avoid out of bounds errors

        if (m_UnlockedColors >= m_LanternColors.Length) 
        {
            m_UnlockedColors = m_LanternColors.Length;
        }
        else if (m_UnlockedColors < 1)
        {
            m_UnlockedColors = 0;
        }

        if (m_MouseWheel != 0 || m_NewAbilityUnlocked) // If the mouse wheel is scrolled, the color will cycle through the unlocked colors (excluding default)
        {
            if (m_MouseWheel < 0)
            {
                SelectNextColor();
            }
            else if (m_MouseWheel > 0)
            {
                SelectPreviousColor();
            }

            m_LanternRenderer.material.color = m_LanternColors[m_CurrentColorIndex];
            switch (m_CurrentColorIndex) // Rather than copying the color from the array, the color is set directly because it is otherwise unnoticable
            {
                case 1:
                    m_Light1.color = new Color(1, 0, 0, 1);
                    m_Light2.color = new Color(1, 0, 0, 1);
                    break;
                case 2:
                    m_Light1.color = new Color(0, 0, 1, 1);
                    m_Light2.color = new Color(0, 0, 1, 1);
                    break;
                case 3:
                    m_Light1.color = new Color(0, 1, 0, 1);
                    m_Light2.color = new Color(0, 1, 0, 1);
                    break;
                case 4:
                    m_Light1.color = new Color(1, 0.92f, 0.016f, 1);
                    m_Light2.color = new Color(1, 0.92f, 0.016f, 1);
                    break;
            }

            SetFrameColors();
        }

        if (m_RightClickPressed) // If the right mouse button is pressed, the color will be set to the default color and back to the last color used
        {
            if (m_PlayerRenderer.material.color != m_LanternColors[0]) 
            {
                m_PlayerRenderer.material.color = m_LanternColors[0];
                m_LanternRenderer.material.color = m_LanternColors[0];
                m_Light1.color = new Color(1, 1, 1, 1);
                m_Light2.color = new Color(1, 1, 1, 1);
                m_CurrentColorIndicatorImage.color = m_LanternColors[0];
                m_NextColorIndicatorImage.color = m_LanternColors[0];
                m_PreviousColorIndicatorImage.color = m_LanternColors[0];
            }
            else 
            {
                m_PlayerRenderer.material.color = m_LanternColors[m_CurrentColorIndex];
                m_LanternRenderer.material.color = m_LanternColors[m_CurrentColorIndex];
                switch (m_CurrentColorIndex)
                {
                    case 1:
                        m_Light1.color = new Color(1, 0, 0, 1);
                        m_Light2.color = new Color(1, 0, 0, 1);
                        m_LanternActionAnimator.SetTrigger("1");
                        break;
                    case 2:
                        m_Light1.color = new Color(0, 0, 1, 1);
                        m_Light2.color = new Color(0, 0, 1, 1);
                        m_LanternActionAnimator.SetTrigger("2");
                        break;
                    case 3:
                        m_Light1.color = new Color(0, 1, 0, 1);
                        m_Light2.color = new Color(0, 1, 0, 1);
                        m_LanternActionAnimator.SetTrigger("3");
                        break;
                    case 4:
                        m_Light1.color = new Color(1, 0.92f, 0.016f, 1);
                        m_Light2.color = new Color(1, 0.92f, 0.016f, 1);
                        m_LanternActionAnimator.SetTrigger("4");
                        break;
                }
                SetFrameColors(); // Restore frame colors for other colors
            }
        }
    }

    private void SetFrameColors() // Set the colors of the lantern frames
    {
        m_CurrentColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex];
        if (m_UnlockedColors == 1)
        {
            m_NextColorIndicatorImage.color = m_LanternColors[0];
            m_PreviousColorIndicatorImage.color = m_LanternColors[0];
        }
        else if (m_UnlockedColors == 2)
        {
            m_NextColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex];
            m_PreviousColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex];
        }
        else if ((m_CurrentColorIndex == 0 || m_CurrentColorIndex == 1) && m_UnlockedColors > 2)
        {
            m_NextColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex + 1];
            m_PreviousColorIndicatorImage.color = m_LanternColors[m_UnlockedColors - 1];
        }
        else if (m_CurrentColorIndex == m_UnlockedColors - 1 && m_UnlockedColors > 2)
        {
            m_NextColorIndicatorImage.color = m_LanternColors[1];
            m_PreviousColorIndicatorImage.color = m_LanternColors[m_UnlockedColors - 2];
        }
        else 
        {
            m_NextColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex + 1];
            m_PreviousColorIndicatorImage.color = m_LanternColors[m_CurrentColorIndex - 1];
        }
    }

    private void SelectNextColor() // Select the next color in the array checking how many colors the player has unlocked.  
    {
        if (m_UnlockedColors > 1) // If the number of unlocked colors is 1 (just the default), the color will not change.
        {
            m_CurrentColorIndex++;
            if (m_CurrentColorIndex >= m_UnlockedColors)
            {
                m_CurrentColorIndex = 1; // The color will never loop back to 0 since it is reserved for the default color, accesed with another key.
            }
        }
    }
    private void SelectPreviousColor()
    {
        if (m_UnlockedColors > 1)
        {
            m_CurrentColorIndex--;
            if (m_CurrentColorIndex < 1)
            {
                m_CurrentColorIndex = m_UnlockedColors - 1;
            }
        }
    }

    public void LanternSpecialAction()
    {
        if (!m_CanPerformLanternAction)
        {
            return;
        }
        m_CanPerformLanternAction = false;
        m_Animator.ResetTrigger("LanternAction"); // Without this line, when spamming click, the animation will play twice

        if (m_AttackingWithMouseWheel)
        {
            m_LightDamageScript.m_CurrentLightDamage = m_LightDamageScript.m_DefaultLightDamage;
            m_CurrentActionCooldown = m_DefaultActionCooldown * 0.3f;
            Attack();
            m_AttackingWithMouseWheel = false;
        }
        else
        {
            if (m_LanternActive)
            {
                if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[0])) // White
                {
                    // Attack
                    m_LightDamageScript.m_CurrentLightDamage = m_LightDamageScript.m_DefaultLightDamage;
                    m_CurrentActionCooldown = m_DefaultActionCooldown * 0.3f;
                    Attack();
                }
                else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[1])) // Red
                {
                    
                    m_LightDamageScript.m_CurrentLightDamage = m_LightDamageScript.m_DefaultLightDamage * 2;
                    m_CurrentActionCooldown = m_DefaultActionCooldown * 0.6f;
                    Attack();
                }
            }
            
            if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[2])) // Blue
            {
                // Invincible for 0.8 seconds, but can't move during most of it (the player is still allowed to move a bit before the invincibility ends)
                m_LightDamageScript.m_CurrentLightDamage = 0;
                m_InvencibleAfterHit = true;
                m_NoControlAfterHit = true;
                m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
                m_Movement.x = 0;
                m_RemainingInvencibleAfterHitDuration = m_DefaultActionCooldown * 0.8f;

                m_Rigidbody2D.gravityScale *= 2;
                StartCoroutine(ReturnGravityToNormal(0.8f));
                
                m_CurrentActionCooldown = m_DefaultActionCooldown;
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[3])) // Green
            {
                // Jump that doesn't consume extra jumps
                m_LightDamageScript.m_CurrentLightDamage = 0;
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
                m_Rigidbody2D.AddForce(Vector2.up * m_CurrentJumpForce * 1.35f);

                if (!m_IsGrounded)
                {
                    SummonAirJumpParticles();
                }

                m_Animator.SetTrigger("JumpPressed");
                
                m_CurrentActionCooldown = m_DefaultActionCooldown;
            }
            else if (CompareColors(m_PlayerRenderer.material.color, m_LanternColors[4])) // Yellow
            {
                m_LightDamageScript.m_CurrentLightDamage = 0;
                // Invert the player's gravity and its sprite vertically
                m_Rigidbody2D.gravityScale *= -1;
                transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y * -1);
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
                m_Lantern.transform.localScale = new Vector2(m_Lantern.transform.localScale.x, m_Lantern.transform.localScale.y * -1);
                m_LanternRenderer.flipY = !m_LanternRenderer.flipY;

                m_DefaultJumpForce *= -1;
                
                m_CurrentActionCooldown = 0.8f;
            }
        }
        
        StartCoroutine(LanternCooldown());
    }

    private void Attack()
    {
        m_LightCollider.enabled = true;
        StartCoroutine(DeactivateLanternCollider());
    }

    private IEnumerator LanternCooldown()
    {
        yield return new WaitForSeconds(m_CurrentActionCooldown);
        m_CanPerformLanternAction = true;
    }

    private IEnumerator ReturnGravityToNormal(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_Rigidbody2D.gravityScale /= 2;
    }

    private IEnumerator DeactivateLanternCollider()
    {
        yield return new WaitForSeconds(0.1f);
        m_LightCollider.enabled = false;
    }

    #endregion

    #region Collision Methods
    // --- COLLISION METHODS ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            EnemyScript thisEnemy = collision.gameObject.GetComponentInParent<EnemyScript>();
            BossProyectileScript thisBossProyectile = collision.gameObject.GetComponent<BossProyectileScript>();
            m_Rigidbody2D.velocity = Vector2.zero;

            if ((thisEnemy != null || thisBossProyectile != null) 
                && m_InvencibleAfterHit 
                && CompareColors(m_PlayerRenderer.material.color, m_LanternColors[2])
                && m_Movement.x == 0)
            {
                if (thisEnemy != null)
                {
                    thisEnemy.GetDamage(thisEnemy.m_DamageDealtToPlayer);
                }
                return;
            }
            else
            {
                StartCoroutine(InvencibilityFlash());
            }

            if (thisEnemy != null)
            {
                ReceiveDamage(thisEnemy.m_DamageDealtToPlayer, collision.transform.position.x);
            }
            if (thisBossProyectile != null)
            {
                ReceiveDamage(thisBossProyectile.m_DamageDealtToPlayer, collision.transform.position.x);
            }
        }
    }

    private IEnumerator InvencibilityFlash()
    {
        float duration = m_InvencibleAfterHitDuration / 8;
        Color originalColor = m_PlayerRenderer.material.color;
        float originalAlpha = 1f;
        float newAlpha = 0.05f;

        for (int i = 0; i < 4; i++)
        {
            float lerpTime = 0f;
            while (lerpTime < duration)
            {
                float alpha = Mathf.Lerp(originalAlpha, newAlpha, lerpTime / duration);
                Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                m_PlayerRenderer.material.color = newColor;
                lerpTime += Time.deltaTime;
                yield return null;
            }

            lerpTime = 0f;
            while (lerpTime < duration)
            {
                float alpha = Mathf.Lerp(newAlpha, originalAlpha, lerpTime / duration);
                Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                m_PlayerRenderer.material.color = newColor;
                lerpTime += Time.deltaTime;
                yield return null;
            }
        }

        m_PlayerRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalAlpha);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeathBox"))
        {
            m_LifePoints = 0;
        }

        if (collision.CompareTag("CheckPoint"))
        {
            m_SpawnPoint = collision.transform;
            collision.GetComponent<BoxCollider2D>().enabled = false;
            collision.GetComponent<CapsuleCollider2D>().enabled = true;
        }

        if (collision.CompareTag("ColorPickup"))
        {
            Destroy(collision.gameObject);
            
            m_UnlockedColors++;
            if (m_UnlockedColors == 4)
            {
                m_CurrentMaxExtraJumps = 1;
            }

            m_CurrentColorIndex = m_UnlockedColors - 1;
            SetFrameColors();
            m_LanternActive = false;
            m_CurrentActionCooldown = m_DefaultActionCooldown;

            m_Animator.SetTrigger("ActiveLantern");
            Invoke("ToggleNewAbility", 0.4f);
        }

        if (collision.CompareTag("BossTrigger") && !m_HasTriggeredBossFight)
        {
             m_CameraStateAnimator.SetTrigger("StartBossFight");

            // Deactivate the box collider and activate the capsule collider
            collision.GetComponent<BoxCollider2D>().enabled = false;
            collision.GetComponent<CapsuleCollider2D>().enabled = true;
            collision.GetComponent<SpriteRenderer>().enabled = true;

            if (SceneManager.GetActiveScene().name == "Level4" || SceneManager.GetActiveScene().name == "Level5")
            {
                m_UnlockedColors = 5;
                if (m_MaxLifePoints == 9)
                {
                    m_MaxLifePoints = 10;
                }
                else if (m_MaxLifePoints <= 8)
                {
                    m_LifePoints = m_MaxLifePoints + 2;
                }
                m_CurrentMaxExtraJumps = 1;
            }
            else 
            {
                m_LifePoints = m_MaxLifePoints + 1;
            }
            
            m_ActivateBossFight = true;
            m_HasTriggeredBossFight = true;
            m_BossArenaSpawnPoint = collision.transform;
            m_SpawnPoint = m_BossArenaSpawnPoint;
        }

        if (collision.CompareTag("EndLevel"))
        {
            m_HasTriggeredBossFight = false;
            m_ScreenTransitionAnimator.SetTrigger("FadeWHITE");
            Invoke("GoToNextLevel", 1);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Spikes") && !m_InvencibleAfterHit)
        {
            m_LifePoints = 0;
        }
    }

    #endregion

    private void GoToNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ToggleNewAbility()
    {
        m_NewAbilityUnlocked = true;
        Invoke("StopNewAbility", 0.1f);
    }

    private void StopNewAbility()
    {
        m_NewAbilityUnlocked = false;
    }
}
