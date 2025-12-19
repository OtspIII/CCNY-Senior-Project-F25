using UnityEngine;

public class ShowTutorialText : MonoBehaviour
{
    [SerializeField] GameObject showText;
    [SerializeField] bool onAtStart;
    void Start()
    {

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player") showText.SetActive(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player") showText.SetActive(false);
    }
}
