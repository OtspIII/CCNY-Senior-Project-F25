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
    private float zoomInSmoothness;
    private float zoomOutSmoothness;
    private float targetZoom;

    private void Start()
    {
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        // Initialize target zoom from current camera setting
        targetZoom = orbitalFollow.RadialAxis.Value;
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        float currentZoom = orbitalFollow.RadialAxis.Value;
        float smoothness = (targetZoom > currentZoom) ? zoomInSmoothness : zoomOutSmoothness;

        orbitalFollow.RadialAxis.Value = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * smoothness);
    }
}
