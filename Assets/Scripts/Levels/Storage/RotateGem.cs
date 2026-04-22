using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class RotateGem : GemInteractions
{
    public bool isHitting;
    //[SerializeField] LightReflection lightSource;
    [SerializeField] GameObject gem, wall;
    Coroutine currentCoroutine;
    [SerializeField] Vector3 target;
    Vector3 startPos;
    float maxDist;
    bool moveWall;
    bool flip = true;
    [SerializeField] Material unlit, lit;
    //[SerializeField] bool temp = true;

    public override void Start()
    {
        GetComponent<Renderer>().material = unlit;
        startPos = wall.transform.position;
        target = startPos + Vector3.up * 3.5f;
        maxDist = Vector3.Distance(startPos, target);
    }

    public override void Update()
    {
        base.Update();

        isHitting = LightTool() != null;

        if (isHitting && !moveWall)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(MoveWall(target));//StartCoroutine(Flip(Quaternion.Euler(0f, 269f, -45f)));
        }
        else if (!isHitting && moveWall)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(MoveWall(startPos));//StartCoroutine(Flip(Quaternion.Euler(0f, 269f, 0f)));
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
        currentCoroutine = null;

    }

    IEnumerator MoveWall(Vector3 target)
    {
        moveWall = !moveWall;
        if (moveWall) if (GetComponent<Renderer>().material != lit) GetComponent<Renderer>().material = lit;
        if (!moveWall) if (GetComponent<Renderer>().material != unlit) GetComponent<Renderer>().material = unlit;

        Vector3 start = wall.transform.position;
        Vector3 endPos = target;

        float elapsed = 0f;
        float currentDistance = Vector3.Distance(wall.transform.position, target);
        float duration = map(currentDistance, 0f, maxDist, 0f, 2.0f);

        // lerp to target
        while (elapsed < duration)
        {
            wall.transform.position = Vector3.Lerp(start, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap position
        wall.transform.position = endPos;
        currentCoroutine = null;
    }

    float map(float value, float minA, float maxA, float minB, float maxB)
    {
        float range = maxA - minA;
        float valuePercent = (value - minA) / range;

        float newRange = maxB - minB;

        return valuePercent * newRange + minB;
    }
}
