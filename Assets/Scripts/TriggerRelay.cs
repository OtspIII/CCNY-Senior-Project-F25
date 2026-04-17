using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    private Lantern lantern;

    private void Awake()
    {
        lantern = GetComponentInParent<Lantern>();
        if (lantern == null)
            Debug.LogError("No lantern found in parents", this);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (lantern == null) return;
        lantern.HandlePlayerEnter(col);
    }

    private void OnTriggerExit(Collider col)
    {
        if (lantern == null) return;
        lantern.HandlePlayerExit(col);
    }
}
