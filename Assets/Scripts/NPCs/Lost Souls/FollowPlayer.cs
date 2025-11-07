using UnityEngine;
using UnityEngine.AI;


public class FollowPlayer : MonoBehaviour
{
    PlayerMovement player;
    NavMeshAgent agent;
    bool awakened;

    void Start()
    {
        player = PlayerMovement.player;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        MoveToPlayer();
    }

    void MoveToPlayer()
    {
        if (agent.pathPending) return;

        if (Vector3.Distance(transform.position, player.transform.position) >= 3.0f && awakened)
        {
            if (agent.isStopped) agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
        else
        {
            if (!agent.isStopped) agent.isStopped = true;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && !awakened) awakened = true;
    }
}
