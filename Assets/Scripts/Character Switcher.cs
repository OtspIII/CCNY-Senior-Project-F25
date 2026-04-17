using UnityEngine;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement player1Controller;
    [SerializeField] private PlayerMovement player2Controller;
    [SerializeField] private GameObject uiElement;

    [SerializeField] private Transform player1YawTarget;
    [SerializeField] private Transform player1PitchTarget;
    [SerializeField] private Transform player1Model;

    [SerializeField] private Transform player2YawTarget;
    [SerializeField] private Transform player2PitchTarget;
    [SerializeField] private Transform player2Model;

    [SerializeField] private List<PromptTrigger> promptTriggers;

    [Header("State")]
    public bool player1Active = true;
    private bool isPlayerInside = false;
    public bool isSplitModeUnlocked = false;

    [Header("Cinemachine References")]
    [SerializeField] private CameraSwitcher camManager;
    [SerializeField] private Test_CamDistance mainCam;
    [SerializeField] private Transform player1Anchor; // child obj on player1 for the cam to follow
    [SerializeField] private Transform player2Anchor;

    [Header("POV Camera Setup")]
    [SerializeField] private Camera p1POVCamera;
    [SerializeField] private Camera p2POVCamera;
    [SerializeField] private RenderTexture povTexture;
    [SerializeField] private GameObject povUIPanel; // raw image on canvas
    [SerializeField] private CameraAiming player1CameraAiming;
    [SerializeField] private CameraAiming player2CameraAiming;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player1Controller.enabled = true;
        player2Controller.enabled = false;

        // ensure POV cameras are off initially
        p1POVCamera.enabled = false;
        p2POVCamera.enabled = false;
        povUIPanel.SetActive(false);

        player2Controller.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {


        if (!isSplitModeUnlocked && isPlayerInside && Input.GetKeyDown(KeyCode.C))
        {
            UnlockSplitMode();
        }
        else if (isSplitModeUnlocked && Input.GetKeyDown(KeyCode.C) && (GameManager.Instance.LanternTravel == null || !GameManager.Instance.LanternTravel.isInsideLantern))
        {
            SwitchPlayer();
        }
    }

    public void UnlockSplitMode()
    {
        isSplitModeUnlocked = true;
        uiElement.SetActive(false);
        povUIPanel.SetActive(true); // shows small pov window

        foreach (PromptTrigger pt in promptTriggers)
        {
            pt.ForceExitFPV();
        }

        SwitchPlayer();

    }

    public bool IsAnyLensActive()
    {
        foreach (PromptTrigger pt in promptTriggers)
        {
            if (pt.IsPlayerInside()) return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isSplitModeUnlocked)
        {
            isPlayerInside = true;
            uiElement.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (!isSplitModeUnlocked) uiElement.SetActive(false);
        }
    }

    void SwitchPlayer()
    {
        player1Active = !player1Active; // toggle active player boolean

        // handles cinemachine target switching
        Transform activeTarget;
        if (player1Active == true)
        {
            activeTarget = player1Anchor;
        }
        else
        {
            activeTarget = player2Anchor;
        }

        if (player1Active)
        {
            camManager.UpdateTargets(
                player1Anchor,
                player1YawTarget,
                player1PitchTarget,
                player1Model);
        }
        else
        {
            camManager.UpdateTargets(
                player2Anchor,
                player2YawTarget,
                player2PitchTarget,
                player2Model);
        }

        UpdateControlsandPOV();
    }

    void UpdateControlsandPOV()
    {
        if (player1Active)
        {
            player1CameraAiming.enabled = true;
            player2CameraAiming.enabled = false;

            // player 1 is controlled, player 2 is in pov box
            player1Controller.enabled = true;

            // Empty player references in Game Manager
            GameManager.Instance.Player = null;
            GameManager.Instance.LanternTravel = null;

            Rigidbody p2rb = player2Controller.GetComponent<Rigidbody>();
            if (p2rb != null)
            {
                p2rb.linearVelocity = Vector3.zero;
                p2rb.angularVelocity = Vector3.zero;
            }

            // Add active player to Game Manager
            GameManager.Instance.Player = player1Controller;

            // Switch active lantern travel script
            player1Controller.GetComponent<LanternTravel>().enabled = true;
            player2Controller.GetComponent<LanternTravel>().enabled = false;

            // Update Main Camera player reference
            mainCam.playerTarget = player1Controller.yawTarget;

            // Add active lantern travel to Game Manager
            GameManager.Instance.LanternTravel = player1Controller.gameObject.GetComponent<LanternTravel>();

            foreach (Lantern l in player2Controller.GetComponent<LanternTravel>().ActivatedLanterns)
            {
                if (player1Controller.gameObject.GetComponent<LanternTravel>().ActivatedLanterns.Contains(l))
                    continue; // Skip lamps already active in p2 script
                else
                    player1Controller.gameObject.GetComponent<LanternTravel>().ActivatedLanterns.Add(l);
            }

            // Turn off player movement script on p2
            player2Controller.enabled = false;

            // Switch camera
            p1POVCamera.enabled = false;
            p1POVCamera.targetTexture = null;

            p2POVCamera.enabled = true;
            p2POVCamera.targetTexture = povTexture;
        }
        else
        {
            player1CameraAiming.enabled = false;
            player2CameraAiming.enabled = true;

            // player 2 is controlled, player 1 is in pov box
            player2Controller.enabled = true;

            Rigidbody p1rb = player1Controller.GetComponent<Rigidbody>();
            if (p1rb != null)
            {
                p1rb.linearVelocity = Vector3.zero;
                p1rb.angularVelocity = Vector3.zero;
            }

            // Set player 2 to active
            //if (!player2Controller.gameObject.activeInHierarchy)
            //    player2Controller.gameObject.SetActive(true);

            // Empty player references in Game Manager
            GameManager.Instance.Player = null;
            GameManager.Instance.LanternTravel = null;

            GameManager.Instance.Player = player2Controller;
            player2Controller.GetComponent<LanternTravel>().enabled = true;

            // Update Main Camera player reference
            mainCam.playerTarget = player2Controller.yawTarget;

            // Add active lantern travel to Game Manager
            GameManager.Instance.LanternTravel = player2Controller.gameObject.GetComponent<LanternTravel>();

            // Update lamps when player 2 spawns
            foreach (Lantern l in player1Controller.GetComponent<LanternTravel>().ActivatedLanterns)
            {
                if (player2Controller.gameObject.GetComponent<LanternTravel>().ActivatedLanterns.Contains(l))
                    continue; // Skip lamps already active in p2 script
                else
                    player2Controller.gameObject.GetComponent<LanternTravel>().ActivatedLanterns.Add(l);
            }

            // Turn off p1 lantern travel after checking for active lanterns
            player1Controller.GetComponent<LanternTravel>().enabled = false;

            // Turn off player movement script on p1
            player1Controller.enabled = false;

            // Switch camera
            p2POVCamera.enabled = false;
            p2POVCamera.targetTexture = null;

            p1POVCamera.enabled = true;
            p1POVCamera.targetTexture = povTexture;
        }
    }
}
