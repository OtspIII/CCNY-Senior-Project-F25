using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class Slider : MonoBehaviour
{
    [Header("Railing Points")] 
    [SerializeField] private Transform railStart;
    [SerializeField] private Transform railEnd;

    [Header("Physics")] 
    [SerializeField] private float friction = 0.3f;
    [SerializeField] float stopThreshold = 0.02f;
    [SerializeField] Rigidbody ingredient;
    
    bool isSliding = false;
    private float t = 0f;
    float velocity = 0f;
    
    void Update()
    {
        if (!isSliding) return;
        if (t >= 1f) return;
        
        Vector3 railDir = (railEnd.position - railStart.position).normalized;
        float gravity = Physics.gravity.magnitude;
        float slope = Vector3.Dot(railDir, Vector3.down * gravity);
        
        velocity += (slope - friction) * Time.deltaTime;
        velocity = Mathf.Max(velocity, 0f, 5f);
        
        float railLength = Vector3.Distance(railStart.position, railEnd.position);
        t += (velocity * Time.deltaTime)/railLength;
        t = Mathf.Clamp01(t);
        
        transform.position = Vector3.Lerp(railStart.position, railEnd.position, t);

        if (t > 1f || Vector3.Distance(transform.position, railEnd.position) < stopThreshold)
        {
            transform.position = railEnd.position;
            isSliding = false;

            if (ingredient != null)
            {
                ingredient.transform.SetParent(null);
                ingredient.isKinematic = false;
            }
        }
    }

    public void StartSlide()
    {
        Debug.Log("StartSlide called!");
        isSliding = true;
        velocity = 0f;
        t = 0f;
    }
}
