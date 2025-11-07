using JetBrains.Annotations;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    GameManager gm;

    // Maybe temporary -- to turn off lightsource while not aiming 
    [SerializeField] GameObject lightSource;
    // Get only instance of player script 
    public static PlayerMovement player;

    // Allow inputs to affect player
    public bool playerControl = true;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    float maxSpeed = 7.5f;
    [Space(5)]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMult;
    bool canJump = true;

    [Header("Ground Check")]
    [SerializeField] LayerMask isGround;
    [SerializeField] bool grounded = true;
    [SerializeField] float groundDrag;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    [Space(15)]
    public Transform camOrientation; // Grab orientation for rotation
    public Transform aimCamOrientation; // ^Same for when aiming

    [Space(15)]
    [Header("Grab Objects")]
    // Moveable object script
    public GrabObject grab = null;
    bool moveObj;

    [Space(15)]
    [Header("Held Items")]
    // Will make static if we only ever need one 
    public GameObject item;

    [Space(15)]
    public PlayerState state;
    public enum PlayerState
    {
        walking,
        grabbing,
        light,
    }

    Rigidbody rb;
    Vector3 moveDirection;
    RaycastHit floorHit;
    public bool isAiming;
    [SerializeField] Transform yawTarget;
    [SerializeField] GameObject lightTool;

    void Awake()
    {
        player = this;
    }
    void Start()
    {
        Physics.gravity = new Vector3(0, -27f, 0);
        Cursor.lockState = CursorLockMode.Locked;

        gm = GameManager.instance;
        rb = GetComponent<Rigidbody>();
    }

    void PlayerInput()
    {
        // Get forward position from camera Y rotation
        Transform orientation = isAiming ? aimCamOrientation : camOrientation;
        orientation.localEulerAngles = new Vector3(0f, orientation.localEulerAngles.y, 0f);

        // Get keyboard input
        if (state == PlayerState.grabbing)
            // Prevent left or right movement while grabbing
            moveDirection = transform.forward * Input.GetAxisRaw("Vertical"); //(orientation.right * Input.GetAxisRaw("Horizontal") * 0.2f);
        else
            // Move in direction of camera
            moveDirection = orientation.forward * Input.GetAxisRaw("Vertical") + orientation.right * Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && canJump && grounded && state != PlayerState.grabbing)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // Wait before resetting jump
        }


        // Check if player is facing moveable object
        if (grab != null)
        {
            moveObj = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.forward, 0.6f, isGround);
            //Debug.DrawLine(transform.position, new Vector3(transform.position.x - 0.6f, transform.position.y - 0.2f, transform.position.z), Color.magenta);
        }
        else
        {
            if (moveObj) moveObj = false;
        }
        isAiming = Input.GetMouseButton(1);

        if (!item.activeInHierarchy) return;
        if (Input.GetMouseButtonDown(1)) LightSwitch(true);
        else if (Input.GetMouseButtonUp(1)) LightSwitch(false);
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Update()
    {
        if (playerControl) PlayerInput();
        GroundCheck();
        StateHandler();
    }

    void StateHandler()
    {
        // Check if player is holding left click while facing moveable object
        if (Input.GetMouseButton(0) && moveObj)
        {
            state = PlayerState.grabbing;

            moveSpeed = 2.7f; // Limit player speed while grabbing

            // THIS ALL SHOULDNT BE IN PLAYER SCRIPT BUT I'LL LEAVE FOR NOW 
            if (grab != null)
            {
                // Signals to object script to remove itself from grab variable
                grab.isGrabbed = true;

                // Set player as object parent
                grab.transform.SetParent(this.transform);

                // Make kinematic when moving backward
                grab.rb.isKinematic = (Input.GetAxisRaw("Vertical") < 0f) ? true : false;

                // Unfreezes position constraints and prevents rotation
                grab.rb.constraints = RigidbodyConstraints.FreezeRotation;

                // Make lighter so player can push object forward
                grab.rb.mass = 1.0f;
            }
        }
        else
        {
            if (grab != null)
            {
                // Unparent player and make object static again
                grab.transform.SetParent(null);
                grab.rb.isKinematic = false;
                grab.rb.mass = 50.0f;

                // Freeze everything but Y position 
                grab.rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }

            state = PlayerState.walking;

            moveSpeed = 8.0f;
        }
    }

    void Movement()
    {
        // Create move Vector from player inputs on X and Z axis
        Vector3 move = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        if (isAiming)
        {
            Vector3 forward = aimCamOrientation.forward;
            Vector3 right = aimCamOrientation.right;

            forward.y = rb.linearVelocity.y;
            right.y = 0;

            forward.Normalize();
            right.Normalize();
            move = -forward * moveDirection.x + right * moveDirection.z;
        }

        //Debug.Log(OnSlope());

        if (OnSlope() && !exitingSlope)
        {
            // Adjust speed while on slope
            rb.linearVelocity = GetSlopeMoveDirection() * moveSpeed;

            // Prevent bump effect when running upward
            if (rb.linearVelocity.y > 0f) rb.AddForce(Vector3.down * 80.0f, ForceMode.Force);
        }
        else
        {
            // Limit movement in air
            rb.linearVelocity = grounded ? move : new Vector3(move.x * airMult, rb.linearVelocity.y, move.z * airMult);
        }

        // Clamp magnitude while on ground
        if (grounded && canJump) rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        // Turn off gravity on slope
        rb.useGravity = !OnSlope();

        // Handle rotation while player is not grabbing an object or aiming lightbeam
        if (moveDirection != Vector3.zero && state != PlayerState.grabbing && !isAiming)
        {
            float angleDiff = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
            rb.angularVelocity = new Vector3(rb.angularVelocity.x, angleDiff * 0.2f, rb.angularVelocity.z);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void GroundCheck()
    {
        // Ground check 
        grounded = Physics.BoxCast(transform.position, transform.localScale * 0.25f, Vector3.down, out floorHit, transform.rotation, 1.2f, isGround);
        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 1.2f, transform.position.z), Color.magenta);

        // Handle drag
        rb.linearDamping = grounded ? groundDrag : 0;
    }

    void Jump()
    {
        exitingSlope = true;

        // Always start with Y Vel at 0
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.linearVelocity += Vector3.up * jumpForce;
    }

    void ResetJump()
    {
        // Reset jump variables
        canJump = true;
        exitingSlope = false;
    }

    bool OnSlope()
    {   //Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), Vector3.down, out slopeHit, 1.2f)
        if (Physics.BoxCast(transform.position, transform.localScale * 0.25f, Vector3.down, out slopeHit, transform.rotation, 1.2f, isGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); // Calculate slope steepness

            // Slide down if slope is too steep
            if (angle > maxSlopeAngle && angle != 0)
            {
                rb.AddForce(Vector3.down * 60.0f, ForceMode.Force);
            }

            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    void LightSwitch(bool active)
    {
        lightSource.SetActive(active);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Exit")
        {
            gm.ResetScene(); // Restart demo
        }
    }
}
