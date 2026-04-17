using UnityEngine;

public class LightCandle : MonoBehaviour
{
    [SerializeField] GameObject fuse;
    void Update()
    {
        if (fuse == null && !transform.GetChild(0).gameObject.activeInHierarchy)
            transform.GetChild(0).gameObject.SetActive(true);
    }
}
