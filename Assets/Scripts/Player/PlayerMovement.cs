using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 startPos;
    GameManager gm;

    // Maybe temporary -- to turn off lightsource while not aiming 
    [SerializeField] GameObject lightSource;
    // Get only instance of player script 
    public PlayerMovement player;

    // Allow inputs to affect player
    public bool playerControl = true;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    float maxFallSpeed = 20.0f;
    [Space(5)]

    [SerializeField] float teleportForce;
    [SerializeField] float teleportCooldown;
    [SerializeField] float airMult;
    bool canTeleport = true;
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
    [Header("Held Items")]
    // Will make static if we only ever need one 
    public GameObject item;
    [Space(15)]
    [Header("Ladder Movement")]
    public LadderMovement ladder;

    [Space(15)]
    public PlayerState state;
    public enum PlayerState
    {
        walking,
        ladder,
        light,
    }
    public Rigidbody rb;
    public Vector3 moveDirection;
    RaycastHit floorHit;
    public bool isAiming;
    public Transform yawTarget;
    [SerializeField] GameObject lightTool;
    [SerializeField] LineRenderer line;
    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject aura;
    [Space]
    public Lantern lantern;
    public bool inLantern;
    [Space]
    public Projector projector;
    [Space]
    [SerializeField] LayerMask allLayersExceptPhase;
    public bool checkpoint;
    bool moveH, moveV;
    bool canMove = true;
    [SerializeField] List<KeyCode> movementKeys;
    KeyCode currentMoveKey;
    SunWheelController sunWheel;
    [SerializeField] Animator anim;
    bool test;
    bool teleportFromLadder;

    void Start()
    {
        if (item != null) item.SetActive(false);
        startPos = transform.position;
        Physics.gravity = new Vector3(0, -27f, 0);
        Cursor.lockState = CursorLockMode.Locked;

        gm = GameManager.Instance;
        rb = GetComponent<Rigidbody>();
        sunWheel = SunWheelController.Instance;

        //line.material = new Material(Shader.Find("Sprites/Default"));
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.positionCount = 2;
    }

    void PlayerInput()
    {
        // Get forward position from camera Y rotation
        Transform orientation = isAiming ? aimCamOrientation : camOrientation;
        orientation.localEulerAngles = new Vector3(0f, orientation.localEulerAngles.y, 0f);

        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Move in direction of camera's flat orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Ensure diagonal movement isn't faster
        if (moveDirection.magnitude > 1) moveDirection.Normalize();

        isAiming = Input.GetMouseButton(1) || test;
        LightSwitch(isAiming);

        /* if (item == null) return;
         if (!item.activeInHierarchy) return;

         if (Input.GetMouseButtonDown(1))
         {
             LightSwitch(true);
         }
         else if (Input.GetMouseButtonUp(1))
         {
             LightSwitch(false);

             // Remove fire VFX if player stops aiming whil burning
             // TEMPORARY
             if (GameObject.FindGameObjectWithTag("Fire") != null)
             {
                 transform.GetChild(1).GetChild(0).GetComponent<LightReflection>().DestoryFireVFX();
             }
         } */

    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;
        Movement();
    }
    private void OnEnable()
    {
        PromptTrigger.OnFPVToggle += HandleFPVChange;
    }

    private void OnDisable()
    {
        PromptTrigger.OnFPVToggle -= HandleFPVChange;
    }

    private void HandleFPVChange(bool isFPVActive)
    {
        if (!this.enabled) return;

        canMove = !isFPVActive;
        float focusAnim = canMove ? 0f : 1f;
        anim.SetFloat("Beam", focusAnim);

        if (isFPVActive)
        {
            test = true;
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
        else
        {
            test = false;
        }
    }

    void Update()
    {
        if (!canMove) return;

        //Debug.Log(camOrientation.localEulerAngles);
        Teleport();
        if (playerControl) PlayerInput();
        GroundCheck();
        StateHandler();
        SunWheelHandler();

        if (transform.position.y < -10.0f || checkpoint)
        {
            rb.isKinematic = true;
            canTeleport = false;
            rb.isKinematic = true; // player unaffected by physics
            playerModel.SetActive(false); // Make player invisible
            exitingSlope = true;
            aura.SetActive(false); // Turn off lightball thing
            line.enabled = false;
            transform.position = startPos;
            Invoke(nameof(ResetTeleport), teleportCooldown);
            if (checkpoint) checkpoint = false;
        }

        // Animation 
        if ((rb.linearVelocity != Vector3.zero && grounded) || ladder != null)
        {
            anim.SetFloat("Walk", 1f);
            AudioLibrary.Instance.PlaySound(Sfx.Walk);
        }
        else
        {
            anim.SetFloat("Walk", 0f);
        }

        if (GetComponent<LanternTravel>().isTraveling)
        {
            anim.SetFloat("Fly", 1f);
        }
        else
        {
            anim.SetFloat("Fly", 0f);
        }
    }

    void StateHandler()
    {
        moveSpeed = isAiming ? 0.7f : 5.5f;
    }

    void Movement()
    {
        // Create move Vector from player inputs on X and Z axis
        Vector3 move = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);

        if (OnSlope() && !exitingSlope)
        {
            // Adjust speed while on slope
            rb.linearVelocity = GetSlopeMoveDirection() * moveSpeed;

            // Prevent bump effect when running upward
            if (moveDirection == Vector3.zero) rb.linearVelocity = Vector3.zero;
            else if (rb.linearVelocity.y > 0f) rb.AddForce(Vector3.down * 80.0f, ForceMode.Force);
        }
        else
        {
            // Limit movement in air
            //rb.linearVelocity = grounded ? move : new Vector3(move.x * airMult, rb.linearVelocity.y, move.z * airMult);
            Vector3 v = grounded ? move : new Vector3(move.x * airMult, rb.linearVelocity.y, move.z * airMult);
            rb.AddForce(v - rb.linearVelocity, ForceMode.VelocityChange);

            // Clamp fall speed
            if (rb.linearVelocity.magnitude > maxFallSpeed) rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxFallSpeed);
        }

        // Turn off gravity on slope
        rb.useGravity = !OnSlope();


        // Handle rotation
        if (moveDirection != Vector3.zero && !isAiming)
        {
            float angleDiff = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
            rb.angularVelocity = new Vector3(rb.angularVelocity.x, angleDiff * 0.15f, rb.angularVelocity.z);
        }
        else if (isAiming)
        {
            Vector3 dir = new Vector3(aimCamOrientation.transform.forward.x, 0f, aimCamOrientation.transform.forward.z);
            float angleDiff = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            rb.angularVelocity = new Vector3(rb.angularVelocity.x, angleDiff * 0.17f, rb.angularVelocity.z);
            //Debug.Log(rb.angularVelocity.y);
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
        //rb.linearDamping = grounded ? groundDrag : 0;
    }

    void Teleport()
    {
        if (!isAiming)
        {
            // Make sure target position isn't inside of something
            // Ignores collider
            bool clearLeft = !Physics.Raycast(new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z),
            new Vector3(camOrientation.forward.x, transform.forward.y, camOrientation.forward.z), 3.1f, allLayersExceptPhase, QueryTriggerInteraction.Ignore);
            bool clearRight = !Physics.Raycast(new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z),
            new Vector3(camOrientation.forward.x, transform.forward.y, camOrientation.forward.z), 3.1f, allLayersExceptPhase, QueryTriggerInteraction.Ignore);
            // Draw line for debug

            // Eventually will switch to raycast or something
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + new Vector3(camOrientation.forward.x, transform.forward.y, camOrientation.forward.z) * 3.0f);

            // Check for jump
            if (clearLeft && clearRight && Input.GetKeyDown(KeyCode.Space) && grounded && ladder == null)
            {
                canTeleport = false;
                rb.isKinematic = true; // player unaffected by physics
                //if (ladder != null) teleportFromLadder = true;
                playerModel.SetActive(false); // Make player invisible
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, camOrientation.localEulerAngles.y, transform.localEulerAngles.z);
                exitingSlope = true;
                aura.SetActive(false); // Turn off lightball thing
                StartCoroutine(FlashTeleport(line.GetPosition(1))); // Lerp player to position
                //transform.position = new Vector3(line.GetPosition(1).x, transform.position.y, line.GetPosition(1).z);
                line.enabled = false;
                Invoke(nameof(ResetTeleport), teleportCooldown);
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

    void ResetTeleport()
    {
        // Reset jump variables
        canTeleport = true;
        exitingSlope = false;
        rb.isKinematic = false;
        line.enabled = true;
        playerModel.SetActive(true);
        //aura.SetActive(true);
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
        if (active)
        {
            MMGameEvent.Trigger("CrosshairOn");
        }
        else
        {
            MMGameEvent.Trigger("CrosshairOff");
        }
    }

    void SunWheelHandler()
    {
        if (sunWheel.unlockedAbilities[sunWheel.centerIndex] == SunSpike.SunSpikeType.Telescope)
        {
            if (!item.activeInHierarchy) item.SetActive(true);
        }
        else
        {
            if (item.activeInHierarchy)
            {
                if (item.transform.GetChild(0).gameObject.activeInHierarchy) item.transform.GetChild(0).gameObject.SetActive(false);
                item.SetActive(false);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Exit")
        {
            //gm.ResetScene(); // Restart demo
        }
    }

    public void KinematicMode()
    {
        rb.isKinematic = !rb.isKinematic;
    }

}
