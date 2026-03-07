using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ShadowRegen : MonoBehaviour
{
    [SerializeField] GameObject piecesPrefab;
    [SerializeField] List<GameObject> allPillars;
    [SerializeField] GameObject regenObj;
    bool test;

    void Update()
    {
        bool regen = true;
        for (int i = 0; i < allPillars.Count; i++)
        {
            if (allPillars[i].activeInHierarchy) regen = false;
        }
        if (regen && !test) StartCoroutine(SpawnObject());
    }
    IEnumerator SpawnObject()
    {
        test = true;

        regenObj.SetActive(false);

        Vector3 target = regenObj.transform.position;
        Vector3 start = target + Vector3.up * 6f;

        yield return new WaitForSeconds(1);

        regenObj.SetActive(true);
        foreach (GameObject p in allPillars)
        {
            p.SetActive(true);
        }

        float elapsed = 0f;
        float duration = 3f;

        while (elapsed < duration)
        {
            float time = elapsed / duration;
            regenObj.transform.position = Vector3.Lerp(start, target, time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        regenObj.transform.position = target;
        test = false;
    }
}
