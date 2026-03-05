using UnityEngine;
using Unity.Cinemachine;


public class TempBurn : MonoBehaviour
{
    RaycastHit hit;
    LineRenderer line;
    [SerializeField] LayerMask burn;
    [SerializeField] float convergenceDistance = 2.0f;
    [SerializeField] float startThickness = 0.5f;
    [SerializeField] float endThickness = 0.1f;
    Lantern currentLantern;
    public bool refraction;
    bool burning;
    float lineDistance = 50.0f;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] Material burningMaterial;
    [SerializeField] Material idleMaterial;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 3;
        line.enabled = false;
        
        if (idleMaterial != null)
            line.material = idleMaterial;
    }

    void Update()
    {
        if (!line.enabled && !refraction) line.enabled = false;
        LightRefraction();
        UpdateLineWidth();
    }

    void UpdateLineWidth()
    {
        if (!line.enabled) return;

        Vector3 p0 = line.GetPosition(0);
        Vector3 p1 = line.GetPosition(1);
        Vector3 p2 = line.GetPosition(2);

        float dist01 = Vector3.Distance(p0, p1);
        float dist12 = Vector3.Distance(p1, p2);
        float totalDist = dist01 + dist12;

        if (totalDist <= 0.001f) return;

        float normP1 = dist01 / totalDist;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, startThickness);
        curve.AddKey(normP1, endThickness);
        curve.AddKey(1f, endThickness);

        line.widthCurve = curve;
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
                if (!burning)
                {
                    burning = true;
                    if (burningMaterial != null) line.material = burningMaterial;
                }
                hit.transform.gameObject.GetComponent<Burnable>().RegisterHit(hit.point);
                lineDistance = hitDistFromCam;
            }
            else
            {
                if (burning)
                {
                    burning = false;
                    if (idleMaterial != null) line.material = idleMaterial;
                }
                currentLantern = hit.transform.gameObject.GetComponent<Lantern>();
                currentLantern.hitsThisFrame = 1;
                if (currentLantern.activeLantern)
                    GetComponentInParent<LanternTravel>().ActivatedLanterns.Add(currentLantern);
            }
        }
        else
        {
            if (burning)
            {
                burning = false;
                if (idleMaterial != null) line.material = idleMaterial;
            }
            
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
