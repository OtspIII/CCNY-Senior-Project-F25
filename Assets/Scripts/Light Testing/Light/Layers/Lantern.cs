using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lantern : MonoBehaviour
{
    [Header("Lantern Movement Position: ")]
    public Transform lanternCore;

    public Collider aimCollider;
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
    Light light;


    private void Start()
    {
        if (activeLantern && LanternTravel.Instance != null)
        {
            if (activeLantern) LanternTravel.Instance?.RegisterActivatedLantern(this);
        }

        anim = GetComponent<Animator>();
        light = GetComponent<Light>();
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
                if (!activeLantern)
                {
                    activeLantern = true;
                    LanternTravel.Instance?.RegisterActivatedLantern(this);
                }
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
                if (anim != null) 
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
            if (!light.enabled) light.enabled = true;
            GetComponent<Renderer>().material = litMaterial;

        }
        else
        {
            //Set To unlitMaterial:
            if (light.enabled) light.enabled = false;
            GetComponent<Renderer>().material = unlitMaterial;
        }
    }
    
    public void HandlePlayerEnter(Collider col)
    {
        if (!activeLantern) return;

        if (col.CompareTag("Player") && PlayerMovement.player.lantern == null)
            PlayerMovement.player.lantern = this;
    }

    public void HandlePlayerExit(Collider col)
    {
        if (!activeLantern) return;

        if (col.CompareTag("Player") && PlayerMovement.player.lantern == this)
            PlayerMovement.player.lantern = null;
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
