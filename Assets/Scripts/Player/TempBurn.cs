using UnityEngine;
using Unity.Cinemachine;


public class TempBurn : MonoBehaviour
{
    RaycastHit hit;
    LineRenderer line;
    [SerializeField] LayerMask burn;
    public bool refraction;
    [SerializeField] CinemachineCamera cam;

    //[SerializeField] LayerMask burn;
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
        if (!refraction) return;

        if (!line.enabled) line.enabled = true;

        Vector3 dir = Camera.main.transform.forward;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + dir * 50.0f);

        if (Physics.Raycast(transform.position, dir, out hit, 50.0f, burn))
        {
            hit.transform.gameObject.GetComponent<Burnable>().ApplyBurn(Time.deltaTime);
        }
        //Debug.DrawRay(transform.position, dir * 50.0f, Color.magenta);
    }
}
