using UnityEngine;

public class Lantern : MonoBehaviour
{
    [Header("Lantern Movement Position: ")]
    public Transform lanternCore;
    [Space]

    [Header("Adjacent Lanterns: ")]
    public Lantern nextLantern;
    public Lantern previousLantern;
    [Space]

    [Header("Lantern Activation Settings:")]
    public float activationTime = 2f;
    public enum ActivationMode { ResetWhenNotHit, PersistAfterHit }
    public ActivationMode activationMode = ActivationMode.ResetWhenNotHit;
    [Space]

    [Header("Is The Lantern Active?")]
    public bool activeLantern = false;
    [Space]


    [Header("Current Info: ")]
    public float currentActivation = 0f;
    public int hitsThisFrame = 0;

    private void Start()
    {
        if (activeLantern && LanternTravel.Instance != null)
        {
            if (!LanternTravel.Instance.ActivatedLanterns.Contains(this))
            {
                LanternTravel.Instance.ActivatedLanterns.Add(this);
            }
        }
    }


    private void Update()
    {
        // Only count down while being hit
        if (hitsThisFrame > 0)
        {
            currentActivation += Time.deltaTime;
            hitsThisFrame = 0;

            if (currentActivation >= activationTime)
            {
                activeLantern = true;
                currentActivation = activationTime;
            }
        }
        else
        {
            if (!activeLantern)
            {
                if (activationMode == ActivationMode.ResetWhenNotHit)
                {
                    currentActivation = 0f;
                }
            }
        }
    }

}
