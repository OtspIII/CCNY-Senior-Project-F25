using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DrawShadows : MonoBehaviour
{
    [SerializeField] Transform box;
    [SerializeField] Transform[] boxCorners;
    [SerializeField] List<Vector3> currentRayPoints = new List<Vector3>();
    Transform playerLight;
    [SerializeField] GameObject testPrefab;
    [SerializeField] LayerMask layerMask;
    GameObject shadow;
    bool shadowCreated;
    float rayDistance = 10.0f;
    RaycastHit hit;
    Vector3 velocity = Vector3.zero;
    float time = 0.15f;


    void Start()
    {
        playerLight = GetComponentInChildren<Light>().transform;
        boxCorners = new Transform[box.childCount];

        for (int i = 0; i < boxCorners.Length; i++)
        {
            boxCorners[i] = box.GetChild(i);
        }
    }
    void Update()
    {
        currentRayPoints.Clear();

        for (int i = 0; i < boxCorners.Length; i++)
        {
            Vector3 direction = boxCorners[i].position - playerLight.position;
            direction.Normalize();
            Debug.DrawRay(boxCorners[i].position, direction * rayDistance, Color.cyan);
            if (Physics.Raycast(boxCorners[i].position, direction, out hit, rayDistance, layerMask))
            {
                Vector3 offset = hit.point - direction * 0.005f;
                currentRayPoints.Add(offset);
            }
        }

        if (Input.GetMouseButtonDown(0) && !shadowCreated)
        {
            shadowCreated = true;
            shadow = Instantiate(testPrefab, GetAveragePosition(currentRayPoints), Quaternion.identity);
        }

        if (shadow != null)
        {
            shadow.transform.position = Vector3.SmoothDamp
            (
                shadow.transform.position,
                GetAveragePosition(currentRayPoints),
                ref velocity,
                time
            );
            //shadow.transform.position = GetAveragePosition(currentRayPoints);

            float dist = Vector3.Distance(box.position, playerLight.position);
            shadow.transform.localScale = Vector3.one / dist;
        }
    }

    public Vector3 GetAveragePosition(List<Vector3> points)
    {
        if (points.Count == 0) return Vector3.zero;

        Vector3 totalPosition = Vector3.zero;
        foreach (Vector3 p in points)
        {
            totalPosition += p;
        }
        return totalPosition / points.Count;
    }
}
