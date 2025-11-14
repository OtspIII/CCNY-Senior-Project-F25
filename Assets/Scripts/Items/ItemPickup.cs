using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (!PlayerMovement.player.item.activeInHierarchy)
                PlayerMovement.player.item.SetActive(true);

            Destroy(this.gameObject);
        }
    }
}
