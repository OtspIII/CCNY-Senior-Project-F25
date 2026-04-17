using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using MoreMountains.Tools;

public class FeedbackTester : MonoBehaviour
{
    [Header("Feedbacks")]
    [Tooltip("Assign 9 MMF Players here. They will be played with keys 1-9 respectively.")]
    public List<MMF_Player> Feedbacks = new List<MMF_Player>(9);

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) MMGameEvent.Trigger("TestEvent");
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayFeedback(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayFeedback(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayFeedback(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayFeedback(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) PlayFeedback(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) PlayFeedback(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) PlayFeedback(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) PlayFeedback(8);
    }

    private void PlayFeedback(int index)
    {
        if (Feedbacks != null && index < Feedbacks.Count && Feedbacks[index] != null)
        {
            Feedbacks[index].PlayFeedbacks();
        }
    }
}
