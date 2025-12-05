using UnityEngine;

public class LevelHazard : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            PlayerMovement.player.checkpoint = true;
            Debug.Log("YERRE");
        }
    }
}
