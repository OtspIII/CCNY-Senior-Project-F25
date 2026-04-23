using UnityEngine;

public class FollowBook : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] Vector3 offset;

    void Start()
    {
        offset = target.transform.position - transform.position;
    }

    void Update()
    {
        transform.position = target.transform.position - offset;
    }
}
