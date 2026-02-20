using UnityEngine;

public class DropLadder : MonoBehaviour
{
    [SerializeField] GameObject[] ropes;
    Rigidbody rb;
    float targetTime = 2.0f;
    bool inPosition;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (inPosition) return;

        if (!rb.isKinematic)
        {
            if (targetTime > 0f)
            {
                targetTime -= Time.deltaTime;
            }
            else
            {
                rb.isKinematic = true;
                inPosition = true;
                return;
            }
        }

        if (ropes[0] == null && ropes[1] == null && rb.isKinematic) rb.isKinematic = false;

    }
}
