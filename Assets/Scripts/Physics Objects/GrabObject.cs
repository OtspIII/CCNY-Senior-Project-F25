using UnityEngine;

public class GrabObject : MonoBehaviour
{
    PlayerMovement player;
    public Rigidbody rb;
    Collider col;
    public bool isGrabbed;
    bool applyMaterial;

    // No friction
    [SerializeField] PhysicsMaterial pm;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        player = PlayerMovement.player;
    }


    void Update()
    {
        // Remove this script reference from player if dropped
        if (transform.parent == null && isGrabbed)
        {
            isGrabbed = false;
            player.grab = null;
        }

        // Add friction when player is moving object
        if (isGrabbed)
        {
            if (applyMaterial) applyMaterial = false;
            col.material = null;
        }
        else if (col.material != applyMaterial)
        {
            applyMaterial = true;
            col.material = pm;
        }
    }


    void OnCollisionEnter(Collision col)
    {
        // Asign script to player on collision
        if (col.gameObject.tag == "Player" && player.grab == null) player.grab = this;
    }

    void OnCollisionExit(Collision col)
    {
        // Remove script when player exits
        if (col.gameObject.tag == "Player" && transform.parent == null && player.grab == this) player.grab = null;
    }
}
