using UnityEngine;

public class TempDrawBridge : MonoBehaviour
{
    [SerializeField] GameObject rope;
    [SerializeField] GameObject ground;
    bool foo;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rope == null && rb.isKinematic && !foo) rb.isKinematic = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == ground)
        {
            rb.isKinematic = true;
            foo = true;
        }
    }
}
