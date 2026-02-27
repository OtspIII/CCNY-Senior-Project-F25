using UnityEngine;

public class FlourBagLauncher : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float upwardForce;
    [SerializeField] private float forwardForce;
    [SerializeField] private Transform launchDirectionReference;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        Rigidbody playerRb = collision.rigidbody;
        if (playerRb != null && playerRb.linearVelocity.y > 0.5f) return;
        
        if (rb == null) return;
        
        Vector3 forwardDir = launchDirectionReference != null ? launchDirectionReference.forward : transform.forward;
        
        forwardDir.y = 0f;
        forwardDir.Normalize();
        
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        rb.linearVelocity = v;
        
        Vector3 impulse = Vector3.up * upwardForce + forwardDir * forwardForce;
        rb.AddForce(impulse, ForceMode.Impulse);
    }
}
