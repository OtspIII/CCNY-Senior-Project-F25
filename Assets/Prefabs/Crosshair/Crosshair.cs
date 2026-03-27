using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour, MMEventListener<MMGameEvent> {
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player toDefault;
    [SerializeField] private MMF_Player toFlame;
    [SerializeField] private MMF_Player toLight;
    [SerializeField] private MMF_Player toNone;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private LayerMask raycastLayers = ~0;

    public enum CrosshairState {
        Default, Flame, Light, None
    }
    
    private CrosshairState currentState = CrosshairState.None;
    private bool isCrosshairOn = false;

    private void Update() {
        if (!isCrosshairOn) {
            SwitchState(CrosshairState.None);
            return;
        }

        DetermineStateFromRaycast();
    }

    private void DetermineStateFromRaycast() {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayers)) {
            if (hit.collider.TryGetComponent<Burnable>(out _)) {
                SwitchState(CrosshairState.Flame);
            } else if (hit.collider.GetComponentInParent<Lantern>() != null) {
                SwitchState(CrosshairState.Light);
            } else {
                SwitchState(CrosshairState.Default);
            }
        } else {
            SwitchState(CrosshairState.Default);
        }
    }

    private void SwitchState(CrosshairState newState) {
        if (currentState == newState)
            return;
        
        if (newState == CrosshairState.Default)
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
        if (feedback == null) return;
        feedback.Initialization();
        feedback.PlayFeedbacks();
    }

    public void OnMMEvent(MMGameEvent gameEvent) {
        if (gameEvent.EventName == "CrosshairOn") {
            isCrosshairOn = true;
        } else if (gameEvent.EventName == "CrosshairOff") {
            isCrosshairOn = false;
            SwitchState(CrosshairState.None);
        }
    }

    void OnEnable() {
        this.MMEventStartListening<MMGameEvent>();
    }

    void OnDisable() {
        this.MMEventStopListening<MMGameEvent>();
    }
}
