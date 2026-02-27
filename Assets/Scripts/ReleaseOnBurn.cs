using UnityEngine;

public class ReleaseOnBurn : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject objectToDisable;

    public void Release()
    {
        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}
