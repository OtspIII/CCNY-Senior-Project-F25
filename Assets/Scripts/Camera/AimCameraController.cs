using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [SerializeField] private Transform yawTarget; // target that was created under the player
    [SerializeField] private Transform pitchTarget; //child of yawTarget

    [SerializeField] private InputActionReference lookInput; // reference to input action that deals with looking
    [SerializeField] private InputActionReference switchShouldInput; //button that switches left to right shoulders

    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private float gamepadSensitvity = 0.5f;
    [SerializeField] private float sensitivity = 1.5f;

    [SerializeField] private float pitchMin = -40f;
    [SerializeField] private float pitchMax = 80f;

    [SerializeField] private CinemachineThirdPersonFollow aimCam;

    [SerializeField] private float shoulderSwitchSpeed = 5f;

    [SerializeField] private Transform playerModel;

    private float yaw;
    private float pitch;
    private float targetCameraSide;

    private void Awake()
    {
        aimCam = GetComponent<CinemachineThirdPersonFollow>();
        targetCameraSide = aimCam.CameraSide; // initializes target camera side
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 angles = yawTarget.rotation.eulerAngles; // default yaw rotation
        yaw = angles.y;
        pitch = angles.x;

        lookInput.asset.Enable();
    }


    private void OnEnable()
    {
        switchShouldInput.action.Enable();
        switchShouldInput.action.performed += OnSwitchShoulder;
    }

    private void OnDisable()
    {
        switchShouldInput.action.Disable();
        switchShouldInput.action.performed -= OnSwitchShoulder;
    }

    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        targetCameraSide = aimCam.CameraSide < 0.5f ? 1f : 0f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 look = lookInput.action.ReadValue<Vector2>(); // will take the look input and set it equal to the look variable 

        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            look *= mouseSensitivity; // mulitplies look input byu sensitivity
        }
        else if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated())
        {
            look *= gamepadSensitvity;
        }

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        aimCam.CameraSide = Mathf.Lerp(aimCam.CameraSide, targetCameraSide, Time.deltaTime * shoulderSwitchSpeed);
    }

    private void LateUpdate()
    {
        if (playerModel != null)
        {
            float yawOnly = yawTarget.eulerAngles.y;
            Quaternion offset = Quaternion.Euler(-90f, 0f, 0f);

            playerModel.rotation = Quaternion.Euler(0f, yawOnly, 0f) * offset;
        }
    }

    internal void SetYawPitchFromCameraForward(Transform cameraTransform)
    {
        Vector3 forward = cameraTransform.forward;

        // gets yaw from flattened forward
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
        if (flatForward.sqrMagnitude < 0.001f)
            return;

        yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;

        // calculates pitch from camera forward
        pitch = -Mathf.Asin(forward.y) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(0f, 0f, 0f); // resets the pitch to 0

        aimCam.ForceCameraPosition(cameraTransform.position, cameraTransform.rotation);
    }
}
