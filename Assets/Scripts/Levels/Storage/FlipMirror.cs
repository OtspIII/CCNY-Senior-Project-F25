using System.Collections;
using UnityEngine;

public class FlipMirror : MonoBehaviour
{
    public bool isHitting;
    [SerializeField] GameObject lightSource;
    [SerializeField] GameObject mirror;
    Coroutine currentCoroutine;
    bool flip;

    void Update()
    {
        isHitting = lightSource.activeInHierarchy && lightSource.GetComponent<LightReflection>().gemHit;

        if (isHitting && !flip)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(Flip(Quaternion.Euler(-5f, 180f, 0f)));
        }
        else if (!isHitting && flip)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(Flip(Quaternion.Euler(-5f, 0f, 0f)));
        }

    }

    IEnumerator Flip(Quaternion target)
    {
        flip = !flip;
        Quaternion startRotation = mirror.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1.0f)
        {
            mirror.transform.rotation = Quaternion.Lerp(startRotation, target, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mirror.transform.rotation = target;
        currentCoroutine = null;

    }

}
