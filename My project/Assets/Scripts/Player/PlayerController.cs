using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public enum MovementState
{
    Grounded,
    InAir,
    Launched,
    LedgeGrabbing,
    
}

public enum CombatState
{
    Neutral,
    Attacking,
    Blocking,
    Hit,
}

public class PlayerController : MonoBehaviour
{
    public MovementState movementState = MovementState.Grounded;
    public CombatState combatState = CombatState.Neutral;

    [Header("Player in game Stats")] 
    [SerializeField] private float totalDamageTaken;
    private int stocks;

    [Header("Movement Stats")] 
    private Vector2 moveInput; // Float that holds the value of the input manager (-1 = left, 0 = neutral, 1 = right)
    
    [SerializeField] private float movementSpeed; // Base movementspeed
    [SerializeField] private float sprintMultiplier; // Multiplies the base movementspeed for a sprint speed

    [SerializeField] private float gravityForce;
    
    [SerializeField] private float jumpForce; // Jump strength
    [SerializeField] private int maxJumps;
    private int jumpsLeft; // Amount of jumps left
    
    [SerializeField] private float dashForce; // Dash Strength

    [Header("Sprint logic")] 
    [SerializeField] private float doubleTapThreshold; // Time window you have for multi tapping and to start sprinting
    [SerializeField] private float deadZoneTreshold; // The value that moveInput needs to be to count as a movement input
    [SerializeField] private float lastTapTime; // Stores the time of the last movement input to detect double taps
    [SerializeField] private bool isSprinting = false;
    [SerializeField] private bool wasMovingLastFrame = false; // Flag to check if you moved last frame
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer; //is the layer called ground
    [SerializeField] private float groundCheckDistance;
    
    [Header("Attack Logic")]
    [SerializeField] private List<HitBoxes> lightHitboxList = new List<HitBoxes>(); // List of hitboxes for light attack
    [SerializeField] private List<HitBoxes> heavyHitboxList = new List<HitBoxes>(); // List of hitboxes for heavy attakcs
    
    private Dictionary<string, Collider[]> lightHitboxes = new Dictionary <string, Collider[]> (); // Dictionary to easily call the light hitboxes
    private Dictionary<string, Collider[]> heavyHitboxes = new Dictionary <string, Collider[]> (); // Dictionary to easily call the heavy hitboxes

    [SerializeField] private List<AttackData> attackData = new List<AttackData>();
    
    [Header("Blocking Logic")]
    [SerializeField] private float totalBlockingTime;
    [SerializeField] private float currentBlockingTime;
    [SerializeField] private float blockRechargeTime;
    [SerializeField] private float shieldReductionFactor; // Factor used for calculating the time that needs to be reduced when hit by an attack

    private bool coroutineRunning = false;
    [Header("Camera Control")]
    [SerializeField] private float cameraFollowWeight;
    [SerializeField] private float cameraFollowRadius;

    [Header("Lerp Smoothing")] 
    [SerializeField] private float rotationSpeed;
    
    [Header("Component references")]
    [SerializeField] private Rigidbody rb;
  
    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
         ResetJumps();
         
