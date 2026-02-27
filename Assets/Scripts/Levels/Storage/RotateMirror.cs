using System.Collections;
using UnityEngine;

public class RotateMirror : MonoBehaviour
{
    [SerializeField] Quaternion[] rotations;
    [Tooltip("Sets target for next rotation.")]
    [SerializeField] int queuedRotation = 1;
    bool rotationInProgress;
    [SerializeField] bool playerInRange;
    [SerializeField] KeyCode actionKey;

    void Update()
    {
        if (Input.GetKeyDown(actionKey) && !rotationInProgress && playerInRange)
            StartCoroutine(Turn(rotations[queuedRotation]));
    }

    IEnumerator Turn(Quaternion target)
    {
        rotationInProgress = true;

        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1.0f)
        {
            transform.rotation = Quaternion.Lerp(startRotation, target, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = target;

        if (queuedRotation < rotations.Length - 1) queuedRotation++;
        else queuedRotation = 0;

        rotationInProgress = false;

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.transform.CompareTag("Player")) playerInRange = false;
    }
}
