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
    [SerializeField] Light lanternLight;


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
        if (activeLantern)
            GetComponent<Renderer>().material = litMaterial;
        else
            GetComponent<Renderer>().material = unlitMaterial;
        //lanternLight = GetComponent<Light>();
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
            if (!lanternLight.enabled) lanternLight.enabled = true;
            GetComponent<Renderer>().material = litMaterial;

        }
        else
        {
            //Set To unlitMaterial:
            if (lanternLight.enabled) lanternLight.enabled = false;
            GetComponent<Renderer>().material = unlitMaterial;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!activeLantern) return;

        if (col.gameObject.tag == "Player" && PlayerMovement.player.lantern == null)
        {
            PlayerMovement.player.lantern = this;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (!activeLantern) return;

        if (col.gameObject.tag == "Player" && PlayerMovement.player.lantern != null)
        {
            PlayerMovement.player.lantern = null;
        }
    }

}
