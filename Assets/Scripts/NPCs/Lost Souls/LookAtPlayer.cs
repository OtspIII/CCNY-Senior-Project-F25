using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        Vector3 player = Camera.main.transform.position;
        transform.LookAt(new Vector3(player.x, transform.position.y, player.z));
        transform.Rotate(0f, 180f, 0f);
    }
}
