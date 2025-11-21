using UnityEngine;
using System.Collections;

public class MoveLantern : MonoBehaviour
{
    [SerializeField] GameObject burnable;
    [SerializeField] Transform target;
    bool inPosition;

    void Update()
    {
        if (burnable == null && !inPosition) StartCoroutine(MoveToPosition(target.position));
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = target;

        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, endPos) / 5.0f;

        // lerp to target
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap position
        transform.position = endPos;
        inPosition = true;
    }
}
