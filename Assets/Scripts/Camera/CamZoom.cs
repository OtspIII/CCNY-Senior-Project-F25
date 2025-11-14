using UnityEngine;
using Unity.Cinemachine;

public class CamZoom : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.25f;   // closest
    [SerializeField] private float maxZoom = 2.5f;    // farthest
    [SerializeField] private float zoomSpeed = 5f;    // how fast zoom reacts
    [SerializeField] private float smoothness = 10f;  // how smooth the zoom is

    private float targetZoom;

    private void Start()
    {
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        // Initialize target zoom from current camera setting
        targetZoom = orbitalFollow.RadialAxis.Value;
    }

    private void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Apply zoom direction (invert scroll if needed)
            targetZoom -= scroll * zoomSpeed * Time.deltaTime;

            // Clamp the zoom distance
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly interpolate zoom, not instantly snap
        orbitalFollow.RadialAxis.Value = Mathf.Lerp(
            orbitalFollow.RadialAxis.Value,
            targetZoom,
            Time.deltaTime * smoothness
        );
    }
}
