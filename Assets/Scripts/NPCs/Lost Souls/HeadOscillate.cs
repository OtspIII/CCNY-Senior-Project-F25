using UnityEngine;

public class HeadOscillate : MonoBehaviour
{
    [SerializeField] Vector3 pos;
    [SerializeField] float dist;

    void Start()
    {
        pos = transform.position;
    }
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + Mathf.Sin(Time.time * 3.0f) * dist * Time.deltaTime, transform.position.z);
    }
}
