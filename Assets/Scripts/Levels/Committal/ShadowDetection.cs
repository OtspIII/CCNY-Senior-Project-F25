using UnityEngine;

public class ShadowDetection : MonoBehaviour
{
    Collider detectionCol, shadowCol; // Colliders for puzzle detection 
    [SerializeField] Collider sizeCheckCol; // Additional collider to use as max size for shadow sprite
    bool shadowDetected;
    public bool shadowIsInside;
    //Vector3 testCorner = Vector3.zero;
    Outline outline;

    void Start()
    {
        detectionCol = GetComponent<Collider>();
        outline = sizeCheckCol.transform.gameObject.GetComponent<Outline>();
        outline.OutlineWidth = 10f;
        outline.OutlineColor = Color.white;
        outline.enabled = false;
    }

    void Update()
    {
        if (shadowCol != null && shadowDetected)
        {
            shadowIsInside = ContainsCollider(detectionCol, shadowCol) &&
                             NoCornersDetected(sizeCheckCol, shadowCol);

            // Turn outline on 
            if (!outline.enabled) outline.enabled = true;

            // Set outline to cyan if at correct position
            if (shadowIsInside && outline.OutlineColor != Color.cyan)
                outline.OutlineColor = Color.cyan;
            // Set outline to white if at wrong position
            else if (!shadowIsInside && outline.OutlineColor != Color.white)
                outline.OutlineColor = Color.white;
        }
        else
        {
            if (shadowIsInside) shadowIsInside = false;
            if (outline.enabled) outline.enabled = false;
        }
    }

    bool ContainsCollider(Collider colA, Collider colB)
    {
        // Check whether shadow collider is completely within the outer box collider
        return colA.bounds.Contains(colB.bounds.min) &&
               colA.bounds.Contains(colB.bounds.max);
    }

    bool NoCornersDetected(Collider colA, Collider colB)
    {
        // Check whether shadow collider is not within the inner box collider
        Vector3 shadowEdges = colB.bounds.size / 2f;
        Vector3 shadowCenter = colB.bounds.center;

        for (int i = 0; i < 4; i++)
        {
            Vector3 corner;
            // Get all corners of the box collider using its center and size as reference
            corner.x = i % 2 == 0 ? shadowEdges.x : -shadowEdges.x;
            corner.y = i % 2 == 0 ? shadowEdges.y : -shadowEdges.y;
            corner.z = 0f; // We may need to change which axis is at 0 based on rotation of shadow 

            Vector3 point = shadowCenter + corner;

            // If any of the corners are within the inner box collider, return false
            if (colA.bounds.Contains(point))
            {
                return false;
            }
        }

        // If no corners are within the inner box collider, return true
        return true;
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(test, 0.01f);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Shadow"))
        {
            shadowCol = col;
            shadowDetected = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Shadow"))
        {
            shadowCol = null;
            shadowDetected = false;
        }
    }
}
