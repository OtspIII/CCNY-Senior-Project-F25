using System;
using UnityEngine;

public class AimActiveState : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("[Aiming Tool] Enabled");
        Debug.Log($"[DisableTracer] {name} ENABLED");
    }

    void OnDisable()
    {
        Debug.Log("[Aiming Tool] Disabled");
        Debug.LogError(
            $"[DisableTracer] {name} was DISABLED.\nStackTrace:\n{Environment.StackTrace}"
        );
    }
}
