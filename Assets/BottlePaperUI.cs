using TMPro;
using MoreMountains.Feedbacks;
using UnityEngine;

public class BottlePaperUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private MMF_Player entranceFeedback;
    [SerializeField] private MMF_Player exitFeedback;

    private void OnEnable()
    {
        Bottle.OnBottleTriggered += HandleBottleTriggered;
        Bottle.OnBottleExit += HandleBottleExit;
    }

    private void OnDisable()
    {
        Bottle.OnBottleTriggered -= HandleBottleTriggered;
        Bottle.OnBottleExit -= HandleBottleExit;
    }

    private void HandleBottleTriggered(string message, float fontSize)
    {
        if (textDisplay != null)
        {
            textDisplay.text = message;
            textDisplay.fontSize = fontSize;
        }

        if (entranceFeedback != null)
        {
            entranceFeedback.PlayFeedbacks();
        }
    }

    private void HandleBottleExit()
    {
        if (exitFeedback != null)
        {
            exitFeedback.PlayFeedbacks();
        }
    }
}
