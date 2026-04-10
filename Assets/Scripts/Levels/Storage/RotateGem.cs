using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class RotateGem : GemInteractions
{
    public bool isHitting;
    //[SerializeField] LightReflection lightSource;
    [SerializeField] GameObject gem;
    [SerializeField] ShadowBurn shadowBurn;
    Coroutine currentCoroutine;
    bool flip = true;
    [SerializeField] Material unlit, lit;
    //[SerializeField] bool temp = true;

    public override void Start()
    {
        GetComponent<Renderer>().material = unlit;
    }

    public override void Update()
    {
        base.Update();

        isHitting = LightTool() != null;

        if (isHitting && !flip)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(Flip(Quaternion.Euler(0f, 269f, -45f)));
        }
        else if (!isHitting && flip)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(Flip(Quaternion.Euler(0f, 269f, 0f)));
        }

    }

    IEnumerator Flip(Quaternion target)
    {
        flip = !flip;
        if (flip) if (GetComponent<Renderer>().material != lit) GetComponent<Renderer>().material = lit;
        if (!flip) if (GetComponent<Renderer>().material != unlit) GetComponent<Renderer>().material = unlit;

        Quaternion startRotation = gem.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1.0f)
        {
            gem.transform.rotation = Quaternion.Lerp(startRotation, target, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gem.transform.rotation = target;
        shadowBurn.isChecking = flip;
        currentCoroutine = null;

    }
}
