using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class UIObserver : MonoBehaviour {
    #region Crosshair
    // Tracks ball collision events
    public delegate void OnCrosshairChange(Crosshair.CrosshairState newState);
    public static event OnCrosshairChange CrosshairChange;

    public static void NotifyCrosshairChange(Crosshair.CrosshairState newState) {
        CrosshairChange?.Invoke(newState);
    }
    #endregion

}