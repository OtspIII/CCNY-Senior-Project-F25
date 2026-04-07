using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour {
    [Header("Speech Bubble Settings")]
    [SerializeField, TextArea(3, 10)] private string speechText = "Hello!";
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float bubbleSpringDampening = 0.5f;
    [SerializeField] private float bubbleSpringFrequency = 5f;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player spawnFeedback;
    [SerializeField] private MMF_Player despawnFeedback;

    [Header("Billboard Settings")]
    [SerializeField] private Transform canvasTransform; // Assign the Canvas child transform
    
    [Header("References")]
    [SerializeField] private TextMeshProUGUI tmpText; // Assign your TMP text component
    
    [Header("Spring Scale Settings")]
    [SerializeField] private MMSpringScale smallSpringScale;
    [SerializeField] private MMSpringScale mediumSpringScale;
    [SerializeField] private MMSpringScale largeSpringScale;


    private Camera mainCamera;
    private Transform playerTransform;
    private bool playerInRange = false;
    private bool isShowing = false;
    private Vector3 anchorPosition;

    void Awake() {
        // Store the initial world position as anchor
        anchorPosition = transform.position;

        // Find and set the TMP Text Reveal feedback's text
        if (spawnFeedback != null) {
            MMF_TMPTextReveal textReveal = spawnFeedback.GetFeedbackOfType<MMF_TMPTextReveal>();
            if (textReveal != null) {
                textReveal.NewText = speechText;
            }
        }
        
        // Find TMP text if not assigned
        if (tmpText == null) {
            tmpText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Set spring scale dampening and frequency
        SetSpringScaleProperties(smallSpringScale);
        SetSpringScaleProperties(mediumSpringScale);
        SetSpringScaleProperties(largeSpringScale);
    }
    
    void SetSpringScaleProperties(MMSpringScale springScale) {
        if (springScale != null) {
            springScale.SpringVector3.UnifiedSpring.Damping = bubbleSpringDampening;
            springScale.SpringVector3.UnifiedSpring.Frequency = bubbleSpringFrequency;
        }
    }

    void Start() {
        mainCamera = Camera.main;

        // If canvas transform not assigned, use this transform
        if (canvasTransform == null) {
            canvasTransform = transform;
        }
    }

    void Update() {
        CheckPlayerProximity();
    }

    void LateUpdate() {
        // Billboard effect - only rotate the canvas to face camera
        if (mainCamera != null && canvasTransform != null) {
            canvasTransform.rotation = mainCamera.transform.rotation;
        }

        // Keep the root object anchored at its original position
        transform.position = anchorPosition;
    }

    void CheckPlayerProximity() {
        // Find player if not already found
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) {
                playerTransform = player.transform;
            }
            else {
                return; // No player found yet
            }
        }

        // Calculate distance from anchor position to player
        float distance = Vector3.Distance(anchorPosition, playerTransform.position);

        // Player entered range
        if (distance <= detectionRadius && !playerInRange) {
            playerInRange = true;
            ShowSpeechBubble();
        }
        // Player exited range
        else if (distance > detectionRadius && playerInRange) {
            playerInRange = false;
            HideSpeechBubble();
        }
    }

    void ShowSpeechBubble() {
        SetSpringScaleProperties(smallSpringScale);
        SetSpringScaleProperties(mediumSpringScale);
        SetSpringScaleProperties(largeSpringScale);
        if (!isShowing && spawnFeedback != null) {
            // Clear the TMP text before playing the reveal feedback
            if (tmpText != null) {
                tmpText.text = "";
            }
            
            spawnFeedback.PlayFeedbacks();
            isShowing = true;
        }
    }

    void HideSpeechBubble() {
        SetSpringScaleProperties(smallSpringScale);
        SetSpringScaleProperties(mediumSpringScale);
        SetSpringScaleProperties(largeSpringScale);
        if (isShowing && despawnFeedback != null) {
            despawnFeedback.PlayFeedbacks();
            isShowing = false;
        }
    }

}