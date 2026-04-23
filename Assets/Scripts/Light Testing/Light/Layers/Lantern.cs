using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

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
    public float detectionRadius = 5f;
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
    [SerializeField] private GameObject inputCanvas;
    [SerializeField] private TMP_Text buttonPromptText;
    bool flicker;
    Animator anim;
    [SerializeField] Light lanternLight;
    PlayerMovement player;
    bool playerDetected;
    private bool isPlayerInside = false;


    private void Start()
    {
        player = GameManager.Instance.Player;
        if (activeLantern && GameManager.Instance.LanternTravel != null)
        {
            if (activeLantern) GameManager.Instance.LanternTravel?.RegisterActivatedLantern(this);
        }

        if (inputCanvas != null)
            inputCanvas.SetActive(false);

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
        if (GameManager.Instance.LanternTravel == null) return;

        // Radius-based player detection
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            bool wasPlayerInside = isPlayerInside;
            isPlayerInside = distToPlayer <= detectionRadius;

            if (isPlayerInside && !wasPlayerInside)
            {
                // Player just entered the radius
                if (activeLantern && player.lantern == null)
                {
                    playerDetected = true;
                    player.lantern = this;
                }
            }
            else if (!isPlayerInside && wasPlayerInside)
            {
                // Player just left the radius
                if (playerDetected || player.lantern == this)
                {
                    playerDetected = false;
                    if (player.lantern == this) player.lantern = null;
                }
                if (inputCanvas != null) inputCanvas.SetActive(false);
            }

            // Continuous checks while inside
            if (isPlayerInside)
            {
                if (activeLantern)
                {
                    if (player.lantern == null)
                    {
                        playerDetected = true;
                        player.lantern = this;
                    }

                    if (inputCanvas != null && !inputCanvas.activeSelf)
                    {
                        inputCanvas.SetActive(true);
                    }

                    if (buttonPromptText != null)
                    {
                        var lanternTravel = GameManager.Instance.LanternTravel;
                        if (lanternTravel != null && lanternTravel.isInsideLantern && lanternTravel.currentLantern == this)
                        {
                            buttonPromptText.text = "Left-Click";
                        }
                        else
                        {
                            buttonPromptText.text = "Q";
                        }
                    }
                }
                else
                {
                    // Lantern not active, should not be assigned as player's current lantern for travel
                    if (player.lantern == this) player.lantern = null;
                    if (inputCanvas != null && inputCanvas.activeSelf) inputCanvas.SetActive(false);
                }
            }
        }

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
}
