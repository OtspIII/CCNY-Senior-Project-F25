using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DrawShadows : MonoBehaviour
{
    [Header("Shadow Object")]
    [SerializeField] Transform box; // Object that will cast the shadow
    [Tooltip("Leave empty. Fills out at runtime.")]
    [SerializeField] Transform[] boxCorners; // All corners of the shadow object
    [Space(15)]
    [SerializeField] List<Vector3> currentRayPoints = new List<Vector3>(); // List of all points hitting wall that projects shadow
    Transform playerLight; // Light inside player
    [SerializeField] GameObject testPrefab; // Test for shadow sprite
    [SerializeField] LayerMask layerMask;
    GameObject shadow; // This will store the prefab once instantiated
    bool shadowCreated; // Track when shadow is created to avoid duplicates
    float rayDistance = 10.0f; // ya know
    RaycastHit hit;
    Vector3 velocity = Vector3.zero;
    float time = 0.15f; // Time it takes for shadow to move into position
    public bool shadowPuzzleActive;


    void Start()
    {
        // Get player light
        playerLight = GetComponentInChildren<Light>().transform;
        boxCorners = new Transform[box.childCount];

        // Use children as reference for all corners
        for (int i = 0; i < boxCorners.Length; i++)
        {
            boxCorners[i] = box.GetChild(i);
        }
    }
    void Update()
    {
        if (!shadowPuzzleActive)
        {
            if (currentRayPoints.Count > 0) currentRayPoints.Clear();
            if (shadow != null) Destroy(shadow);
            if (shadowCreated) shadowCreated = false;

            return;
        }

        // Clear raycast hits every frame
        currentRayPoints.Clear();

        for (int i = 0; i < boxCorners.Length; i++)
        {
            // Get direction of raycast
            Vector3 direction = boxCorners[i].position - playerLight.position;
            direction.Normalize();
            //Debug.DrawRay(boxCorners[i].position, direction * rayDistance, Color.cyan);

            // If ray hits wall that will project the shadow, store the hit point in a list
            if (Physics.Raycast(boxCorners[i].position, direction, out hit, rayDistance, layerMask))
            {
                Vector3 offset = hit.point - direction * 0.005f;
                currentRayPoints.Add(offset);
            }
        }

        if (!shadowCreated)
        {
            shadowCreated = true;
            shadow = Instantiate(testPrefab, GetAveragePosition(currentRayPoints), Quaternion.identity);
        }

        if (shadow != null)
        {
            // Smoothly move shadow into position
            shadow.transform.position = Vector3.SmoothDamp
            (
                shadow.transform.position,
                GetAveragePosition(currentRayPoints),
                ref velocity,
                time
            );
            //shadow.transform.position = GetAveragePosition(currentRayPoints);

            // Scale shadow sprite based on distance to shadow object
            float dist = Vector3.Distance(box.position, playerLight.position);
            shadow.transform.localScale = Vector3.one / dist;
        }
    }

    // Get center of all raycast hit points on the wall 
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
