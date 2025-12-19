using UnityEngine;
using MoreMountains.Feedbacks;

public class SunkenShipSection : MonoBehaviour
{
    [Header("MMF Players")]
    [SerializeField] private MMF_Player shipEnterAndExit;

    [Header("Settings")]
    [SerializeField] private bool playMoreThanOnce = false;

    private BoxCollider boxCollider;
    private bool hasPlayed = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("SunkenShipSection requires a BoxCollider component!");
        }
        
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && !playMoreThanOnce)
        {
            Debug.Log("Already played, skipping...");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected - playing feedback");
            shipEnterAndExit?.PlayFeedbacks();
            hasPlayed = true; 
        }
    }
}