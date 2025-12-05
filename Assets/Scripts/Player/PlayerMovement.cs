using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 startPos;
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
    [SerializeField] LayerMask moveable;
    RaycastHit moveHit;
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
    [SerializeField] LineRenderer line;
    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject aura;
    public Lantern lantern;
    [SerializeField] LayerMask allLayersExceptPhase;
    public bool checkpoint;

    void Awake()
    {
        player = this;
    }
    void Start()
    {
        startPos = transform.position;
        Physics.gravity = new Vector3(0, -27f, 0);
        Cursor.lockState = CursorLockMode.Locked;

        gm = GameManager.instance;
        rb = GetComponent<Rigidbody>();

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.positionCount = 2;
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

        // Check if player is facing moveable object
        // if (grab != null)
        // {
        //     moveObj = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.forward, 0.6f, isGround);
        //     //Debug.DrawLine(transform.position, new Vector3(transform.position.x - 0.6f, transform.position.y - 0.2f, transform.position.z), Color.magenta);
        // }
        // else
        // {
        //     if (moveObj) moveObj = false;
        // }

        moveObj = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.forward, out moveHit, 0.7f, moveable);
        if (moveObj) grab = moveHit.transform.gameObject.GetComponent<GrabObject>();
        else grab = null;

        isAiming = Input.GetMouseButton(1);

        if (item == null) return;
        if (!item.activeInHierarchy) return;
        if (Input.GetMouseButtonDown(1)) LightSwitch(true);
        else if (Input.GetMouseButtonUp(1)) LightSwitch(false);
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;
        Movement();
    }

    void Update()
    {

        //Debug.Log(camOrientation.localEulerAngles);
        Teleport();
        if (playerControl) PlayerInput();
        GroundCheck();
        StateHandler();

        if (transform.position.y < -10.0f || checkpoint)
        {
            rb.isKinematic = true;
            canJump = false;
            rb.isKinematic = true; // player unaffected by physics
            playerModel.SetActive(false); // Make player invisible
            exitingSlope = true;
            aura.SetActive(false); // Turn off lightball thing
            line.enabled = false;
            transform.position = startPos;
            Invoke(nameof(ResetJump), jumpCooldown);
            if (checkpoint) checkpoint = false;
        }

    }

    void StateHandler()
    {
        // Check if player is holding left click while facing moveable object
        if (Input.GetMouseButton(0) && moveObj)
        {
            state = PlayerState.grabbing;

            moveSpeed = 2.0f; // Limit player speed while grabbing

            // Free player rotation
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // THIS ALL SHOULDNT BE IN PLAYER SCRIPT BUT I'LL LEAVE FOR NOW 
            if (grab != null)
            {
                // if(grab.gameObject.GetComponent<FixedJoint>() == null)
                // {
                //     FixedJoint newJoint;
                //     newJoint.
                //     grab.gameObject.AddComponent<FixedJoint>();
                // }

                // Make kinematic when moving backward
                grab.rb.isKinematic = (Input.GetAxisRaw("Vertical") < 0f) ? true : false;

                // Unfreezes position constraints and prevents rotation
                grab.rb.constraints = RigidbodyConstraints.FreezeRotation;

                // Set player as object parent
                grab.transform.SetParent(this.transform);

                // Make lighter so player can push object forward
                grab.rb.mass = 0.5f;

                // Signals to object script to remove itself from grab variable
                grab.isGrabbed = true;
            }
        }
        else
        {
            if (grab != null)
            {
                // Unparent player and make object static again
                grab.rb.isKinematic = true;
                grab.rb.mass = 50.0f;

                // Freeze everything but Y position on object
                grab.rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                // Freeze everything but Y rotation on player 
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

                grab.transform.SetParent(null);
            }

            state = PlayerState.walking;

            moveSpeed = 5.5f;
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
        }

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
            //rb.linearVelocity = grounded ? move : new Vector3(move.x * airMult, rb.linearVelocity.y, move.z * airMult);
            Vector3 v = grounded ? move : new Vector3(move.x * airMult, rb.linearVelocity.y, move.z * airMult);
            rb.AddForce(v - rb.linearVelocity, ForceMode.VelocityChange);
        }

        // Clamp magnitude while on ground
        //if (grounded && canJump) rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        // Turn off gravity on slope
        rb.useGravity = !OnSlope();

        // Handle rotation while player is not grabbing an object or aiming lightbeam
        // if (isAiming)
        // {
        //     //float angleDiff = Vector3.SignedAngle(transform.forward, camOrientation.forward, Vector3.up);
        //     //Debug.Log(angleDiff);
        //     //rb.angularVelocity = new Vector3(rb.angularVelocity.x, angleDiff * 0.2f, rb.angularVelocity.z);
        // }
        // else 

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

    void Teleport()
    {
        if (!isAiming)
        {
            // Make sure target position isn't inside of something
            // Ignores collider
            bool clear = !Physics.Raycast(transform.position, transform.forward, 3.1f, allLayersExceptPhase, QueryTriggerInteraction.Ignore);

            // Draw line for debug
            // Eventually will switch to raycast or something
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + transform.forward * 3.0f);

            // Check for jump
            if (clear && Input.GetKeyDown(KeyCode.Space) && grounded && canJump)
            {
                canJump = false;
                rb.isKinematic = true; // player unaffected by physics
                playerModel.SetActive(false); // Make player invisible
                exitingSlope = true;
                aura.SetActive(false); // Turn off lightball thing
                StartCoroutine(FlashTeleport(line.GetPosition(1))); // Lerp player to position
                //transform.position = new Vector3(line.GetPosition(1).x, transform.position.y, line.GetPosition(1).z);
                line.enabled = false;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
        else
        {
            // Comments above
            bool clear = !Physics.Raycast(transform.position, aimCamOrientation.forward, 4.1f, allLayersExceptPhase, QueryTriggerInteraction.Ignore);
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + aimCamOrientation.forward * 4.0f);

            if (clear && Input.GetKeyDown(KeyCode.Space) && grounded && canJump)
            {
                canJump = false;
                rb.isKinematic = true;
                playerModel.SetActive(false);
                exitingSlope = true;
                aura.SetActive(false);
                float y = line.GetPosition(1).y <= transform.position.y ? transform.position.y : line.GetPosition(1).y;
                StartCoroutine(FlashTeleport(new Vector3(line.GetPosition(1).x, y, line.GetPosition(1).z)));
                //transform.position = new Vector3(line.GetPosition(1).x, y, line.GetPosition(1).z);
                line.enabled = false;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
    }

    // Thanks, Josh!
    IEnumerator FlashTeleport(Vector3 target)
    {
        Vector3 start = transform.position;
        Vector3 endPos = target;

        float elapsed = 0f;
        float duration = Vector3.Distance(start, endPos) / 20f;

        // lerp to target
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(new Vector3(start.x, transform.position.y, start.z), endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap position
        transform.position = endPos;
    }

    void Jump()
    {
        exitingSlope = true;

        // Always start with Y Vel at 0
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //rb.linearVelocity += Vector3.forward * jumpForce;
    }

    void ResetJump()
    {
        // Reset jump variables
        canJump = true;
        exitingSlope = false;
        rb.isKinematic = false;
        line.enabled = true;
        playerModel.SetActive(true);
        aura.SetActive(true);
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
            //gm.ResetScene(); // Restart demo
        }
    }
}
