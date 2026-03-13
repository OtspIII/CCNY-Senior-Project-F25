using UnityEngine;

public class BrigCheckpoint : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    bool checkpointSet;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" && !checkpointSet)
        {
            GameManager.Instance.Player.startPos =
            GameManager.Instance.Player.transform.position + offset;
            checkpointSet = true;
        }
    }
}
