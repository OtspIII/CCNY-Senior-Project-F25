using UnityEngine;

public class ShadowOutline : MonoBehaviour
{
    Outline outline;
    void Start()
    {
        outline = GetComponent<Outline>();
        outline.OutlineColor = Color.white;
    }

    void Update()
    {

    }
}
