using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

[Flags]
public enum MovementState : byte // I can have more than 1 states active in one enum which is useful for the dashing state
{       
    Grounded = 1,
    InAir = 2,
    Dashing = 4,
    Launched = 8,
    LedgeGrabbing = 16,
}

[Flags]
public enum CombatState : byte
{
    Neutral = 1,
    Attacking = 2,
    Blocking = 4,
    HitStun = 8,
    Unstoppable = 16,
}

public class PlayerController : MonoBehaviour, IKnockbackable, IDamageable
{
    #region Variables
    
    public MovementState movementState = MovementState.Grounded;
    public CombatState combatState = CombatState.Neutral;

    [Header("Player in game Stats")] 
    [SerializeField] private float totalDamageTaken;
    [SerializeField] private int stocks;
    [SerializeField] public GameObject playerIngameUI;

    [Header("Player Core")]
    public int playerID;
    [SerializeField] protected Vector2 moveInput; // Float that holds the value of the input manager (-1 = left, 0 = neutral, 1 = right)
    
    [SerializeField] private float movementSpeed; // Base movementspeed
    [SerializeField] private float sprintMultiplier; // Multiplies the base movementspeed for a sprint speed
    [SerializeField] private float diStrength; // How much player can influence their movement when launched
    
    [SerializeField] private float gravityForce; // Gravity strength
    [SerializeField] private float gravityForceLaunched; // Gravity strength when being launched, for more floaty feel
    
    [SerializeField] private float jumpForce; // Jump strength
    [SerializeField] private int maxJumps; // Total jumps player can have in total
    [SerializeField] private int jumpsLeft; // Amount of jumps left
    
    public Transform playerTransform;
    [Header("Dash Stats")] [SerializeField]
    private float dashForce; // Dash Strength

    [SerializeField]
    private float dashTime; // Duration of the dash PLayer doesnt have control of the character for this amount

    [Header("Sprint logic")] [SerializeField]
    private float doubleTapThreshold; // Time window you have for multi tapping and to start sprinting
    
    [SerializeField]
    protected float deadZoneThreshold; // The value that moveInput needs to be to count as a movement input

    [SerializeField] private float lastTapTime; // Stores the time of the last movement input to detect double taps
    private bool isSprinting;
    private bool wasMovingLastFrame; // Flag to check if you moved last frame

    [Header("Ground Check")] [SerializeField]
    private LayerMask groundCheckLayer; // The ground layer, used for checking if the player is grounded 

    [SerializeField]
    private float groundCheckDistance; // Half of the characters height + a little bit for ground check raycast
    
    [Header("LedgeGrab Check")]
    [SerializeField] private LayerMask ledgeLayer;
    
    [Header("Launch Logic")] 
    [SerializeField] private int weight; // Weight determines the players knockback, heavier weight less knockback
    [SerializeField] private float knockbackReduceSpeed; // Controls how quickly knockback slows down
    [SerializeField] private float knockbackDurationFactor; // Factor used for calculating the time that needs to be reduced when hit by an attack
    
    [SerializeField] private float velocityMganitude; // Current launch velocity being applied on the player 
    
    private float minKnockback;
    private bool isBeingKnocked = false;
    
    private Vector2 knockbackDirection; // The ammount of force that should be applied on both axis from 0 to 1 
    private Vector2 knockbackVelocity; 
    
    [Header("Attack Logic")] 
    [SerializeField] private LayerMask player;
    private Coroutine attackCoroutine; // Variable to hold the attack coroutine so it can be cancelled
    
    // List of hitboxes for light attacks 
    [SerializeField] private List<HitBoxes> lightHitboxList = new List<HitBoxes>(); 
    [SerializeField] private List<HitBoxes> heavyHitboxList = new List<HitBoxes>(); 
    
    // Dictionary for all character attacks
    private Dictionary<string, Collider[]> lightHitboxes = new Dictionary<string, Collider[]>(); 
    private Dictionary<string, Collider[]> heavyHitboxes = new Dictionary<string, Collider[]>(); 