         foreach (var pair in lightHitboxList)
         {
             if (!lightHitboxes.ContainsKey(pair.attackName))
             {
                lightHitboxes[pair.attackName] = pair.collider;
             }
         }
         foreach (var pair in heavyHitboxList)
         {
             if (!heavyHitboxes.ContainsKey(pair.attackName))
             {
                 heavyHitboxes[pair.attackName] = pair.collider;
             }
         }
    }
    
    private void Update() 
    {
        IsGrounded();
        LedgeGrab();
        Blocking();
    }
    
    private void FixedUpdate()  // Use FixedUpdate for physics
    {
        Move();
        ApplyGravity();
    }

    #region Movement
    
    private void ApplyGravity()
    {
        if (movementState != MovementState.Grounded && movementState != MovementState.LedgeGrabbing) // Apply gravity only if not grounded
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }
    private void Move()
    {
        switch (movementState)
        {
            default:
                float speed = isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
                rb.velocity = new Vector3(moveInput.x * speed, rb.velocity.y, 0);
    
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
        moveInput = ctx.ReadValue<Vector2>();  // Get X-axis input
        
       //print("moveInput: " + moveInput.x);
        bool isMovingNow = Mathf.Abs(moveInput.x) > deadZoneTreshold;  // Consider movement if input is significant

        if (isMovingNow && !wasMovingLastFrame)  // Detect "new" movement input
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            if (timeSinceLastTap <= doubleTapThreshold && movementState == MovementState.Grounded)
            {
                isSprinting = true;
                Dash(moveInput.x);
            }
            else
            {
                isSprinting = false;
            }

            lastTapTime = Time.time;  // Update last tap time
        }

        wasMovingLastFrame = isMovingNow;  // Store movement state
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        switch (movementState)
        {
            default:
                if (jumpsLeft > 0 && ctx.started)
                {
                    jumpsLeft--;
            
                    rb.velocity = new Vector3(rb.velocity.y, jumpForce); // Apply vertical jump force
                }
                break;
            case MovementState.LedgeGrabbing:
                // LedgeGrab logic
                break;
        }
    }

    private void Dash(float direction) // Dash Into the direction of the last double pressed direction
    {   
        Vector3 dashDirection = new Vector3(direction, 0, 0);
       // rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
    }

    private void LedgeGrab()
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
        Vector3 lineDownStart = transform.position + Vector3.up * 1.2f + direction;
        Vector3 lineDownEnd = transform.position + Vector3.down * 0.4f + direction;
    
        Debug.DrawLine(lineDownStart, lineDownEnd, Color.red, 2f);

        if (!Physics.Linecast(lineDownStart, lineDownEnd, out RaycastHit downHit, groundLayer))
            return;

        Vector3 lineForwardStart = new Vector3(transform.position.x, downHit.point.y, transform.position.z);
        Vector3 lineForwardEnd = lineForwardStart + direction;
    
        Debug.DrawLine(lineForwardStart, lineForwardEnd, Color.green, 2f);

        if (!Physics.Linecast(lineForwardStart, lineForwardEnd, out RaycastHit forwardHit, groundLayer))
            return;

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    
        movementState = MovementState.LedgeGrabbing;
    
        Vector3 hangPosition = new Vector3(forwardHit.point.x, downHit.point.y, forwardHit.point.z);
        Vector3 targetOffset = -direction * 0.1f + Vector3.up * -0.5f;
    
        transform.position = hangPosition + targetOffset;
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
        bool grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        if (grounded)
        {
            movementState = MovementState.Grounded;
        }
        else if (movementState != MovementState.LedgeGrabbing)
        {
            movementState = MovementState.InAir;
        }
      
        return grounded;
    }
    
    private void ResetJumps()
    {
        jumpsLeft = maxJumps;
    }
    #endregion

    #region Combat Logic

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        
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
        if (!ctx.started) return;
        
        string direction = GetAttackDirection(moveInput);
        string moveName = $"{movementState.ToString()} {direction}";
        
        if (!heavyHitboxes.TryGetValue(moveName, out Collider[] colliders))
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

    private IEnumerator PerformAttack(AttackData attackData, Collider[] colliders)
    {   
        combatState = CombatState.Attacking;

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
    }
    
    private void DetectHits(Collider attackCollider, AttackData attackData)
    {
        // Check for overlaps with other colliders/players
        Collider[] hitObjects = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents, attackCollider.transform.rotation);

        foreach (var hit in hitObjects)
        {
            PlayerController player = hit.transform.GetComponent<PlayerController>();
            player.OnHit(attackData.damage, attackData.knockback);
        }
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

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (ctx.started) // When button is pressed
        {
            Debug.Log("Blocking started");
            combatState = CombatState.Blocking;
        }

        if (ctx.canceled)
        {
            Debug.Log("Blocking stopped");
            combatState = CombatState.Neutral;
        }
    }
    
    private void Blocking()
    {
        if (combatState != CombatState.Blocking)
        {
            if (currentBlockingTime < totalBlockingTime) // Recharge block
            {
                currentBlockingTime = Mathf.MoveTowards(currentBlockingTime, totalBlockingTime, Time.deltaTime * blockRechargeTime);
                currentBlockingTime = Mathf.Clamp(currentBlockingTime, 0 , totalBlockingTime);
            }
        }
        else // If blocking is true
        {
            currentBlockingTime -= Time.deltaTime;
            currentBlockingTime = Mathf.Clamp(currentBlockingTime, 0, totalBlockingTime);
        }
    }

   /* private IEnumerator RechargeBlock()
    {
        coroutineRunning = true;

        while (currentBlockingTime < totalBlockingTime)
        {
            currentBlockingTime = Mathf.MoveTowards(currentBlockingTime, totalBlockingTime, Time.deltaTime * blockRechargeTime);
            currentBlockingTime = Mathf.Clamp(currentBlockingTime, 0 , totalBlockingTime);
            yield return null;
        }
        
        coroutineRunning = false;
    }
    */
    public void OnHit(float damage, float knockBack)
    {
        switch (combatState)
        {
            default:
                totalDamageTaken += damage;
                print($"Took {damage} damage");
                    
                // KnockBack logic here
                
                break;
            case CombatState.Blocking:
                currentBlockingTime -= damage * shieldReductionFactor;
                
                break;
        }
    }

    private void OnDeath()
    {
        stocks--;
        if (stocks > 0)
        {
            // respawn
        }
        
        // play particle and death sound
    }

    #endregion
    private void ChangeUI()
    {
        
    }
    
    [Serializable]
    public struct HitBoxes 
    {
        public string attackName;         // Name of the attack
        public Collider[] collider;   // The actual collider
    }
}

