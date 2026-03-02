using UnityEngine;
using Unity.Cinemachine;


public class TempBurn : MonoBehaviour
{
    RaycastHit hit;
    LineRenderer line;
    [SerializeField] LayerMask burn;
    [SerializeField] float convergenceDistance = 2.0f;
    Lantern currentLantern;
    public bool refraction;
    bool burning;
    float lineDistance = 50.0f;
    [SerializeField] CinemachineCamera cam;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.startWidth = 0.3f;
        line.endWidth = 0.3f;
        line.positionCount = 3;
        line.enabled = false;
    }

    void Update()
    {
        if (!line.enabled && !refraction) line.enabled = false;
        LightRefraction();
    }

    void LightRefraction()
    {

        if (!refraction)
        {
            if (line.enabled) line.enabled = false;
            return;
        }

        if (!line.enabled) line.enabled = true;

        Vector3 camPos = Camera.main.transform.position;
        Vector3 camDir = Camera.main.transform.forward;
        
        Vector3 crosshairPoint = camPos + camDir * convergenceDistance;
        
        line.SetPosition(0, transform.position);

        if (Physics.Raycast(camPos, camDir, out hit, 50.0f, burn))
        {
            float hitDistFromCam = Vector3.Distance(camPos, hit.point);
            
            if (hitDistFromCam <= convergenceDistance)
            {
                line.SetPosition(1, hit.point);
                line.SetPosition(2, hit.point);
            }
            else
            {
                line.SetPosition(1, crosshairPoint);
                line.SetPosition(2, hit.point);
            }

            if (hit.transform.gameObject.layer == 8)
            {
                burning = true;
                hit.transform.gameObject.GetComponent<Burnable>().RegisterHit(hit.point);
                lineDistance = hitDistFromCam;
            }
            else
            {
                burning = false;
                currentLantern = hit.transform.gameObject.GetComponent<Lantern>();
                currentLantern.hitsThisFrame = 1;
                if (currentLantern.activeLantern)
                    GetComponentInParent<LanternTravel>().ActivatedLanterns.Add(currentLantern);
            }
        }
        else
        {
            if (burning) burning = false;
            
            line.SetPosition(1, crosshairPoint);
            line.SetPosition(2, camPos + camDir * 50.0f);
            
            if (currentLantern != null && currentLantern.hitsThisFrame > 0)
            {
                currentLantern.hitsThisFrame = 0;
                currentLantern = null;
            }
        }
    }
}
