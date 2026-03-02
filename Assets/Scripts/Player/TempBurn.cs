using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;


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
    List<GameObject> mirrors = new List<GameObject>();

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
        if (currentLantern != null) currentLantern = null;
        LightRefraction();
    }

    void LightRefraction()
    {

        if (!refraction)
        {
            if (line.enabled)
            {
                line.enabled = false;
                line.positionCount = 2;
            }

            return;
        }

        if (!line.enabled) line.enabled = true;

        Vector3 dir = Camera.main.transform.forward;
        line.SetPosition(0, transform.position);
        float lineDist = burning ? lineDistance : 50.0f;
        line.SetPosition(1, transform.position + dir * lineDist);

        if (Physics.Raycast(transform.position, dir, out hit, 50.0f, burn))
        {
            if (hit.transform.gameObject.layer == 11)
            {
                burning = true;
                lineDistance = Vector3.Distance(hit.point, transform.position);

                //if (!mirrors.Contains(hit.transform.gameObject))
                //    mirrors.Add(hit.transform.gameObject);

                //int lineCount = line.positionCount + mirrors.Count + 2;
                line.positionCount = 4;
                Vector3 nextDir = Vector3.Reflect(hit.point, Vector3.right);
                line.SetPosition(2, hit.point);
                line.SetPosition(3, hit.point + nextDir * 50.0f);
                RaycastHit newHit;
                if (Physics.Raycast(hit.point, Vector3.Reflect(hit.point, Vector3.right), out newHit, 50.0f, burn))
                {
                    if (hit.transform.gameObject.layer == 8)
                    {
                        newHit.transform.gameObject.GetComponent<Burnable>().ApplyBurn(Time.deltaTime);
                    }
                }
            }
            if (hit.transform.gameObject.layer == 8)
            {
                burning = true;
                hit.transform.gameObject.GetComponent<Burnable>().ApplyBurn(Time.deltaTime);
                lineDistance = Vector3.Distance(hit.point, transform.position);
            }
            else if (hit.transform.gameObject.layer == 12)
            {
                burning = false;
                currentLantern = hit.transform.gameObject.GetComponent<Lantern>();
                currentLantern.hitsThisFrame = 1;
            }
            else
            {
                if (currentLantern != null && currentLantern.activeLantern)
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
            if (line.positionCount > 2) line.positionCount = 2;
        }
        //Debug.DrawRay(transform.position, dir * 50.0f, Color.magenta);
    }
}
