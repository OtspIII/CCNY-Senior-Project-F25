using Unity.Cinemachine;
using UnityEngine;

public class Test_CamZoom : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    private float minZoom;
    private float maxZoom;
    private float zoomSpeed;

    private float zoomInSmoothness;
    private float zoomOutSmoothness;
    private float targetZoom;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        targetZoom = orbitalFollow.RadialAxis.Value;
    }

    // Update is called once per frame
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
