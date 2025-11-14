using UnityEngine;

public class BodyOscillate : MonoBehaviour
{
    [SerializeField] Vector3 rot;
    [SerializeField] float dist;

    void Start()
    {
        rot = transform.localEulerAngles;
    }
    void Update()
    {
        transform.localEulerAngles = rot;

        rot.y += dist * Time.deltaTime;
    }
}
