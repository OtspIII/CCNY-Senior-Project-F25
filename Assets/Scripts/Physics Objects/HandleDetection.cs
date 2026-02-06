using UnityEngine;

public class HandleDetection : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player") col.gameObject.GetComponent<PlayerMovement>().nearHandle = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player") col.gameObject.GetComponent<PlayerMovement>().nearHandle = false;
    }
}
