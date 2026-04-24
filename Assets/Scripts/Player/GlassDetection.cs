using UnityEngine;

public class GlassDetection : MonoBehaviour
{
    PlayerMovement player;
    void Start()
    {
        player = GameManager.Instance.Player;
    }

    void Update()
    {
        if (player != GameManager.Instance.Player) player = GameManager.Instance.Player;
        if (player == null) return;

        transform.position = player.transform.position - new Vector3(0f, 0.2f, 0f) + new Vector3(player.camOrientation.forward.x, player.transform.forward.y, player.camOrientation.forward.z) * 3f;
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("YERR");
    }
}
