using UnityEngine;

public class ShowTutorialText : MonoBehaviour
{
    [SerializeField] GameObject tutorial;

    void Update()
    {
        if (gameObject.GetComponent<Lantern>().activeLantern && !tutorial.activeInHierarchy)
        {
            tutorial.SetActive(true);
        }
    }
}
