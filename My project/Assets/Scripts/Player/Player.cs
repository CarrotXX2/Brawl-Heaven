using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public enum PlayerState
{
    Grounded,
    InAir,
    Launched,
    LedgeGrabbing,
    Blocking,
}


public class Player : MonoBehaviour
{
    public PlayerState currentState = PlayerState.Grounded;

    [Header("Player in game Stats")] 
    private float totalDamageTaken;
    private int stocks;

    [Header("Movement Stats")] 
    private float moveInput; // Float that holds the value of the input manager (-1 = left, 0 = neutral, 1 = right)
    
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
    
    [Header("Camera Control")]
    [SerializeField] private float cameraFollowWeight;
    [SerializeField] private float cameraFollowRadius;
    
    [Header("Component references")]
    [SerializeField] private Rigidbody rb;
  
    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
         ResetJumps();
    }

    private void Start()
    { 
        // probably need to fully change camera behaviour but will do that at home
        ComponentReferenceHolder.Instance.cmtg.AddMember(gameObject.transform, cameraFollowWeight,cameraFollowRadius ); // Camera setup
    }

    private void Update() 
    {
        IsGrounded();
            /*
            switch (currentState)
            {
                case PlayerState.Standing:
                    PerformStandingAction();
                    break;
                case PlayerState.Walking:
                    PerformWalkingAction();
                    break;
                case PlayerState.Running:
                    PerformRunningAction();
                    break;
                case PlayerState.Attacking:
                    PerformAttackingAction();
                    break;
                case PlayerState.InAir:

                    break;
                case PlayerState.Launched:
                    CalculateLaunch();
                    break;
                case PlayerState.LedgeGrabbing:
                    PerformLedgeGrabbingAction();
                    break;
            }*/ 
    } 

    private void FixedUpdate()  // Use FixedUpdate for physics
    {
        Move();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (currentState != PlayerState.Grounded) // Apply gravity only if not grounded
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }
    private void Move()
    {
        switch (currentState)
        {
            case PlayerState.Grounded:
                float speed = isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
                rb.velocity = new Vector3(moveInput * speed, rb.velocity.y, 0);  // Preserve vertical velocity
                
                break;
            
            case PlayerState.Launched:
                // movement logic when launched 
                
                break;
            case PlayerState.InAir:
                // movement logic when in air
                
                break;
            
        }
      
    }
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();  // Get X-axis input

        bool isMovingNow = Mathf.Abs(moveInput) > deadZoneTreshold;  // Consider movement if input is significant

        if (isMovingNow && !wasMovingLastFrame)  // Detect "new" movement input
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            if (timeSinceLastTap <= doubleTapThreshold && currentState == PlayerState.Grounded)
            {
                isSprinting = true;
                Dash(moveInput);
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
        if (jumpsLeft > 0 && ctx.started)
        {
            jumpsLeft--;

            currentState = PlayerState.InAir;
            
            rb.velocity = new Vector3(rb.velocity.y, jumpForce); // Apply vertical jump force
        }
    }

    private void Dash(float direction) // Dash Into the direction of the last double pressed direction
    {   
        Vector3 dashDirection = new Vector3(direction, 0, 0);
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
    }

    private void LedgeGrab()
    {
        if (rb.velocity.y < 0 && currentState != PlayerState.LedgeGrabbing)
        {
            Vector3 lineDownStart = (transform.position + Vector3.up * 1.5f);
            Vector3 lineDownEnd = (transform.position + Vector3.down * 0.7f);
            RaycastHit downHit; 
            Physics.Linecast(lineDownStart, lineDownEnd, out downHit, groundLayer);

            if (downHit.collider != null)
            {
                Vector3 lineForwardStart = new Vector3(transform.position.x, downHit.point.y, transform.position.z); 
                Vector3 lineForwardEnd= new Vector3(transform.position.x, downHit.point.y, transform.position.z) + transform.forward; 
                RaycastHit forwardHit;
                Physics.Linecast(lineForwardStart, lineForwardEnd, out forwardHit, groundLayer);

                if (forwardHit.collider != null)
                {
                    rb.velocity = Vector3.zero;
                    
                    currentState = PlayerState.LedgeGrabbing;
                    
                    Vector3 hangPosition = new Vector3(forwardHit.point.x, downHit.point.y, forwardHit.point.z);
                    Vector3 targetOffset = transform.forward * -0.1f + transform.up * -1f;
                    
                    hangPosition += targetOffset;
                    transform.position = hangPosition;
                }
            }
        }
    }
    public void OnDamageTaken(float damage, float knockBack)
    {
        totalDamageTaken += damage;
        
        // KnockBack logic here
    }

    private void OnDeath()
    {
        stocks--;
        
        // play particle and death sound
    }
    private void OnCollisionEnter(Collision collision) // Having both a ray and collision detection helps for easier walljump implementation with certain objects
    {
        if (collision.gameObject.transform.CompareTag("Wall Jump")) 
        {
            ResetJumps();
        }
    }
    
    private void IsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer))
        {
            currentState = PlayerState.Grounded;
        }
    }
    
    private void ResetJumps()
    {
        jumpsLeft = maxJumps;
    }
    private void PerformStandingAction()
    {
        
    }
    
    private void PerformWalkingAction()
    {
        
    }

    private void PerformRunningAction()
    {
        
    }

    private void PerformAttackingAction()
    {
        
    }

   

    private void PerformLedgeGrabbingAction()
    {
        
    }

    private void CalculateLaunch()
    {
        
    }
 
    private void ChangeUI()
    {
        
    }
}

