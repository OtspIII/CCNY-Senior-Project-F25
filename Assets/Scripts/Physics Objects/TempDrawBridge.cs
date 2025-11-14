using UnityEngine;

public class TempDrawBridge : MonoBehaviour
{
    [SerializeField] GameObject rope;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(rope == null && rb.isKinematic) rb.isKinematic = false;
    }
}
