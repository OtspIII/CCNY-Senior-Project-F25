using UnityEngine;
using Unity.AI.Navigation;


public class RebakeNav : MonoBehaviour
{
    [SerializeField] NavMeshSurface surface;
    [SerializeField] GameObject groundToHit, connectedBody;
    [SerializeField] GameObject obstacle;
    [SerializeField] FollowTarget soul;
    [SerializeField] GameObject soulText;
    bool fin;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (connectedBody == null && !fin && rb.isKinematic)
        {
            rb.isKinematic = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == groundToHit && !fin)
        {
            if (obstacle != null) obstacle.SetActive(false);
            surface.BuildNavMesh();
            if (!rb.isKinematic) rb.isKinematic = true;
            soul.awakened = true;
            transform.tag = "Untagged";
            if (soulText != null) Destroy(soulText);
            fin = true;
        }
    }
}
