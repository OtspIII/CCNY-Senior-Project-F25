using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JuicyLock : MonoBehaviour
{
    public Image Lock;

    public Vector3 squashScale = new Vector3(1.1f, 0.8f, 1); // squashed down
    public Vector3 finalScale = new Vector3(1f, 1f, 1);  // bigger than normal
    public float squashTime = 0.2f;
    public float popTime = 0.3f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale; // store original size
    }

    public void PopOut()
    {
        StopAllCoroutines();
        StartCoroutine(PopOutCoroutine());
    }

    IEnumerator PopOutCoroutine()
    {
        yield return new WaitForSeconds(.3f);

        Lock.enabled = true;

        float elapsed = 0f;

        //Squash down
        while (elapsed < squashTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / squashTime);
            transform.localScale = Vector3.Lerp(originalScale, squashScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = squashScale;

        //Pop out to final bigger size
        elapsed = 0f;
        while (elapsed < popTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / popTime);
            transform.localScale = Vector3.Lerp(squashScale, finalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = finalScale; // stay bigger than the other spikes
    }
}
