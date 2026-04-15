using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ProjectorTraversal : MonoBehaviour
{
    public static ProjectorTraversal Instance;

    [Header("References: ")]
    public PlayerMovement player;
    public Rigidbody rb;
    public Transform playerCamera;
    [Space]
    public LightReflection lightReflection; // Players Light Source
    public LightModeToggle lightModeToggle;
    [Space]

    [Header("Projector Rotation Controls (when inside): ")]
    public KeyCode rotateYLeftKey = KeyCode.A;
    public KeyCode rotateYRightKey = KeyCode.D;
    public float ySpeedDegPerSec = 120f;
    private float yAimSpeedDegPerSec = 30f;
    [Space]
    public KeyCode rotateZUpKey = KeyCode.W;    // increases Z rotation (positive)
    public KeyCode rotateZDownKey = KeyCode.S;  // decreases Z rotation (negative)
    public float zSpeedDegPerSec = 60f;
    private float zAimSpeedDegPerSec = 15f;
    [Space]

    [Header("Camera Offset Parameters:")]
    public float xOffset;
    public float yOffset;
    public float zOffset;

    [Header("Traversal Parameters: ")]
    public KeyCode enterTraversalKey = KeyCode.E;
    public KeyCode exitTraversalKey = KeyCode.Space;
    public float travelSpeed = 10f;

    [Header("State: ")]
    public Projector currentProjector = null;
    public bool isInsideProjector = false;
    public bool isTraveling = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (!isInsideProjector)
        {
            // Use PlayerMovement trigger-based detection first (same approach as Lantern)
            Projector detected = GameManager.Instance.Player != null ? GameManager.Instance.Player.projector : null;

            if (detected != null)
            {
                // Ensure beam updates immediately when a projector is detected (no ray hit yet).
                if (lightReflection != null)
                {
                    Vector3 point = detected.beamRoot != null ? detected.beamRoot.position : detected.transform.position;
                    lightReflection.RefreshProjectorProjection(detected, point, registerHit: true, insideProjector: false);
                }

                if (Input.GetKeyDown(enterTraversalKey) && detected.enterable && !isTraveling && (player == null || player.state != PlayerMovement.PlayerState.grabbing))
                {
                    currentProjector = detected;
                    if (currentProjector != null)
                    {
                        EnterProjectorMode();
                        StartCoroutine(MoveToProjector(currentProjector));
                    }
                }
            }
            return;
        }

        // When inside: allow rotating the projector's parent and keep beam alignment logic identical to normal hits.
        if (isInsideProjector && currentProjector != null)
        {
            //Rotation Input:
            HandleProjectorRotationInput();
            transform.position = currentProjector.PivotPosition.position;

            Projector detected = GameManager.Instance.Player != null ? GameManager.Instance.Player.projector : null;

            //Aim Alignment:
            if (player.isAiming)
            {
                //Store Base Offset:
                //Quaternion baseOffset = Quaternion.Euler(detected.baseRotationEuler);

                //Store Camera Forward in Local Space:
                //Vector3 localCameraForward = detected.transform.InverseTransformDirection(playerCamera.forward);

                //Calculate Rotation to Look in that Direction:
                //Quaternion cameraRotationY = Quaternion.LookRotation(localCameraForward, Vector3.up);

                //Apply Horizontal Fix to Align with Projector's Forward:
                //Quaternion horizontalFix = Quaternion.Euler(0f, 90f, 0f);

                //Final Light Rotation:
                //detected.lightRotationOffset = cameraRotationY * horizontalFix * baseOffset;
            }




            //Beam Visual:
            Vector3 point = detected.beamRoot != null ? detected.beamRoot.position : detected.transform.position;
            lightReflection.RefreshProjectorProjection(detected, point, registerHit: true, insideProjector: true);
        }

        // Exit Projector Mode:
        if (Input.GetKeyDown(exitTraversalKey) && isInsideProjector && !isTraveling)
        {
            ExitProjectorMode();
            return;
        }
    }

    private void HandleProjectorRotationInput()
    {
        if (currentProjector == null) return;

        // Choose transform to rotate: ParentObject preferred, otherwise projector.transform
        Transform rotation = currentProjector.ParentObject != null ? currentProjector.ParentObject : currentProjector.transform;

        // Y-axis rotation controlled by A/D, clamped to +/- maxAngleVertical
        float yDelta = 0f;
        float ySpeed = Input.GetMouseButton(1) ? yAimSpeedDegPerSec : ySpeedDegPerSec;
        if (Input.GetKey(rotateYLeftKey)) yDelta -= ySpeed * Time.deltaTime;
        if (Input.GetKey(rotateYRightKey)) yDelta += ySpeed * Time.deltaTime;

        if (Mathf.Abs(yDelta) > Mathf.Epsilon)
        {
            Vector3 localEuler = rotation.localEulerAngles;
            float currentY = NormalizeAngle(localEuler.y);
            float targetY = currentY + yDelta;

            float limitY = currentProjector != null ? currentProjector.maxHorizontalRotatation : 45f;
            targetY = Mathf.Clamp(targetY, -limitY, limitY);

            localEuler.y = targetY;
            rotation.localEulerAngles = localEuler;
        }

        // Z-axis rotation controlled by W/S, clamped to +/- maxAngleVertical
        float zDelta = 0f;
        float zSpeed = Input.GetMouseButton(1) ? zAimSpeedDegPerSec : zSpeedDegPerSec;
        if (Input.GetKey(rotateZUpKey)) zDelta -= zSpeed * Time.deltaTime;
        if (Input.GetKey(rotateZDownKey)) zDelta += zSpeed * Time.deltaTime;

        if (Mathf.Abs(zDelta) > Mathf.Epsilon)
        {
            Vector3 localEuler = rotation.localEulerAngles;
            float currentZ = NormalizeAngle(localEuler.z);
            float targetZ = currentZ + zDelta;

            float limitZ = currentProjector != null ? currentProjector.maxVerticalRotatation : 45f;
            targetZ = Mathf.Clamp(targetZ, -limitZ, limitZ);

            localEuler.z = targetZ;
            rotation.localEulerAngles = localEuler;
        }
    }

    // Convert Euler to signed angle
    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private IEnumerator MoveToProjector(Projector target)
    {
        if (target == null || target.beamRoot == null) yield break;

        isTraveling = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.PivotPosition.position;

        float elapsed = 0f;
        float duration = Vector3.Distance(startPosition, endPosition) / travelSpeed;

        if (duration <= 0f) duration = 0.01f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        isTraveling = false;
    }

    private void EnterProjectorMode()
    {
        if (currentProjector == null) return;

        if (rb != null) rb.isKinematic = true;

        if (lightReflection != null) lightReflection.suppressRaycasting = true;
        if (!lightModeToggle.inProjector) lightModeToggle.inProjector = true;

        isInsideProjector = true;
        if (currentProjector != null) currentProjector.isPlayerInside = true;
    }

    private void ExitProjectorMode()
    {
        isInsideProjector = false;

        if (rb != null) rb.isKinematic = false;

        if (lightReflection != null) lightReflection.suppressRaycasting = false;
        if (lightModeToggle.inProjector) lightModeToggle.inProjector = false;

        isInsideProjector = false;
        if (currentProjector != null) currentProjector.isPlayerInside = false;
        currentProjector.lightRotationOffset = Quaternion.Euler(currentProjector.baseRotationEuler);

        currentProjector = null;
        transform.position = transform.position;
    }

    private void OnDrawGizmos()
    {
        /*if (GameManager.Instance.Player == null) return;

        Projector detected = GameManager.Instance.Player.projector;
        if (detected == null || detected.beamRoot == null) return;

        //Beam Rotation:
        Quaternion finalRotation = detected.ParentObject.rotation * detected.lightRotationOffset * detected.cameraRotationOffset;

        // -------------------------------------------------------------------

        //Beam Root:
        Vector3 origin = detected.beamRoot.position;

        //Point of Origin:
        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawSphere(origin, 0.05f);

        //Beam Length:
        float axisLength = 1.5f;

        Vector3 BasisForward = -currentProjector.transform.right;
        Vector3 BasisRight = currentProjector.transform.forward;
        Vector3 BasisUp = currentProjector.transform.up;

        // -------------------------------------------------------------------

        //Forward Vector Comparison [Z]:
        Gizmos.color = UnityEngine.Color.blue;

        Gizmos.DrawLine(origin, origin + BasisForward * axisLength);
        Gizmos.DrawSphere(origin + BasisForward * axisLength, 0.03f);

        Gizmos.DrawLine(origin, origin + playerCamera.forward * axisLength);
        Gizmos.DrawSphere(origin + playerCamera.forward * axisLength, 0.03f);

        // -------------------------------------------------------------------


        //Right Vector Comparison [X]:
        Gizmos.color = UnityEngine.Color.red;

        Gizmos.DrawLine(origin, origin + BasisRight * axisLength);
        Gizmos.DrawSphere(origin + BasisRight * axisLength, 0.03f);

        Gizmos.DrawLine(origin, origin + playerCamera.right * axisLength);
        Gizmos.DrawSphere(origin + playerCamera.right * axisLength, 0.03f);

        // -------------------------------------------------------------------


        //Up Vector Comparison [Y]:
        Gizmos.color = UnityEngine.Color.green;

        Gizmos.DrawLine(origin, origin + BasisUp * axisLength);
        Gizmos.DrawSphere(origin + BasisUp * axisLength, 0.03f);

        Gizmos.DrawLine(origin, origin + playerCamera.up * axisLength);
        Gizmos.DrawSphere(origin + playerCamera.up * axisLength, 0.03f);

        // -------------------------------------------------------------------

        //Forward Vector Output [Z]:
        Vector3 forward = finalRotation * Vector3.up;
        Gizmos.color = UnityEngine.Color.blue;
        Gizmos.DrawLine(origin, origin + forward * axisLength);

        //Right Vector Output [X]:
        Vector3 right = finalRotation * Vector3.right;
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawLine(origin, origin + right * axisLength);

        //Up Vector Output [Y]:
        Vector3 up = finalRotation * Vector3.forward;
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawLine(origin, origin + up * axisLength);

        // -------------------------------------------------------------------
        */
    }
}