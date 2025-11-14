using System.Collections.Generic;
using UnityEngine;

public class WhaleGutsDoor : MonoBehaviour
{
    void Update()
    {
        if (transform.childCount == 2) Destroy(gameObject);
    }
}
