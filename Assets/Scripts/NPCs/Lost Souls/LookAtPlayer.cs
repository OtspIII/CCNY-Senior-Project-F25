using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(GameManager.Instance.Player.transform);
    }
}
