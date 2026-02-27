using UnityEngine;
using Unity.Cinemachine;


public class TempBurn : MonoBehaviour
{
    RaycastHit hit;
    LineRenderer line;
    [SerializeField] LayerMask burn;
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

        Vector3 dir = Camera.main.transform.forward;
        line.SetPosition(0, transform.position);
        float lineDist = burning ? lineDistance : 50.0f;
        line.SetPosition(1, transform.position + dir * lineDist);

        if (Physics.Raycast(transform.position, dir, out hit, 50.0f, burn))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                burning = true;
                hit.transform.gameObject.GetComponent<Burnable>().ApplyBurn(Time.deltaTime);
                lineDistance = Vector3.Distance(hit.point, transform.position);
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
            if (currentLantern != null && currentLantern.hitsThisFrame > 0)
            {
                currentLantern.hitsThisFrame = 0;
                currentLantern = null;
            }
        }
        //Debug.DrawRay(transform.position, dir * 50.0f, Color.magenta);
    }
}
