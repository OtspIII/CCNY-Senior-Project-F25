using UnityEngine;

public class FlourBagLauncher : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTrigger = "FallIntoPot";
    [SerializeField] private bool plankDestroyed = false;

    public void SetPlankAsDestroyed()
    {
        plankDestroyed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (plankDestroyed)
            {
                TriggerSequence();
            }
        }
    }

    private void TriggerSequence()
    {
        animator.SetTrigger(animationTrigger);
    }

}
