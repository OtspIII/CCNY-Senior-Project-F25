using UnityEngine;

public class BrigCheckpoint : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    bool checkpointSet;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && !checkpointSet)
        {
            PlayerMovement.player.startPos = PlayerMovement.player.transform.position + offset;
            checkpointSet = true;
        }
    }
}
