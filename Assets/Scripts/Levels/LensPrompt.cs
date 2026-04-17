using UnityEngine;

public class LensPrompt : MonoBehaviour
{
    ProjectorTraversal projector;
    public GameObject text;
    Outline outline;

    void Start()
    {
        outline = GetComponentInChildren<Outline>();
        outline.enabled = false;
        text.SetActive(false);
    }

    void Update()
    {
        if (projector != null)
        {
            if (!text.activeInHierarchy && !projector.isInsideProjector)
            {
                text.SetActive(true);
                outline.enabled = true;
            }
            if (text.activeInHierarchy && projector.isInsideProjector)
            {
                text.SetActive(false);
                outline.enabled = false;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            text.SetActive(true);
            outline.enabled = true;
            projector = col.gameObject.GetComponent<ProjectorTraversal>();
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            text.SetActive(false);
            outline.enabled = false;
            projector = null;
        }
    }
}
