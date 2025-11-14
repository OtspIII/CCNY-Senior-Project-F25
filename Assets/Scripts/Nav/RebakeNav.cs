using UnityEngine;
using Unity.AI.Navigation;


public class RebakeNav : MonoBehaviour
{
    [SerializeField] NavMeshSurface surface;
    [SerializeField] GameObject attached, connectedBody;
    bool fin;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (connectedBody == null && !fin && rb.isKinematic)
            rb.isKinematic = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == attached && !fin)
        {
            surface.BuildNavMesh();
            if (!rb.isKinematic) rb.isKinematic = true;
            fin = true;
        }
    }
}
