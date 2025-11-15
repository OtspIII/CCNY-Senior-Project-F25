using Unity.VisualScripting;
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

    [Header("Lantern Colors: ")]

    public Material unlitMaterial;
    public Material litMaterial;
    bool flicker;
    Animator anim;

    private void Start()
    {
        if (activeLantern && LanternTravel.Instance != null)
        {
            if (!LanternTravel.Instance.ActivatedLanterns.Contains(this))
            {
                LanternTravel.Instance.ActivatedLanterns.Add(this);
            }
        }

        anim = GetComponent<Animator>();
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

        if (LanternTravel.Instance.currentLantern == this)
        {
            // Start flicker animation
            if (!flicker)
            {
                anim.SetTrigger("Flicker");
                flicker = true;
            }
        }
        else if (flicker)
        {
            flicker = false;
        }
        else if (activeLantern)
        {
            //Set To litMaterial:
            GetComponent<Renderer>().material = litMaterial;

        }
        else
        {
            //Set To unlitMaterial:
            GetComponent<Renderer>().material = unlitMaterial;
        }
    }

}
