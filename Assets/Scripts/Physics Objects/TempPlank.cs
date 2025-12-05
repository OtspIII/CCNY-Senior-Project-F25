using UnityEngine;

public class TempPlank : MonoBehaviour
{
    bool startTimer;
    float targetTime = 1.0f;
    [SerializeField] GameObject floor;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (startTimer)
        {
            if (targetTime > 0f) targetTime -= Time.deltaTime;
            else if (!rb.isKinematic) rb.isKinematic = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == floor)
        {
            startTimer = true;
        }
    }
}
