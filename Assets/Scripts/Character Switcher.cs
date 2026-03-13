using UnityEngine;

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

    [Header("State")]
    public bool player1Active = true;
    private bool isPlayerInside = false;
    private bool isSplitModeUnlocked = false;

    [Header("Cinemachine References")]
    [SerializeField] private CameraSwitcher camManager;
    [SerializeField] private Transform player1Anchor; // child obj on player1 for the cam to follow
    [SerializeField] private Transform player2Anchor;

    [Header("POV Camera Setup")]
    [SerializeField] private Camera p1POVCamera;
    [SerializeField] private Camera p2POVCamera;
    [SerializeField] private RenderTexture povTexture;
    [SerializeField] private GameObject povUIPanel; // raw image on canvas

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player1Controller.enabled = true;
        player2Controller.enabled = false;

        // ensure POV cameras are off initially
        p1POVCamera.enabled = false;
        p2POVCamera.enabled = false;
        povUIPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSplitModeUnlocked && isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            UnlockSplitMode();
        }
        else if (isSplitModeUnlocked && Input.GetKeyDown(KeyCode.F))
        {
            SwitchPlayer();
        }
    }

    public void UnlockSplitMode()
    {
        isSplitModeUnlocked = true;
        uiElement.SetActive(false);
        povUIPanel.SetActive(true); // shows small pov window

        SwitchPlayer();

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
            // player 1 is controlled, player 2 is in pov box
            player1Controller.enabled = true;
            player2Controller.enabled = false;

            p1POVCamera.enabled = false;
            p1POVCamera.targetTexture = null;

            p2POVCamera.enabled = true;
            p2POVCamera.targetTexture = povTexture;
        }
        else
        {
            // player 2 is controlled, player 1 is in pov box
            player1Controller.enabled = false;
            player2Controller.enabled = true;

            p2POVCamera.enabled = false;
            p2POVCamera.targetTexture = null;

            p1POVCamera.enabled = true;
            p1POVCamera.targetTexture = povTexture;
        }
    }
}
