using UnityEngine;
using System.Collections;

public class FourKeyPlatform : MonoBehaviour
{
    int positionIndex = 0;
    Vector3[] nextPosition = new Vector3[3];
    float moveSpeed = 0.5f;
    [SerializeField] GameObject[] lights;
    [SerializeField] Material lit;
    void Start()
    {
        Vector3 startPos = transform.position;
        for (int i = 0; i < nextPosition.Length; i++)
        {
            nextPosition[i] = startPos + Vector3.down * 2f;
            startPos = nextPosition[i];
        }
    }

    // Called from Shadow Burn
    public void NextThreshold()
    {
        StartCoroutine(MovePlatform());
    }

    IEnumerator MovePlatform()
    {
        Vector3 start = transform.position;
        Vector3 endPos = nextPosition[positionIndex];


        float elapsed = 0f;
        float duration = Vector3.Distance(start, endPos) / moveSpeed;

        // lerp to target
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap position
        transform.position = endPos;
        lights[positionIndex].GetComponent<Renderer>().material = lit;
        positionIndex++;
    }
}
