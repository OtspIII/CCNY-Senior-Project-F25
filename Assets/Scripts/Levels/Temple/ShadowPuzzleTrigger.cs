using UnityEngine;

public class ShadowPuzzleTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<DrawShadows>().shadowPuzzleActive = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<DrawShadows>().shadowPuzzleActive = false;
    }
}
