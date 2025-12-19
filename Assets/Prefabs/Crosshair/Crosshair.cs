using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour {
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player toDefault;
    [SerializeField] private MMF_Player toFlame;
    [SerializeField] private MMF_Player toLight;
    [SerializeField] private MMF_Player toNone;


    public enum CrosshairState {
        Default, Flame, Light, None
    }
    CrosshairState currentState = CrosshairState.Default;

    private void SwitchState(CrosshairState newState) {
        if (currentState == newState)
            return;
        else if (newState == CrosshairState.Default)
            PlayFeedback(toDefault);
        else if (newState == CrosshairState.Flame)
            PlayFeedback(toFlame);
        else if (newState == CrosshairState.Light)
            PlayFeedback(toLight);
        else if (newState == CrosshairState.None)
            PlayFeedback(toNone);
        currentState = newState;
    }

    void PlayFeedback(MMF_Player feedback) {
        feedback.Initialization();
        feedback.PlayFeedbacks();
    }

    void OnEnable() {
        UIObserver.CrosshairChange += OnCrosshairChange;
    }
    void OnDisable() {
        UIObserver.CrosshairChange -= OnCrosshairChange;
    }

    void OnCrosshairChange(CrosshairState newState) {
        SwitchState(newState);
    }
    //call UIObserver.NotifyCrosshairChange(Crosshair.CrosshairState.NEWSTATE); in other scripts
}
