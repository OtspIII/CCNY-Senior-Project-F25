using UnityEngine;

public class CameraAiming : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Rotation Settings")]
    public float horizontalSpeed = 100f;
    public float verticalSpeed = 80f;
    [Space]
    public Vector3 rotationOffset = new Vector3(-90f, 0f, 0f); // Offset to align Light Forward Since it Faces Upwards

    private float currentYaw;   // Y-Axis rotation
    private float currentPitch; // X-Axis rotation

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        //NOTE: [Euler Angles Are in the order of (X, Y, Z) => (Pitch, Yaw, Roll)]
        Vector3 initialEuler = transform.eulerAngles;

        //Store Initial Rotation As Starting Point For Smooth Transition:
        currentYaw = initialEuler.y;
        currentPitch = initialEuler.x;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        //Get Camera Rotation With Euler Angles:
        Vector3 cameraEuler = mainCamera.transform.eulerAngles;

        //Store Camera Rotation With Offset For Proper Alignment:
        float targetYaw = cameraEuler.y + rotationOffset.y;
        float targetPitch = cameraEuler.x + rotationOffset.x;

        //REFRENCE: [https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mathf.MoveTowardsAngle.html]
        //Parameters => (Current Axis Rotation, Target Axis Rotation, Speed * DeltaTime)
        //TLDR: Interpolates Between Current And Target Rotation Smoothly Over Time.
        currentYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, horizontalSpeed * Time.deltaTime);
        currentPitch = Mathf.MoveTowardsAngle(currentPitch, targetPitch, verticalSpeed * Time.deltaTime);

        //Apply Real Time Rotation Update To The Light Object:
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, rotationOffset.z);
    }
}