using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;

public class ProjectorTraversal : MonoBehaviour
{
    public static ProjectorTraversal Instance;

    [Header("References: ")]
    public PlayerMovement player;
    public Rigidbody rb;
    [Space]
    public LightReflection lightReflection; // Players Light Source
    public LightModeToggle lightModeToggle;
    [Space]

    [Header("Projector Rotation Controls (when inside): ")]
    public KeyCode rotateYLeftKey = KeyCode.A;
    public KeyCode rotateYRightKey = KeyCode.D;
    public float ySpeedDegPerSec = 120f;
    [Space]
    public KeyCode rotateZUpKey = KeyCode.W;    // increases Z rotation (positive)
    public KeyCode rotateZDownKey = KeyCode.S;  // decreases Z rotation (negative)
    public float zSpeedDegPerSec = 60f;
    [Space]

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
            Projector detected = PlayerMovement.player != null ? PlayerMovement.player.projector : null;

            if (detected != null && detected.enterable)
            {
                // Ensure beam updates immediately when a projector is detected (no ray hit yet).
                if (lightReflection != null)
                {
                    Vector3 point = detected.beamRoot != null ? detected.beamRoot.position : detected.transform.position;
                    lightReflection.RefreshProjectorProjection(detected, point, registerHit: true);
                }

                if (Input.GetKeyDown(enterTraversalKey) && !isTraveling && (player == null || player.state != PlayerMovement.PlayerState.grabbing))
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
            //Rotation input:
            HandleProjectorRotationInput();

            //Ensure player stays at pivot point (prevents sliding if on a slope or if projector moves):
            transform.position = currentProjector.PivotPosition.position;

            //Visual updates:
            Projector detected = PlayerMovement.player != null ? PlayerMovement.player.projector : null;
            Vector3 point = detected.beamRoot != null ? detected.beamRoot.position : detected.transform.position;
            lightReflection.RefreshProjectorProjection(detected, point, registerHit: true);
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
        if (Input.GetKey(rotateYLeftKey)) yDelta += ySpeedDegPerSec * Time.deltaTime;
        if (Input.GetKey(rotateYRightKey)) yDelta -= ySpeedDegPerSec * Time.deltaTime;

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
        if (Input.GetKey(rotateZUpKey)) zDelta += zSpeedDegPerSec * Time.deltaTime;
        if (Input.GetKey(rotateZDownKey)) zDelta -= zSpeedDegPerSec * Time.deltaTime;

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

        currentProjector = null;
        transform.position = transform.position;
    }
}