    [SerializeField] private List<AttackData> attackData = new List<AttackData>();

    private AttackData currentAttack;
    private Collider[] colliders;
    
    [SerializeField] private float chargeStartTime; // Used to track how long the player is charging
    [SerializeField] private float chargeRatio;
    [SerializeField] private float chargeDamage;
    
    private bool performingAttack; // Bool to keep track if the coroutine is ongoing or not
    public bool[] attackType; // 0 is light attack 1 is heavy attack
    public bool isCharging; // Bool to check if player is using a charge attack
    
    [Header("Blocking Logic")] 
    [SerializeField] private float totalBlockingTime;
    [SerializeField] private float shieldReductionFactor;
    [SerializeField] private float currentBlockingTime;
    [SerializeField] private float blockRechargeTime;
    [SerializeField] private GameObject blockingObject; 
        
    [Header("Ultimate logic")] 
    [SerializeField] protected float maxUltCharge;
    [SerializeField] protected float currentUltCharge;
    
    protected bool usingUltimate = false;

    [Header("Death")]
    [SerializeField] private float respawnTime;
    public bool touchedDeathZone;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem launchedParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem healParticle;
    
    [Header("Camera Control")] // Need to change camera logic 
    [SerializeField] private float cameraFollowWeight;
    [SerializeField] private float cameraFollowRadius;
    
    [Header("Animation Logic")]
    protected string lastAnimation;
    protected bool isTriggerActive = false;
    
    [Header("RB logic")]
    protected Rigidbody rb;

    [SerializeField] private float groundedDrag;
    [SerializeField] private float inAirDrag;
    
    [Header("Animator Reference")]
    protected Animator animator;
    
    [Header("UI")]
    protected bool gamePaused;
    #endregion
    
    #region Unity Methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        combatState = CombatState.Neutral; 
        
       
        foreach (var pair in lightHitboxList) // Setting up the light attack dictionary
        {
            if (!lightHitboxes.ContainsKey(pair.attackName))
            {
                lightHitboxes[pair.attackName] = pair.collider;
            }
        }

        foreach (var pair in heavyHitboxList) // Setting up the heavy attack dictionary
        {
            if (!heavyHitboxes.ContainsKey(pair.attackName))
            {
                heavyHitboxes[pair.attackName] = pair.collider;
            }
        }
        
