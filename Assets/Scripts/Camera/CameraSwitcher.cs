using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    //the script will listen for aim input, raise the aim camera's priority and then disable zoom input, and rotate the player to match the camera forward on exit

    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera aimCam; // will be used to switch btwn those two cams
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private Camera mainCamera;
    //[SerializeField] private PlayerController player;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private PlayerControls input;

    [SerializeField] private AimCameraController aimController;

    private InputAction aimAction;
    private bool isAiming = false;
    private Transform yawTarget;
    private Transform pitchTarget;

    private AimCameraController aimCamController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aimCamController = aimCam.GetComponent<AimCameraController>();
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
        (GameManager.Instance.Player.projector != null && GameManager.Instance.Player.projector.isPlayerInside)
        || GameManager.Instance.LanternTravel.isInsideLantern;

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

    private void SnapAimCameraToFreelookForward()
    {
        if (GameManager.Instance.Player.projector == null &&
        !GameManager.Instance.LanternTravel.isInsideLantern)
            aimCamController.SetYawPitchFromCamForward(freelookCam.transform);
        else
            aimCamController.SetYawPitchFromCamForward(GameManager.Instance.Player.transform);
        //aimCamController.SetYawPitchFromCamForward(freelookCam.transform);

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


    public void UpdateTargets(Transform followTarget, Transform yawTarget, Transform pitchTarget, Transform playerModel)
    {
        freelookCam.Follow = followTarget;
        freelookCam.LookAt = followTarget;

        aimCam.Follow = pitchTarget;
        aimCam.LookAt = pitchTarget;

        aimController.SetTargets(yawTarget, pitchTarget, playerModel);
        SnapFreeLookToAimForward();
    }

}
