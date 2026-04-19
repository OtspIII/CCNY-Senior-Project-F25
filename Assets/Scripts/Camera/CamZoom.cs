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
    
    private LanternTravel lanternTravel;

    private void Start()
    {
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        // Initialize target zoom from current camera setting
        targetZoom = orbitalFollow.RadialAxis.Value;

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            lanternTravel = GameManager.Instance.Player.GetComponent<LanternTravel>();
        }
    }

    void Update()
    {
        /*float scroll = Input.mouseScrollDelta.y;
        bool inLantern = GetComponent<LanternTravel>().isInsideLantern;
        float effectiveZoomSpeed = inLantern ? 2f : zoomSpeed;
        float effectiveMaxZoom = inLantern ? 1f : maxZoom;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * effectiveZoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, effectiveMaxZoom);
        }

        float currentZoom = orbitalFollow.RadialAxis.Value;
        float smoothness = (targetZoom > currentZoom) ? zoomInSmoothness : zoomOutSmoothness;

        orbitalFollow.RadialAxis.Value = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * smoothness);*/
        if (orbitalFollow == null) return;
        bool inLantern = (lanternTravel != null) && lanternTravel.isInsideLantern;
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float effectiveSpeed = inLantern ? zoomSpeed * 0.4f : zoomSpeed;
            targetZoom -= scroll * effectiveSpeed * Time.deltaTime;

            float effectiveZoom = inLantern ? 1f : maxZoom;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        float currentZoom = orbitalFollow.RadialAxis.Value;
        orbitalFollow.RadialAxis.Value = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * 5f);
    }
}
