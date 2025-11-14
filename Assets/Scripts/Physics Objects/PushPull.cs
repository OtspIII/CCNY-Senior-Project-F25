using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PushPull : MonoBehaviour
{
    [Header("References")]
    public Transform grabOrigin;
    public LayerMask moveableLayer;
    public float grabRange = 2f;
    public float attachDistance = 1f;
    public float moveSpeed = 8f;

    private Rigidbody grabbedBody;
    private bool isGrabbing;

    private PlayerControls controls;
    private InputAction grabAction;
    private InputAction moveAction;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        grabAction = controls.Gameplay.Grab;
        moveAction = controls.Gameplay.Move;
        controls.Gameplay.Enable();

        grabAction.performed += OnGrabPressed;
    }

    private void OnDisable()
    {
        grabAction.performed -= OnGrabPressed;
        controls.Gameplay.Disable();
    }


    void FixedUpdate()
    {
        if (isGrabbing && grabbedBody != null)
        {
            KeepObjectAttached();
        }
    }
    private void OnGrabPressed(InputAction.CallbackContext context)
    {
        if (isGrabbing) ReleaseObject();
        else TryGrabObject();
    }

    private void Update()
    {

        // toggles grab/release
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (isGrabbing) ReleaseObject();
            else TryGrabObject();
        }

        // keeps grabbed object locked infront of the player
        if (isGrabbing && grabbedBody != null)
        {
            KeepObjectAttached();
        }

        Debug.DrawRay(grabOrigin.position, grabOrigin.forward * grabRange, Color.yellow);
    }



    private void TryGrabObject()
    {
        if (Physics.Raycast(grabOrigin.position, grabOrigin.forward, out RaycastHit hit, grabRange, moveableLayer))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null && !rb.isKinematic)
            {
                grabbedBody = rb;
                isGrabbing = true;

                // temporarily makes the obj lighter and disables gravity
                grabbedBody.useGravity = false;
                grabbedBody.linearDamping = 5f;

                Debug.Log("Grabbed: " + grabbedBody.name);
            }
        }
    }

    private void ReleaseObject()
    {
        if(!isGrabbing || grabbedBody == null) return;

        Debug.Log("Released");

        grabbedBody.useGravity = true;
        grabbedBody.linearDamping = 0f;
        grabbedBody = null;
        isGrabbing = false;
    }

    private void KeepObjectAttached()
    {
        Vector3 targetPos = grabOrigin.position + grabOrigin.forward * attachDistance;
        Vector3 moveDir = (targetPos - grabbedBody.position);

        grabbedBody.MovePosition(grabbedBody.position + moveDir * Time.fixedDeltaTime * moveSpeed);

        grabbedBody.MoveRotation(Quaternion.Lerp(grabbedBody.rotation, transform.rotation, Time.fixedDeltaTime));
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float forwardInput = moveInput.y;

        // target follow position
        Vector3 targetPos = grabOrigin.position + grabOrigin.forward * attachDistance;
        Vector3 moveDir = (targetPos - grabbedBody.position);

        // move with physics — safe, smooth collisions
        grabbedBody.MovePosition(
            grabbedBody.position +
            (moveDir + transform.forward * forwardInput) *
            Time.fixedDeltaTime * moveSpeed
        );

        // smoothly rotate to match player
        grabbedBody.MoveRotation(
            Quaternion.Lerp(
                grabbedBody.rotation,
                transform.rotation,
                Time.fixedDeltaTime * 10f
            )
        );
    }
}
