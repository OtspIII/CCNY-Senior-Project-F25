using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class Test_AimCamera : MonoBehaviour
{
    [SerializeField] private Transform yawTarget;
    [SerializeField] private Transform pitchTarget;

    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private InputActionReference switchShoulderInput;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float gamepadSensitivity;
    [SerializeField] private float sensitivity;

    [SerializeField] private float pitchMin;
    [SerializeField] private float pitchMax;

    [SerializeField] private float shoulderSwitchSpeed;

    [SerializeField] private Transform playerModel;

    [SerializeField] private CinemachineThirdPersonFollow aimCamFollow;

    [SerializeField] private LayerMask collisionMask;
    
    public Transform YawTarget => yawTarget;

    private float yaw;
    private float pitch;
    private float targetCamSide;

    private void Awake()
    {
        if (aimCamFollow == null)
            aimCamFollow = GetComponent<CinemachineThirdPersonFollow>();
        if (aimCamFollow != null)
        {
            targetCamSide = aimCamFollow.CameraSide;
            aimCamFollow.AvoidObstacles.Enabled = true;
            aimCamFollow.AvoidObstacles.CollisionFilter = collisionMask;
            aimCamFollow.AvoidObstacles.CameraRadius = 0.3f;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        InitYawPitchFromTransform(yawTarget);
        lookInput.asset.Enable();
    }

    private void OnEnable()
    {
        switchShoulderInput.action.Enable();
        switchShoulderInput.action.performed += OnSwitchShoulder;
    }

    private void OnDisable()
    {
        switchShoulderInput.action.Disable();
        switchShoulderInput.action.performed -= OnSwitchShoulder;
    }

    private void OnSwitchShoulder(InputAction.CallbackContext ctx)
    {
        targetCamSide = aimCamFollow.CameraSide < 0.5f ? 1f : 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (float.IsNaN(yaw) || float.IsNaN(pitch))
        {
            Debug.LogWarning("Yaw or Pitch is NaN");
            yaw = 0f;
            pitch = 0f;
        }
        
        Vector2 look = lookInput.action.ReadValue<Vector2>();
        
        if (Mouse.current != null && Mouse.current.delta.IsActuated())
            look *= mouseSensitivity;
        else if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated())
            look *= gamepadSensitivity;
        
        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        
        aimCamFollow.CameraSide = Mathf.Lerp(aimCamFollow.CameraSide, targetCamSide, shoulderSwitchSpeed * Time.deltaTime);
        
    }

    private void InitYawPitchFromTransform(Transform source)
    {
        if (source == null)
        {
            yaw = 0f;
            pitch = 0f;
            return;
        }
        
        Vector3 angles = source.rotation.eulerAngles;
        yaw = float.IsNaN(angles.y) ? 0f : angles.y;
        if (pitchTarget != null)
        {
            float localX = pitchTarget.localEulerAngles.x;
            if (localX > 180f) localX -= 360f;
            pitch = float.IsNaN(localX) ? 0f : localX;
        }
        else
        {
            pitch = 0f;
        }
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    public void SetTargets(Transform newYaw, Transform newPitch, Transform newModel)
    {
        yawTarget = newYaw;
        pitchTarget = newPitch;
        playerModel = newModel;
        
        InitYawPitchFromTransform(newYaw);
    }

    internal void SetYawPitchFromCamForward(Transform cameraTransform)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
        if (flatForward.sqrMagnitude < 0.001f) return;
        
        yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;
        pitch = Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f) * Mathf.Rad2Deg);
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (float.IsNaN(yaw) || float.IsNaN(pitch))
        {
            Debug.LogWarning("Yaw or Pitch is NaN");
            yaw = 0f;
            pitch = 0f;
        }
        
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        
        aimCamFollow.ForceCameraPosition(cameraTransform.position, cameraTransform.rotation);
    }
}
