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
    }

    // Update is called once per frame
    void Update()
    {
        bool aimPressed = aimAction.IsPressed();
        PlayerMovement.player.isAiming = aimPressed;

        if (aimPressed && !isAiming)
        {
            EnterAimMode();
        }
        else if (!aimPressed && isAiming)
        {
            ExitAimMode();
        }
    }

    private void ExitAimMode()
    {
        isAiming = false;

        SnapFreeLookBehindPlayer();

        aimCam.Priority = 10;
        freelookCam.Priority = 20;

        inputAxisController.enabled = true;
    }

    private void SnapFreeLookBehindPlayer()
    {
        CinemachineOrbitalFollow orbitalFollow = freelookCam.GetComponent<CinemachineOrbitalFollow>();
        Vector3 forward = aimCam.transform.forward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        orbitalFollow.HorizontalAxis.Value = angle;
    }

    private void SnapAimCameraToPlayerForward()
    {
        aimCamController.SetYawPitchFromCameraForward(freelookCam.transform);
    }

    private void EnterAimMode()
    {
        isAiming = true;

        SnapAimCameraToPlayerForward();

        aimCam.Priority = 20;
        freelookCam.Priority = 10;

        inputAxisController.enabled = false; // freelook cam cannot rotate
    }
}
