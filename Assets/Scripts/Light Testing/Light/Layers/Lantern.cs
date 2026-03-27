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
    [SerializeField] private Color unactivatedOutlineColor = Color.white;
    [SerializeField] private Color activatedOutlineColor = Color.yellow;
    [SerializeField] private float unactivatedOutlineThickness = 2f;
    [SerializeField] private float activatedOutlineThickness = 5f;
    [SerializeField] private float proximityFadeDuration = 0.5f;
    private float currentProximityAlpha = 0f;
    bool flicker;
    Animator anim;
    [SerializeField] Light lanternLight;
    private Outline outline;
    private Renderer objectRenderer;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        objectRenderer = GetComponent<Renderer>();
        if (outline != null)
        {
            outline.OutlineColor = unactivatedOutlineColor;
            outline.OutlineWidth = 0f; // Initially hidden unless in FPV or similar
        }
    }

    private void OnEnable()
    {
        PromptTrigger.OnFPVToggle += HandleFPVChange;
    }

    private void OnDisable()
    {
        PromptTrigger.OnFPVToggle -= HandleFPVChange;
    }

    private bool isFPVActive;

    private void HandleFPVChange(bool isFPVActive)
    {
        this.isFPVActive = isFPVActive;
        UpdateOutlineState();
    }

    private void UpdateOutlineState()
    {
        if (outline == null) return;

        bool showByFPV = isFPVActive;
        float distToPlayer = (LanternTravel.Instance != null && PlayerMovement.player != null && lanternCore != null)
            ? Vector3.Distance(PlayerMovement.player.transform.position, lanternCore.position)
            : float.MaxValue;
        
        bool isInProximity = LanternTravel.Instance != null && distToPlayer <= LanternTravel.Instance.entryProximity;
        bool targetProximityState = activeLantern && isInProximity;

        // Animate the alpha value
        float targetAlpha = targetProximityState ? 1f : 0f;
        if (proximityFadeDuration > 0.001f)
        {
            currentProximityAlpha = Mathf.MoveTowards(currentProximityAlpha, targetAlpha, Time.deltaTime / proximityFadeDuration);
        }
        else
        {
            currentProximityAlpha = targetAlpha;
        }

        if (showByFPV)
        {
            if (activeLantern)
            {
                outline.OutlineColor = activatedOutlineColor;
                outline.OutlineWidth = activatedOutlineThickness;
            }
            else
            {
                // Unlit: show baseline width (will be animated in Update based on activation progress)
                outline.OutlineWidth = unactivatedOutlineThickness;
            }
        }
        else if (currentProximityAlpha > 0f)
        {
            if (activeLantern)
            {
                outline.OutlineColor = activatedOutlineColor;
                outline.OutlineWidth = activatedOutlineThickness * currentProximityAlpha;
            }
            else
            {
                // If it becomes unlit while in proximity, we might still want the fade out
                outline.OutlineWidth = unactivatedOutlineThickness * currentProximityAlpha;
            }
        }
        else
        {
            outline.OutlineWidth = 0f;
        }
    }

    private void Start()
    {
        if (activeLantern && LanternTravel.Instance != null)
        {
            if (activeLantern) LanternTravel.Instance?.RegisterActivatedLantern(this);
        }

        anim = GetComponent<Animator>();
        if (activeLantern)
        {
            if (objectRenderer != null)
                objectRenderer.material = litMaterial;
        }
        else
        {
            if (objectRenderer != null)
                objectRenderer.material = unlitMaterial;
        }
    }


    private void Update()
    {
        UpdateOutlineState();

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

        if (outline != null && !activeLantern)
        {
            float t = currentActivation / Mathf.Max(0.001f, activationTime);
            outline.OutlineColor = Color.Lerp(unactivatedOutlineColor, activatedOutlineColor, t);
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
            if (!lanternLight.enabled) lanternLight.enabled = true;
            if (objectRenderer != null)
                objectRenderer.material = litMaterial;
        }
        else
        {
            //Set To unlitMaterial:
            if (lanternLight.enabled) lanternLight.enabled = false;
            if (objectRenderer != null)
                objectRenderer.material = unlitMaterial;

            if (outline != null && outline.OutlineWidth > 0f && isFPVActive)
            {
                float t = currentActivation / Mathf.Max(0.001f, activationTime);
                outline.OutlineColor = Color.Lerp(unactivatedOutlineColor, activatedOutlineColor, t);
                outline.OutlineWidth = Mathf.Lerp(unactivatedOutlineThickness, activatedOutlineThickness, t);
            }
        }
    }
}
