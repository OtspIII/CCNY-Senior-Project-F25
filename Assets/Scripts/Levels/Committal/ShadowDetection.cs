using UnityEngine;

public class ShadowDetection : MonoBehaviour
{
    Collider detectionCol, shadowCol;
    [SerializeField] Collider sizeCheckCol;
    bool shadowDetected;
    public bool shadowIsInside;
    //Vector3 testCorner = Vector3.zero;
    void Start()
    {
        detectionCol = GetComponent<Collider>();
    }

    void Update()
    {
        if (shadowDetected)
        {
            shadowIsInside = ContainsCollider(detectionCol, shadowCol) && NoCornersDetected(sizeCheckCol, shadowCol);
        }
        else
        {
            if (shadowIsInside) shadowIsInside = false;
        }
    }

    bool ContainsCollider(Collider colA, Collider colB)
    {
        return colA.bounds.Contains(colB.bounds.min) &&
           colA.bounds.Contains(colB.bounds.max);
    }

    bool NoCornersDetected(Collider colA, Collider colB)
    {
        Vector3 shadowEdges = colB.bounds.size / 2f;
        Vector3 shadowCenter = colB.bounds.center;

        for (int i = 0; i < 4; i++)
        {
            Vector3 corner;
            corner.x = i % 2 == 0 ? shadowEdges.x : -shadowEdges.x;
            corner.y = i % 2 == 0 ? shadowEdges.y : -shadowEdges.y;
            corner.z = 0f;

            Vector3 point = shadowCenter + corner;

            if (colA.bounds.Contains(point))
            {
                return false;
            }
        }

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
