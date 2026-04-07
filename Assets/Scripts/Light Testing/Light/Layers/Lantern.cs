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
    [SerializeField] Light lanternLight;
    PlayerMovement player;


    private void Start()
    {
        player = GameManager.Instance.Player;
        if (activeLantern && GameManager.Instance.LanternTravel != null)
        {
            if (activeLantern) GameManager.Instance.LanternTravel?.RegisterActivatedLantern(this);
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
        if (player != GameManager.Instance.Player) player = GameManager.Instance.Player;
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
                    GameManager.Instance.LanternTravel?.RegisterActivatedLantern(this);
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

        if (GameManager.Instance.LanternTravel.currentLantern == this)
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

    public void HandlePlayerEnter(Collider col)
    {
        if (!activeLantern) return;

        if (col.CompareTag("Player") && player.lantern == null)
            player.lantern = this;
    }

    public void HandlePlayerExit(Collider col)
    {
        if (!activeLantern) return;

        if (col.CompareTag("Player") && player.lantern == this)
            player.lantern = null;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!activeLantern) return;

        if (col.gameObject.tag == "Player" && player.lantern == null)
        {
            player.lantern = this;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (!activeLantern) return;

        if (col.gameObject.tag == "Player" && player.lantern != null)
        {
            player.lantern = null;
        }
    }

}
