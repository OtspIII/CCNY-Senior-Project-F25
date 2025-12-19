using UnityEngine;
using MoreMountains.Feedbacks;

public class TutorialPopups : MonoBehaviour
{
    [Header("MMF Players")]
    [SerializeField] private MMF_Player vignetteEnterPlayer;
    [SerializeField] private MMF_Player vignetteExitPlayer;
    [SerializeField] private MMF_Player playerControlsEnterPlayer;
    [SerializeField] private MMF_Player playerControlsExitPlayer;
    [SerializeField] private MMF_Player lanternControlsEnterPlayer;
    [SerializeField] private MMF_Player lanternControlsExitPlayer;
    [SerializeField] private MMF_Player mantaRayEnterPlayer;
    [SerializeField] private MMF_Player mantaRayExitPlayer;

    [Header("Settings")]
    public TutorialType tutorialType;
    [SerializeField] private bool playMoreThanOnce = false;

    private BoxCollider boxCollider;
    private bool hasPlayed = false;
    private bool isShowingTutorial = false;

    public enum TutorialType
    {
        LearnSpyglass,
        LearnPlayer,
        LearnLantern
    }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("TutorialPopups requires a BoxCollider component!");
        }
        
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && !playMoreThanOnce)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Detected");
            ShowTutorial();
        }
    }

    private void Update()
    {
        if (isShowingTutorial && Input.GetKeyDown(KeyCode.Space))
        {
            HideTutorial();
        }
    }

    private void ShowTutorial()
    {
        isShowingTutorial = true;
        hasPlayed = true;

        Time.timeScale = 0f;
        vignetteEnterPlayer?.PlayFeedbacks();

        switch (tutorialType)
        {
            case TutorialType.LearnSpyglass:
                mantaRayEnterPlayer?.PlayFeedbacks();
                break;
            case TutorialType.LearnPlayer:
                playerControlsEnterPlayer?.PlayFeedbacks();
                break;
            case TutorialType.LearnLantern:
                lanternControlsEnterPlayer?.PlayFeedbacks();
                break;
        }
    }

    private void HideTutorial()
    {
        isShowingTutorial = false;

        vignetteExitPlayer?.PlayFeedbacks();

        // Play the appropriate exit feedback based on tutorial type
        switch (tutorialType)
        {
            case TutorialType.LearnSpyglass:
                mantaRayExitPlayer?.PlayFeedbacks();
                break;
            case TutorialType.LearnPlayer:
                playerControlsExitPlayer?.PlayFeedbacks();
                break;
            case TutorialType.LearnLantern:
                lanternControlsExitPlayer?.PlayFeedbacks();
                break;
        }

        // Unpause the game
        Time.timeScale = 1f;
    }

    // Optional: Method to reset the tutorial manually
    public void ResetTutorial()
    {
        hasPlayed = false;
        isShowingTutorial = false;
    }
}