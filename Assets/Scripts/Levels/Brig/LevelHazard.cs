using UnityEngine;

public class LevelHazard : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            GameManager.Instance.Player.checkpoint = true;
            //Debug.Log("YERRE");
        }
    }
}
