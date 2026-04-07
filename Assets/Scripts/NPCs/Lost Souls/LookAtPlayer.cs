using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(PlayerMovement.player.transform);
    }
}
