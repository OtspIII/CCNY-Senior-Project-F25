using UnityEngine;
using UnityEngine.AI;


public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    NavMeshAgent agent;
    public bool awakened;
    public bool tagged;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        if (agent.pathPending || !awakened) return;

        if (Vector3.Distance(transform.position, target.transform.position) >= 1.0f)
        {
            if (agent.isStopped) agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            if (!agent.isStopped) agent.isStopped = true;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //if (col.gameObject.tag == "Player" && !awakened) awakened = true;
    }
}
