using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_CamSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera aimCam;
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private Camera mainCam;

    [SerializeField] private GameObject crosshairUI;

    private PlayerControls input;
    [SerializeField] private Test_AimCamera aimController;
    private InputAction aimAction;
    private bool isAiming = false;

    private Test_AimCamera aimCamController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aimCamController = aimCam.GetComponent<Test_AimCamera>();
        inputAxisController = freelookCam.GetComponent<CinemachineInputAxisController>();

        input = new PlayerControls();
        input.Enable();
        aimAction = input.Gameplay.Aim;

        ExitAimMode();
    }

    // Update is called once per frame
    void Update()
    {
        bool aimPressed = Input.GetMouseButton(1) ||
        (GameManager.Instance.Player.projector != null && GameManager.Instance.Player.projector.isPlayerInside) ||
        GameManager.Instance.LanternTravel.isInsideLantern; ;

        if (aimPressed && !isAiming)
            EnterAimMode();
        else if (!aimPressed && isAiming)
            ExitAimMode();
    }

    private void ExitAimMode()
    {
        isAiming = false;
        SnapFreeLookToAimForward();

        aimCam.Priority = 10;
        freelookCam.Priority = 20;

        inputAxisController.enabled = true;

        if (crosshairUI != null) crosshairUI.SetActive(false);
    }

    private void SnapFreeLookToAimForward()
    {
        CinemachineOrbitalFollow orbitalFollow = freelookCam.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) return;

        Vector3 forward = (aimCamController != null && aimCamController.YawTarget != null)
            ? aimCamController.YawTarget.forward : aimCam.transform.forward;

        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        orbitalFollow.HorizontalAxis.Value = angle;
    }

    private void EnterAimMode()
    {
        isAiming = true;
        SnapAimCameraToFreelookForward();

        aimCam.Priority = 20;
        freelookCam.Priority = 10;

        inputAxisController.enabled = false;

        if (crosshairUI != null) crosshairUI.SetActive(true);
    }

    private void SnapAimCameraToFreelookForward()
    {
        aimCamController.SetYawPitchFromCamForward(freelookCam.transform);
    }

    public void UpdateTargets(Transform followTarget, Transform yawTarget, Transform pitchTarget, Transform playerModel)
    {
        freelookCam.Follow = followTarget;
        freelookCam.LookAt = followTarget;

        aimCam.Follow = yawTarget;
        aimCam.LookAt = yawTarget;

        aimController.SetTargets(yawTarget, pitchTarget, playerModel);
        SnapFreeLookToAimForward();
    }
}