        ResetJumps();
    }

    protected virtual void Update()
    {
        if (gamePaused) return;
        
        LedgeGrab();
        Blocking();
        
        AnimationStates(); // Manages the animations
        
        if(isCharging) ChargeLogic();
        // if (currentUltCharge < maxUltCharge) ChargeUltimate();
    }

  
    private void FixedUpdate() // Use FixedUpdate for physics
    {
        if (!rb.isKinematic && !gamePaused)
        {    
            IsGrounded();
            ApplyGravity();
            UpdateKnockback();
            Move();
        }
    }
    #endregion

    #region Manager Setups

    public void GameStartSetup()
    {
        GameplayManager.Instance.players.Add(gameObject);
        GameplayManager.Instance.playersAlive.Add(gameObject);
    }

    #endregion

    #region Movement

    private void ApplyGravity() 
    {
        switch (movementState)
        {
            case MovementState.Launched:
                rb.AddForce(Vector3.down * gravityForceLaunched, ForceMode.Force);
                break;
            case MovementState.InAir:
                rb.AddForce(Vector3.down * gravityForce, ForceMode.Force);
                break;
        }
    }

    private void Move()
    {
        if (isBeingKnocked || usingUltimate)
        {
            return;
        }
        switch (movementState)
        {
            case MovementState.LedgeGrabbing:
                // Cant move/rotate while ledge grabbing

                break;

            case MovementState.Launched:
                // Let the player have some agency to move while being launched 
                rb.velocity += new Vector3(moveInput.x * diStrength * Time.deltaTime, moveInput.y * diStrength * Time.deltaTime, 0);
                break;
            
            default:
                Vector3 velocity = rb.velocity;
                velocity.x = moveInput.x * (isSprinting ? movementSpeed * sprintMultiplier : movementSpeed);
                rb.velocity = velocity;

                if (Mathf.Abs(moveInput.x) > deadZoneThreshold && combatState != CombatState.Attacking)
                {
                    transform.rotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0, 0));
                }

                break;
           
        }
    }

    private bool CanMove()
    {   
        // attackType 1 are heavy attacks, cant move while using/performing them 
        if (isCharging || attackType[1] || combatState == CombatState.Blocking || combatState == CombatState.HitStun 
            || movementState == MovementState.Launched || isBeingKnocked) 
        {
            return false;
        }

        return true;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (usingUltimate)
        {
            moveInput = ctx.ReadValue<Vector2>(); // Get X-axis input
            return;
        }
        
        if (!CanMove())
        {
            moveInput = Vector2.zero; 
            return;
        }
        
        moveInput = ctx.ReadValue<Vector2>(); // Get X-axis input

        //print("moveInput: " + moveInput.x);
        bool isMovingNow = Mathf.Abs(moveInput.x) > deadZoneThreshold; // Consider movement if input is significant

        if (isMovingNow && !wasMovingLastFrame) // Detect "new" movement input
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            if (timeSinceLastTap <= doubleTapThreshold && movementState == MovementState.Grounded)
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }

            lastTapTime = Time.time; // Update last tap time
        }

        wasMovingLastFrame = isMovingNow; // Store movement state
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!CanPerformAction()) return;
        switch (movementState)
        {
            case MovementState.LedgeGrabbing:
                SetTriggerAnimation("Jump");
                rb.isKinematic = false;
                movementState = MovementState.InAir;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                
                break;
            
            case MovementState.Launched:
                // Can't jump
                break;
            default:
                if (jumpsLeft > 0 && ctx.performed && combatState == CombatState.Attacking)
                {
                    jumpsLeft--;
                    
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                }
                else if (jumpsLeft > 0 && ctx.performed)
                {
                    jumpsLeft--;
                    
                    SetTriggerAnimation("Jump");
                    
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                }
                break;
            
        }
    }
    
    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!CanPerformAction() || !ctx.performed || moveInput.x == 0 || movementState == MovementState.InAir) return;
        
        Dash(moveInput.x);
        
    }
    private void Dash(float direction)
    {
        direction = Mathf.Sign(direction);
        
        SetTriggerAnimation("Dash");
        Vector3 dashVelocity = new Vector3(dashForce * direction, 0, 0);
        dashVelocity.y = rb.velocity.y; // Preserve gravity effect
        rb.AddForce(dashVelocity, ForceMode.Impulse);
        
        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        movementState = IsGrounded() ? (MovementState)(byte)5 : (MovementState)(byte)6;

        yield return new WaitForSeconds(dashTime); // Wait for dash duration
        
        movementState = IsGrounded() ? MovementState.Grounded : MovementState.InAir;
    }

    private void LedgeGrab() // Try ledgeGrabbing on forward and backside of the character 
    {
        if (rb.velocity.y >= 0 || movementState == MovementState.LedgeGrabbing) return;
        
        Vector3[] directions = { transform.forward, -transform.forward }; // Check front & back

        foreach (var direction in directions)
        {
            TryLedgeGrab(direction);
        }
    }

    private void TryLedgeGrab(Vector3 direction)
    {
        // Make a begin and end position for the linecast based on players height 
        Vector3 lineDownStart = playerTransform.position + Vector3.up * 1.2f + direction;
        Vector3 lineDownEnd = playerTransform.position + Vector3.up * 1f + direction;

        Debug.DrawLine(lineDownStart, lineDownEnd, Color.red, 2f);

        // Shoot a lineCast in front and back of the player to check for a ledgegrabbable ledge 
        if (!Physics.Linecast(lineDownStart, lineDownEnd, out RaycastHit downHit, ledgeLayer))
            return;

        Vector3 lineForwardStart = new Vector3(transform.position.x, downHit.point.y, transform.position.z);
        Vector3 lineForwardEnd = lineForwardStart + direction;

        Debug.DrawLine(lineForwardStart, lineForwardEnd, Color.green, 2f);

        // Shoot a lineCast at the height of the previous hitpoint to check where the ledge is
        if (!Physics.Linecast(lineForwardStart, lineForwardEnd, out RaycastHit forwardHit, ledgeLayer))
            return;

        // Player shouldn't be affected by forces when ledgegrabbing 
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        movementState = MovementState.LedgeGrabbing;

        // Calculate where to set the player position to ledgegrab
        Vector3 hangPosition = new Vector3(forwardHit.point.x, downHit.point.y, forwardHit.point.z);
        Vector3 targetOffset = -direction * 0.1f + Vector3.up * -0.8f;

        Vector3 lookDirection = (forwardHit.point - transform.position).normalized;
        lookDirection.y = 0; // Keep only horizontal rotation

        // Apply rotation only on the Y-axis
        if (lookDirection != Vector3.zero) // Prevents errors when direction is (0,0,0)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        transform.position = hangPosition + targetOffset;
        
        CancelAttack();
    }
    private bool HasState(MovementState state) => (movementState & state) != 0;
    private bool HasState(CombatState state) => (combatState & state) != 0;

    private bool CanPerformAction()
    {
        return !(HasState(MovementState.Dashing) || HasState(MovementState.Launched) ||
                 HasState(CombatState.Blocking) || touchedDeathZone || gamePaused || usingUltimate);
    }
    private void OnCollisionEnter(Collision collision) // Having both a ray and collision detection helps for easier walljump implementation with certain objects
    {
        if (collision.gameObject.transform.CompareTag("Wall Jump"))
        {
            ResetJumps();
        }
    }

    private bool IsGrounded()
    {
        if ((movementState & MovementState.Dashing) != 0) return true;

        bool grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCheckDistance, groundCheckLayer);
         
        if (grounded)
        {
            movementState = MovementState.Grounded;
            rb.drag = groundedDrag;
        }
        else if (movementState != MovementState.LedgeGrabbing && movementState != MovementState.Launched)
        {
            movementState = MovementState.InAir;
            rb.drag = inAirDrag;
        }

        return grounded;
    }
    
    private void ResetJumps()
    {
        jumpsLeft = maxJumps;
    }

    #endregion

    #region Combat Logic

    #region Attacking and Hit logic

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !CanPerformAction() || !CanAttack()) return;
        
        string direction = GetAttackDirection(moveInput);
        string moveName = $"Light {movementState.ToString()} {direction}";

        if (!lightHitboxes.TryGetValue(moveName, out Collider[] colliders))
        {
            Debug.LogWarning($"{moveName} does not exist");
            return;
        }

        AttackData currentAttack = attackData.Find(attack => attack.attackName == moveName);

        if (currentAttack != null)
        {
            attackCoroutine = StartCoroutine(PerformAttack(currentAttack, colliders, 0));
        }
        else
        {
            Debug.LogWarning($"No attack data found for {moveName}");
        }

        print(moveName);

    }

    public void OnHeavytAttack(InputAction.CallbackContext ctx)
    {
        // Start charging when button is pressed
        if (ctx.started && CanPerformAction() && CanAttack())
        {
            string direction = GetAttackDirection(moveInput);
            string moveName = $"Heavy {movementState.ToString()} {direction}"; 
            
            chargeStartTime = 0f;
            chargeStartTime = Time.time;
            
            rb.velocity = Vector3.zero;
            
            if (!heavyHitboxes.TryGetValue(moveName, out Collider[] collidersFound))
            {
                Debug.LogWarning($"{moveName} does not exist");
                return;
            }
            
            colliders = collidersFound;
            currentAttack = attackData.Find(attack => attack.attackName == moveName);
            
            if (!currentAttack.chargeAttack)
            {
                attackCoroutine = StartCoroutine(PerformAttack(currentAttack, colliders, 1));
            }
            else
            {
                SetAnimation(currentAttack.chargeAnimation.name);
                isCharging = true;
                print($"Play animation {currentAttack.chargeAnimation.name}");
            }
        }
        else if (ctx.canceled && isCharging) // Release charge when button is released
        {
            float chargeDuration = Mathf.Clamp(Time.time - chargeStartTime, 0, currentAttack.maxChargeTime);
            
            ReleaseCharge(chargeDamage);
        }
    }

    private void ChargeLogic()
    {
        float chargeDuration = Mathf.Clamp(Time.time - chargeStartTime, 0, currentAttack.maxChargeTime);
        chargeRatio = chargeDuration / currentAttack.maxChargeTime;
            
        chargeDamage = Mathf.Lerp(currentAttack.minChargeDamage, currentAttack.maxChargeDamage, chargeRatio);

        if (chargeRatio >= 1)
        {
            ReleaseCharge(chargeDamage);
        }
    }
    
    private void ReleaseCharge(float chargeDamage)
    {
        print("Charge released");
        attackCoroutine = StartCoroutine(PerformChargeAttack(currentAttack, chargeDamage, colliders, 1));

        chargeRatio = 0;
    }

    private IEnumerator PerformAttack(AttackData attackData, Collider[] colliders, int attackTypeIndex) 
    {
        combatState = CombatState.Attacking;
        attackType[attackTypeIndex] = true;
        SetTriggerAnimation(attackData.animation.name);

        if (attackData.moveOnAttack)
        {
               Vector3 direction = attackData.movementDirection;
               rb.AddForce(direction, ForceMode.Impulse);
        }
        
        if (attackData.unstoppable) // If the attack has the property unstoppable it won't get cancelled when hit
        {
            combatState = (CombatState)(byte)18; 
        }
        performingAttack = true;

        yield return new WaitForSeconds(attackData.startupTime);

        foreach (var collider in colliders)
        {
            collider.enabled = true;
            DetectHits(attackData, collider);
        }

        yield return new WaitForSeconds(attackData.activeTime);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        yield return new WaitForSeconds(attackData.moveCooldown);

        combatState = CombatState.Neutral;
        attackType[attackTypeIndex] = false;
        performingAttack = false;
    }
    private IEnumerator PerformChargeAttack(AttackData attackData, float chargedDamage ,Collider[] colliders, int attackTypeIndex) 
    {
        combatState = CombatState.Attacking;
        attackType[attackTypeIndex] = true;
        SetTriggerAnimation(attackData.animation.name);

        isCharging = false;
        if (attackData.moveOnAttack)
        {
             Vector3 direction = attackData.movementDirection;
             rb.AddForce(direction, ForceMode.Impulse);
        }
        
        if (attackData.unstoppable) // If the attack has the property unstoppable it won't get cancelled when hit
        {
            combatState = (CombatState)(byte)18; 
        }
        performingAttack = true;

        yield return new WaitForSeconds(attackData.startupTime);

        foreach (var collider in colliders)
        {
            if (collider.enabled == false)
            {
                DetectChargedHits(attackData, chargedDamage ,collider);
                collider.enabled = true;
            }
        }
        
        yield return null;
        
        yield return new WaitForSeconds(attackData.activeTime);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        yield return null;

        yield return new WaitForSeconds(attackData.moveCooldown);

        combatState = CombatState.Neutral;
        performingAttack = false;
        attackType[attackTypeIndex] = false;
    }
    
    private void DetectHits(AttackData attackData, Collider attackCollider)
    {
        attackCollider.gameObject.GetComponent<AttackColliders>().isHeavyAttack = false;
        attackCollider.gameObject.GetComponent<AttackColliders>().attackData = attackData;
        attackCollider.gameObject.GetComponent<AttackColliders>().playerTransform = playerTransform;
    }
    
    private void DetectChargedHits(AttackData attackData, float damage, Collider attackCollider)
    {
        attackCollider.gameObject.GetComponent<AttackColliders>().isHeavyAttack = true;
        attackCollider.gameObject.GetComponent<AttackColliders>().attackData = attackData;
        attackCollider.gameObject.GetComponent<AttackColliders>().chargedDamage = damage;
        attackCollider.gameObject.GetComponent<AttackColliders>().playerTransform = playerTransform;
    }

    private void CancelAttack() // Cancel attack when certain interactions are met
    {
        attackType[0] = false;
        attackType[1] = false;

        if (combatState == CombatState.Attacking )
        {
             StopCoroutine(attackCoroutine);
        }
       
        combatState = CombatState.Neutral;
    }

    public void TakeDamage([CanBeNull] AttackData attackData, Transform enemyTransform, float damage)
    {
        switch (combatState)
        {
            default:
                if (attackData)
                {
                    totalDamageTaken += damage;
                    
                    TakeKB(attackData, enemyTransform, attackData.knockback); // Apply's a knockback if the move has knockback property's
                   
                    playerIngameUI.GetComponent<PlayerUIManager>().UpdatePlayerHealthUI(playerID,totalDamageTaken);
                }
                else // Explosion damage
                {
                    totalDamageTaken += damage;
                    playerIngameUI.GetComponent<PlayerUIManager>().UpdatePlayerHealthUI(playerID,totalDamageTaken);
                }
                break;
            case CombatState.Blocking:
                currentBlockingTime -= attackData.damage * shieldReductionFactor;

                break;
        }
    }
    public void TakeDamage(AttackData attackData,float chargedDamage ,Transform enemyTransform)
    {
        switch (combatState)
        {
            default:
                totalDamageTaken += chargedDamage;
                print($"Took {chargedDamage} damage");

                TakeKB(attackData, enemyTransform, attackData.knockback); // Apply's a knockback if the move has knockback property's
                
                playerIngameUI.GetComponent<PlayerUIManager>().UpdatePlayerHealthUI(playerID,totalDamageTaken);
                break;
            case CombatState.Blocking:
                currentBlockingTime -= attackData.damage * shieldReductionFactor;

                break;
        }
    }

    public void Heal(int healAmount)
    {
        totalDamageTaken -= healAmount;
        totalDamageTaken = Mathf.Clamp(totalDamageTaken, 0, Mathf.Infinity);
        
        healParticle.Play();
        playerIngameUI.GetComponent<PlayerUIManager>().UpdatePlayerHealthUI(playerID ,totalDamageTaken);
    }
    
    private void UpdateKnockback()
    {
        if (isBeingKnocked)
        {
            if (Mathf.Abs(rb.velocity.x) < velocityMganitude)
            {
                // Reset knockback state more comprehensively
                isBeingKnocked = false;
                movementState = IsGrounded() ? MovementState.Grounded : MovementState.InAir;
                launchedParticles.gameObject.SetActive(false);
                return;
            }
            movementState = MovementState.Launched;
            if (!launchedParticles.gameObject.activeInHierarchy)
            {
                 launchedParticles.gameObject.SetActive(true);
            }
            
            // Gradually reduce knockback with a more controlled approach
            knockbackVelocity *= knockbackReduceSpeed;
            rb.velocity = new Vector3(knockbackVelocity.x, knockbackVelocity.y, 0);
          
        }
    }

    public void TakeKB([CanBeNull] AttackData attackData, Transform kbSource, float kb)
    {
        if (attackData)
        {
            minKnockback = attackData.minKnockback;
            knockbackDirection = attackData.hitDirection;
        }
        else
        {
            minKnockback = 0;
            knockbackDirection = Vector2.one;
        }

        // Formula for knockback intensity 
        float knockbackIntensity = (((totalDamageTaken / 100) * kb * (200 / (weight + 100)) + minKnockback));

        // Get base direction from attacker to target
        Vector3 baseDirection = (transform.position - kbSource.position).normalized;

        // Use the attack's hit direction directly
        Vector3 finalDirection = new Vector3(
            Mathf.Sign(baseDirection.x) * knockbackDirection.x,
            knockbackDirection.y,
            0
        );

        // Calculate and apply launch force
        knockbackVelocity = finalDirection * knockbackIntensity;

        // Set knockback state
        movementState = MovementState.Launched;
        isBeingKnocked = true;

        // Clear any current velocity before applying knockback
        rb.velocity = Vector3.zero;

        // Apply initial impulse
        rb.velocity = new Vector3(knockbackVelocity.x, knockbackVelocity.y, 0);

        // Additional debug logging
        Debug.Log($"Knockback Applied - Intensity: {knockbackIntensity}, Direction: {knockbackVelocity}");
    } 
    
    private bool CanAttack()
    {
        if (movementState == MovementState.Grounded && combatState == CombatState.Neutral ||
            movementState == MovementState.InAir && combatState == CombatState.Neutral || combatState == CombatState.Blocking)
        {
            return true;
        }

        return false;
    }

    private string GetAttackDirection(Vector2 inputDir)
    {
        if (inputDir.magnitude < deadZoneThreshold)
        {
            return "Neutral"; // No direction pressed
        }

        if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
        {
            return inputDir.x > 0 ? "Right" : "Left";
        }
        else
        {
            return inputDir.y > 0 ? "Up" : "Down";
        }
    }

    
    #endregion

    #region Blocking Logic

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (ctx.started && CanBlock()) // When button is pressed
        {
            if (!CanPerformAction()) return;
            Debug.Log("Blocking started");
            combatState = CombatState.Blocking;
            rb.velocity = Vector3.zero; 
            blockingObject.SetActive(true);
        }

        if (ctx.canceled && combatState == CombatState.Blocking)
        {
            Debug.Log("Blocking stopped");
            combatState = CombatState.Neutral;
            blockingObject.SetActive(false);
        }
    }
    
    private bool CanBlock()
    {
        if (movementState == MovementState.Grounded && combatState == CombatState.Neutral)
        {
            return true;
        }

        return false;
    }

    private void Blocking()
    {
        if (combatState != CombatState.Blocking)
        {
            if (currentBlockingTime < totalBlockingTime) // Recharge block
            {
                currentBlockingTime = Mathf.MoveTowards(currentBlockingTime, totalBlockingTime, Time.deltaTime * blockRechargeTime);
                currentBlockingTime = Mathf.Clamp(currentBlockingTime, 0, totalBlockingTime);
            }
        }
        else // If blocking is true
        {
            
            currentBlockingTime -= Time.deltaTime;
            currentBlockingTime = Mathf.Clamp(currentBlockingTime, 0, totalBlockingTime);
            
            
            if (currentBlockingTime == 0)
            {
                combatState = CombatState.Neutral;
                blockingObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Ult Logic

    private void ChargeUltimate()
    {   
        currentUltCharge += Time.deltaTime;
    }
    public virtual void OnUltimateCast(InputAction.CallbackContext ctx)
    { 
        // Every Character (if we add more) Has their own ultimate \
        if (currentUltCharge <= maxUltCharge)
        {
            return;
        }
    }
        

    #endregion
    
    #region Death

    private void OnTriggerEnter(Collider other) // When you hit the outer borders the player should die 
    {
        if (other.CompareTag("Border"))
        {
            if (!touchedDeathZone)
            {
                OnStockLost();
                
                touchedDeathZone = true;
                knockbackVelocity = Vector2.zero;
                rb.velocity = Vector2.zero;
            
                // Get the closest point on the border collider to the player
                Vector3 closestPoint = other.ClosestPoint(transform.position);
            
                // Calculate direction from the border to the player (this is the normal)
                Vector3 collisionNormal = (transform.position - closestPoint).normalized;
            
                // Calculate rotation based on the normal
                Quaternion particleRotation = Quaternion.LookRotation(collisionNormal);
            
                // Calculate a position closer to the collision point
                // Adjust the multiplier (0.25f) to control how close to the border the effect appears
                Vector3 spawnPosition = closestPoint + collisionNormal * 0.25f;
            
                // Instantiate death particle with the calculated rotation and adjusted position
                ParticleSystem deathParticle = Instantiate(deathParticles, spawnPosition, particleRotation);
                Destroy(deathParticle.gameObject, 10);
                
            }
        }
    }
    
    
    private void OnStockLost()
    {
        // play particle and death sound
        if (!touchedDeathZone)
        {
             stocks--;
             rb.isKinematic = true;
        }
       
        
        if (stocks > 0)
        {
            Respawn();
        }
        else
        {
            Die();
        }
    }

    private void Respawn()
    {
        totalDamageTaken = 0;
        playerIngameUI.GetComponent<PlayerUIManager>().UpdatePlayerHealthUI(playerID, totalDamageTaken);
        knockbackVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        
        movementState = IsGrounded() ? MovementState.Grounded : MovementState.InAir;
        StartCoroutine(GameplayManager.Instance.RespawnPlayer(gameObject));
    }

    private void Die()
    {
        WinScreenAnimatie.Instance.AssignPlayerPlacement(playerID);
        GameplayManager.Instance.PlayerDeath(gameObject);
    }
    #endregion


    #endregion

    #region Other Inputs 

    public virtual void OnRightAnalogStickMove(InputAction.CallbackContext ctx)
    {
      
    }

    #endregion 

    #region Animator

    private void AnimationStates() // Sets animation bools based on state
    {
        if (isTriggerActive && combatState == CombatState.Attacking) return;
        
        if (isCharging)
        {
            SetAnimation(currentAttack.chargeAnimation.name);
            return;
        }
        
        switch (combatState)
        {
            case CombatState.Blocking:
                SetAnimation("Blocking");
                return;
                break;
            case CombatState.HitStun:
                
                break;
            case CombatState.Neutral:
                
                break;
        }

        switch (movementState)
        {   
            case MovementState.Dashing:
              SetAnimation("Dashing");
                break;
            case MovementState.Grounded:
                if (isSprinting)
                {
                    SetAnimation("Sprinting");
                }
                else if (Mathf.Abs(moveInput.x) > deadZoneThreshold)
                {
                   SetAnimation("Walking");
                }
                else
                {
                    SetAnimation("Idle");
                }
                break;
            
             case MovementState.Launched:
                 SetAnimation("Launched");
                 break;
             
            case MovementState.LedgeGrabbing:
                SetAnimation("LedgeGrabbing");
                
                break;
            
            case MovementState.InAir:
                    SetAnimation("Falling");
                break;
        }
    }

    private void SetAnimation(string animation)
    {
        if (lastAnimation == animation) return; // Prevent resetting the animation

        if (!string.IsNullOrEmpty(lastAnimation)) 
        {
            animator.SetBool(lastAnimation, false); // Disable the last animation
        }

        animator.SetBool(animation, true); // Enable the new animation
        lastAnimation = animation; // Store the last played animation
    }
    
    private void SetTriggerAnimation(string trigger)
    {
        isTriggerActive = true; // Prevent other animations from playing
        animator.SetTrigger(trigger);

        // Make sure this animation has priority over others 
        StartCoroutine(WaitForAnimation(trigger));
    }

    private IEnumerator WaitForAnimation(string trigger)
    {
        yield return null; // Wait one frame to ensure the state updates

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length; // Get animation duration

        yield return new WaitForSeconds(animationDuration); // Wait until animation finishes 
        isTriggerActive = false;
    }
    #endregion

    #region UI Logic
    public void SetPlayerIngameUI()
    {
        playerIngameUI = GameObject.FindGameObjectWithTag("UI");
    }

    public void GamePaused()
    {
        rb.isKinematic = true;
        gamePaused = true;
    }

    public void GameResumed()
    {
        rb.isKinematic = false;
        gamePaused = false;
    }
    #endregion
   

    [Serializable]
    public struct HitBoxes
    {
        public string attackName; // Name of the attack
        public Collider[] collider; // The actual collider
    }

}

