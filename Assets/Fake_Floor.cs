using UnityEngine;

public class Fake_Floor : MonoBehaviour
{
    void OnCollisionEnter(Collision collider)
    {
        Debug.Log("Works");
        if (collider.gameObject.tag == "Player")
        {
            this.gameObject.SetActive(false);
        }
    }
}
