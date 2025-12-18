using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] SunWheelController sunWheel;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (!PlayerMovement.player.item.activeInHierarchy)
                PlayerMovement.player.item.SetActive(true);

            if (sunWheel != null)
                sunWheel.UnlockAbility(SunSpike.SunSpikeType.Telescope);

            Destroy(this.gameObject);
        }
    }
}
