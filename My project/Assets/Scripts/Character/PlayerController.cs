using System;
using System.Collections;
using System.Collections.Generic;
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
    public MovementState movementState = MovementState.Grounded;
    public CombatState combatState = CombatState.Neutral;

    [Header("Player in game Stats")] 
    [SerializeField] private float totalDamageTaken;

    [SerializeField] private int stocks;

    [Header("Movement Stats")]
    [SerializeField] protected Vector2 moveInput; // Float that holds the value of the input manager (-1 = left, 0 = neutral, 1 = right)

    [SerializeField] private float movementSpeed; // Base movementspeed
    [SerializeField] private float sprintMultiplier; // Multiplies the base movementspeed for a sprint speed

    [SerializeField] private float gravityForce; // Gravity strength

    [SerializeField] private float jumpForce; // Jump strength
    [SerializeField] private int maxJumps; // Total jumps player can have in total
    [SerializeField] private int jumpsLeft; // Amount of jumps left

    [Header("Dash Stats")] [SerializeField]
    private float dashForce; // Dash Strength

    [SerializeField]
    private float dashTime; // Duration of the dash PLayer doesnt have control of the character for this amount

    [Header("Sprint logic")] [SerializeField]
    private float doubleTapThreshold; // Time window you have for multi tapping and to start sprinting

    [SerializeField]
    private float deadZoneTreshold; // The value that moveInput needs to be to count as a movement input

    [SerializeField] private float lastTapTime; // Stores the time of the last movement input to detect double taps
    private bool isSprinting;
    private bool wasMovingLastFrame; // Flag to check if you moved last frame

    [Header("Ground Check")] [SerializeField]
    private LayerMask groundCheckLayer; // The ground layer, used for checking if the player is grounded 

    [SerializeField]
    private float groundCheckDistance; // Half of the characters height + a little bit for ground check raycast
    
    [Header("LedgeGrab Check")]
    [SerializeField] private LayerMask ledgeLayer;
    [SerializeField] private Transform lineCastTransform;
    
    [Header("Launch Logic")] [SerializeField]
    private int weight; // Weight determines the players knockback, heavier weight less knockback

    [SerializeField] private float minLaunchForce;

    [Header("Attack Logic")] [SerializeField]
    private LayerMask player;

    [SerializeField]
    private List<HitBoxes> lightHitboxList = new List<HitBoxes>(); // List of hitboxes for light attacks

    [SerializeField]
    private List<HitBoxes> heavyHitboxList = new List<HitBoxes>(); // List of hitboxes for heavy attakcs

    private Dictionary<string, Collider[]>
        lightHitboxes = new Dictionary<string, Collider[]>(); // Dictionary to easily call the light hitboxes

    private Dictionary<string, Collider[]>
        heavyHitboxes = new Dictionary<string, Collider[]>(); // Dictionary to easily call the heavy hitboxes

    [SerializeField] private List<AttackData> attackData = new List<AttackData>();

    private bool performingAttack; // Bool to keep track if the coroutine is ongoing or not
    private bool chargeAttack = false; // Bool to check if 
    [Header("Blocking Logic")] [SerializeField]
    private float totalBlockingTime;

    [SerializeField] private float currentBlockingTime;
    [SerializeField] private float blockRechargeTime;

    [SerializeField]
    private float
        shieldReductionFactor; // Factor used for calculating the time that needs to be reduced when hit by an attack

    [Header("Respawn Logic")]
    private bool
        invincible = false; // When Respawning you become Invincible for a few seconds to get back into the fight 
    // ^^ implement this

    [SerializeField] private float respawnTime;
    public bool touchedDeathZone;

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
        IsGrounded();
        LedgeGrab();
        Blocking();
        
        AnimationStates(); // Manages the animations
    }

  
    private void FixedUpdate() // Use FixedUpdate for physics
    {
        if (!rb.isKinematic)
        { 
            ApplyGravity();
            Move();
        }
   
    }

    #region Movement

    private void ApplyGravity()
    {
        if (movementState == MovementState.InAir || movementState == MovementState.Launched) // Apply gravity only if In air or launched
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }

    private void Move()
    {
        if (!CanPerformAction()) return;
        
        switch (movementState)
        {
            default:
                Vector3 velocity = rb.velocity;
                velocity.x = moveInput.x * (isSprinting ? movementSpeed * sprintMultiplier : movementSpeed);
                rb.velocity = velocity;

                if (Mathf.Abs(moveInput.x) > deadZoneTreshold)
                {
                    transform.rotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0, 0));
                }

                break;
            case MovementState.LedgeGrabbing:
                // Cant move/rotate while ledge grabbing

                break;

            case MovementState.Launched:
                // movement logic when launched 

                break;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanPerformAction())
        {
            moveInput = Vector2.zero; 
            return;
        }
        
        moveInput = ctx.ReadValue<Vector2>(); // Get X-axis input

        //print("moveInput: " + moveInput.x);
        bool isMovingNow = Mathf.Abs(moveInput.x) > deadZoneTreshold; // Consider movement if input is significant

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
        switch (movementState)
        {
            default:
                if (jumpsLeft > 0 && ctx.performed)
                {
                    jumpsLeft--;
                    
                    SetTriggerAnimation("Jump");
                    
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                }

                break;
            case MovementState.LedgeGrabbing:
                SetTriggerAnimation("Jump");
                rb.isKinematic = false;
                movementState = MovementState.InAir;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                
                break;
            
            case MovementState.Launched:
                
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
        rb.velocity = dashVelocity;
        
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
        Vector3 lineDownStart = lineCastTransform.position + Vector3.up * 1.2f + direction;
        Vector3 lineDownEnd = lineCastTransform.position + Vector3.up * 1f + direction;

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
        Vector3 targetOffset = -direction * 0.1f + Vector3.up * -0.5f;

        Vector3 lookDirection = (forwardHit.point - transform.position).normalized;
        lookDirection.y = 0; // Keep only horizontal rotation

        // Apply rotation only on the Y-axis
        if (lookDirection != Vector3.zero) // Prevents errors when direction is (0,0,0)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        transform.position = hangPosition + targetOffset;
    }
    private bool HasState(MovementState state) => (movementState & state) != 0;
    private bool HasState(CombatState state) => (combatState & state) != 0;

    private bool CanPerformAction()
    {
        return !(HasState(MovementState.Dashing) || HasState(CombatState.HitStun) ||
                 HasState(CombatState.Attacking) || HasState(CombatState.Blocking) || touchedDeathZone);
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

        bool grounded = Physics.Raycast(lineCastTransform.position, Vector3.down, groundCheckDistance, groundCheckLayer);
        
        if (grounded)
        {
            movementState = MovementState.Grounded;
            rb.drag = groundedDrag;
        }
        else if (movementState != MovementState.LedgeGrabbing)
        {
            movementState = MovementState.InAir;
            rb.drag = inAirDrag;
        }

        return true;
    }

    private void ResetJumps()
    {
        jumpsLeft = maxJumps;
    }

    #endregion

    #region Combat Logic

    #region Attacking And Hit logic

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !CanPerformAction() || !CanAttack()) return;
        
        string direction = GetAttackDirection(moveInput);
        string moveName = $"{movementState.ToString()} {direction}";

        if (!lightHitboxes.TryGetValue(moveName, out Collider[] colliders))
        {
            Debug.LogWarning($"{moveName} does not exist");
            return;
        }

        AttackData currentAttack = attackData.Find(attack => attack.attackName == moveName);

        if (currentAttack != null)
        {
            StartCoroutine(PerformAttack(currentAttack, colliders));
        }
        else
        {
            Debug.LogWarning($"No attack data found for {moveName}");
        }

        print(moveName);

    }

    public void OnHeavytAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !CanPerformAction() || !CanAttack()) return;
        
        string direction = GetAttackDirection(moveInput);
        string moveName = $"{movementState.ToString()} {direction}";

        if (!heavyHitboxes.TryGetValue(moveName, out Collider[] colliders))
        {
            Debug.LogWarning($"{moveName} does not exist");
            return;
        }
        
        AttackData currentAttack = attackData.Find(attack => attack.attackName == moveName);
        if (currentAttack.chargeAttack)
        {
            SetAnimation(currentAttack.chargeAnimation.name); 
            chargeAttack = true;
            StartCoroutine(ChargeHeavyAttack());
            // perform charge logic here 
        }
        else
        {
            StartCoroutine(PerformAttack(currentAttack, colliders));
        }
        
        if (ctx.canceled && currentAttack.chargeAttack)
        {
            StartCoroutine(PerformChargeAttack(currentAttack, 5f,colliders));
        }
        
        /*
        if (currentAttack != null)
        {
            StartCoroutine(PerformAttack(currentAttack, colliders));
        }
        else
        {
            Debug.LogWarning($"No attack data found for {moveName}");
        }*/

        print(moveName);

    }

    private IEnumerator ChargeHeavyAttack(AttackData a)
    {
        float chargeDamage = Mathf.MoveTowards()
        return null;
    }
    
    private IEnumerator PerformAttack(AttackData attackData, Collider[] colliders) 
    {
        combatState = CombatState.Attacking;
        
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
            DetectHits(collider, attackData);
        }

        yield return new WaitForSeconds(attackData.activeTime);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        yield return new WaitForSeconds(attackData.moveDuration);

        combatState = CombatState.Neutral;
        performingAttack = false;
    }
    private IEnumerator PerformChargeAttack(AttackData attackData, float chargedDamage ,Collider[] colliders) 
    {
        combatState = CombatState.Attacking;
        
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
            DetectHits(collider, attackData);
        }

        yield return new WaitForSeconds(attackData.activeTime);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        yield return new WaitForSeconds(attackData.moveDuration);

        combatState = CombatState.Neutral;
        performingAttack = false;
    }
    private void DetectHits(Collider attackCollider, AttackData attackData)
    {
        // Corrected size calculation to ensure full detection
        Vector3 boxSize = attackCollider.bounds.size; 

        Collider[] hitObjects = Physics.OverlapBox(
            attackCollider.bounds.center,
            boxSize,
            attackCollider.transform.rotation,
            player
        );

        foreach (var hit in hitObjects)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            
            // Ignore self
            if (hit.gameObject == gameObject) continue;
            
            if (damageable != null)
            {
                damageable.TakeDamage(attackData, transform);
            }
        }
    }


    public void TakeDamage(AttackData attackData, Transform enemyTransform)
    {
        switch (combatState)
        {
            default:
                totalDamageTaken += attackData.damage;
                print($"Took {attackData.damage} damage");

                TakeKB(attackData, enemyTransform); // Apply's a knockback if the move has knockback property's
                StartCoroutine(ApplyHitStun(attackData)); // Apply's hitstun if the move has hitstun property's

                break;
            case CombatState.Blocking:
                currentBlockingTime -= attackData.damage * shieldReductionFactor;

                break;
        }
    }

    public void TakeKB(AttackData attackData, Transform kbSource)
    {
        if (attackData.knockback == 0) return;

        float knockback = (((totalDamageTaken / 100) * attackData.knockback * (200 / (weight + 100)) + minLaunchForce));

        // Get knockback direction (from attacker to target)
        Vector3 hitDirection = (transform.position - kbSource.position).normalized;

        Vector2 launchForce = attackData.hitDirection * knockback;
        rb.AddForce(launchForce, ForceMode.Impulse);
    }

    private IEnumerator ApplyHitStun(AttackData attackData) // If hitstun is 0 the coroutine simply sets combatState to neutral and stops all ongoing attacks because you got hit
    {
        combatState = CombatState.HitStun;
        if (performingAttack && combatState != CombatState.Unstoppable)
        {
            StopCoroutine(PerformAttack(null, null));
        }

        yield return null;

        yield return new WaitForSeconds(attackData.hitStun);

        combatState = CombatState.Neutral;
    }
    
    private bool CanAttack()
    {
        if (movementState == MovementState.Grounded && combatState == CombatState.Neutral ||
            movementState == MovementState.InAir && combatState == CombatState.Neutral)
        {
            return true;
        }

        return false;
    }

    private string GetAttackDirection(Vector2 inputDir)
    {
        if (inputDir.magnitude < deadZoneTreshold)
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

    public virtual void OnUltimateCast(InputAction.CallbackContext ctx)
    {
        // Every Character (if we add more) Has their own ultimate 
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
        }

        if (ctx.canceled && combatState == CombatState.Blocking)
        {
            Debug.Log("Blocking stopped");
            combatState = CombatState.Neutral;
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
            }
        }
    }

    #endregion

    #region Death

    private void OnTriggerEnter(Collider other) // When you hit the outer borders the player should die 
    {
        if (other.CompareTag("Border"))
        {
            touchedDeathZone = true;
            OnStockLost();
        }
    }
    private void OnStockLost()
    {
        // play particle and death sound
        stocks--;
        rb.isKinematic = true;
        
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
        StartCoroutine(GameplayManager.Instance.RespawnPlayer(gameObject));
    }

    private void Die()
    {
        GameplayManager.Instance.PlayerDeath(gameObject);
    }
    #endregion


    #endregion

    #region Animator

    private void AnimationStates() // Sets animation bools based on state
    {
        if (chargeAttack) return;
     
        switch (combatState)
        {
            case CombatState.Blocking:
                
                
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
                else if (Mathf.Abs(moveInput.x) > deadZoneTreshold)
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
    
    private void ChangeUI()
    {
       
    }

    [Serializable]
    public struct HitBoxes
    {
        public string attackName; // Name of the attack
        public Collider[] collider; // The actual collider
    }

}

