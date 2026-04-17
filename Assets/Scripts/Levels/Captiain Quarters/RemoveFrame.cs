using UnityEngine;

public class RemoveFrame : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball")) Destroy(gameObject);
    }
}
