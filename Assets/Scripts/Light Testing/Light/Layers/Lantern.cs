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
    public Material defaultMaterial;
    public Color unlitColor = Color.grey;
    public Color litColor = Color.white;
    public Color closeColorMaterial = Color.yellow;
    public Color insideColorMaterial = Color.cyan;
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Glow Settings: ")]
    [SerializeField] private float unlitGlow = 0f;
    [SerializeField] private float litGlow = 1f;
    [SerializeField] private float closeGlow = 2f;
    [SerializeField] private float insideGlow = 4f;
    [SerializeField] private string emissionPropertyName = "_EmissionColor";

    [Header("Outline Settings: ")]
    [SerializeField] private Outline outline;
    [SerializeField] private Color insideOutlineColor = Color.cyan;
    [SerializeField] private Color closeColor = Color.yellow;
    [SerializeField] private Color litOutlineColor = Color.white;
    [SerializeField] private Color offColor = Color.white;
    [SerializeField] private float insideOutlineWidth = 8f;
    [SerializeField] private float closeWidth = 5f;
    [SerializeField] private float litOutlineWidth = 2f;
    [SerializeField] private float offWidth = 0f;
    [SerializeField] private float lerpTime = 0.5f;

    [SerializeField] private GameObject inputCanvas;
    [SerializeField] private TMP_Text buttonPromptText;
    [SerializeField] private GameObject lightModel;
    bool flicker;
    Animator anim;
    [SerializeField] Light lanternLight;
    PlayerMovement player;
    bool playerDetected;
    private bool isPlayerInside = false;
    private Material runtimeMaterial;
    private Renderer lanternRenderer;


    private void Start()
    {
        player = GameManager.Instance.Player;
        lanternRenderer = GetComponent<Renderer>();
        
        if (activeLantern && GameManager.Instance.LanternTravel != null)
        {
            if (activeLantern) GameManager.Instance.LanternTravel?.RegisterActivatedLantern(this);
        }

        if (inputCanvas != null)
            inputCanvas.SetActive(false);

        anim = GetComponent<Animator>();

        if (defaultMaterial != null)
        {
            runtimeMaterial = new Material(defaultMaterial);
            lanternRenderer.material = runtimeMaterial;
            
            // Set initial color and glow based on state
            Color initialColor = activeLantern ? litColor : unlitColor;
            float initialGlow = activeLantern ? litGlow : unlitGlow;
            
            runtimeMaterial.SetColor(colorPropertyName, initialColor);
            
            if (runtimeMaterial.HasProperty(emissionPropertyName))
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
                runtimeMaterial.SetColor(emissionPropertyName, initialColor * initialGlow);
            }
        }
        //lanternLight = GetComponent<Light>();

        if (lightModel != null)
            lightModel.SetActive(false);
    }


    private enum LanternState { Unlit, Lit, Close, Inside }

    private void Update()
    {
        if (player != GameManager.Instance.Player) player = GameManager.Instance.Player;
        if (GameManager.Instance.LanternTravel == null) return;

        HandlePlayerDetection();
        UpdateActivation();
        UpdateLightModel();
        UpdateVisuals();
    }

    private void HandlePlayerDetection()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool wasPlayerInside = isPlayerInside;
        isPlayerInside = distToPlayer <= detectionRadius;

        if (isPlayerInside && !wasPlayerInside)
        {
            if (activeLantern && player.lantern == null)
            {
                playerDetected = true;
                player.lantern = this;
            }
        }
        else if (!isPlayerInside && wasPlayerInside)
        {
            if (playerDetected || player.lantern == this)
            {
                playerDetected = false;
                if (player.lantern == this) player.lantern = null;
            }
            if (inputCanvas != null) inputCanvas.SetActive(false);
        }

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
                    inputCanvas.SetActive(true);

                if (buttonPromptText != null)
                {
                    var lanternTravel = GameManager.Instance.LanternTravel;
                    if (lanternTravel != null && lanternTravel.isInsideLantern && lanternTravel.currentLantern == this)
                        buttonPromptText.text = "Left-Click";
                    else
                        buttonPromptText.text = "Q";
                }
            }
            else
            {
                if (player.lantern == this) player.lantern = null;
                if (inputCanvas != null && inputCanvas.activeSelf) inputCanvas.SetActive(false);
            }
        }
    }

    private void UpdateActivation()
    {
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
        else if (!activeLantern && activationMode == ActivationMode.ResetWhenNotHit)
        {
            currentActivation = 0f;
        }
    }

    private void UpdateLightModel()
    {
        var lanternTravel = GameManager.Instance.LanternTravel;
        if (lanternTravel.currentLantern == this)
        {
            if (!flicker)
            {
                if (anim != null) anim.SetTrigger("Flicker");
                flicker = true;
            }

            if (lightModel != null)
            {
                bool isInsideThisLantern = lanternTravel.isInsideLantern;
                if (lightModel.activeSelf != isInsideThisLantern)
                    lightModel.SetActive(isInsideThisLantern);
            }
        }
        else
        {
            if (flicker) flicker = false;
            if (lightModel != null && lightModel.activeSelf)
                lightModel.SetActive(false);
        }
    }

    private void UpdateVisuals()
    {
        LanternState state = GetCurrentState();
        float activationRatio = currentActivation / activationTime;

        // Determine target visual values
        Color targetMaterialColor;
        float targetGlow;
        Color targetOutlineColor;
        float targetOutlineWidth;

        switch (state)
        {
            case LanternState.Inside:
                targetMaterialColor = insideColorMaterial;
                targetGlow = insideGlow;
                targetOutlineColor = insideOutlineColor;
                targetOutlineWidth = insideOutlineWidth;
                break;
            case LanternState.Close:
                targetMaterialColor = closeColorMaterial;
                targetGlow = closeGlow;
                targetOutlineColor = closeColor;
                targetOutlineWidth = closeWidth;
                break;
            case LanternState.Lit:
                targetMaterialColor = litColor;
                targetGlow = litGlow;
                targetOutlineColor = litOutlineColor;
                targetOutlineWidth = litOutlineWidth;
                break;
            case LanternState.Unlit:
            default:
                targetMaterialColor = Color.Lerp(unlitColor, litColor, activationRatio);
                targetGlow = Mathf.Lerp(unlitGlow, litGlow, activationRatio);
                targetOutlineColor = Color.Lerp(offColor, litOutlineColor, activationRatio);
                targetOutlineWidth = Mathf.Lerp(offWidth, litOutlineWidth, activationRatio);
                break;
        }

        // Apply Light component state
        if (lanternLight != null)
        {
            bool shouldBeOn = activeLantern;
            if (lanternLight.enabled != shouldBeOn)
                lanternLight.enabled = shouldBeOn;
        }

        // Lerp Material
        if (runtimeMaterial != null)
        {
            float t = (lerpTime > 0) ? Time.deltaTime / lerpTime : 1f;
            
            Color currentColor = runtimeMaterial.GetColor(colorPropertyName);
            Color nextColor = Color.Lerp(currentColor, targetMaterialColor, t);
            runtimeMaterial.SetColor(colorPropertyName, nextColor);

            if (runtimeMaterial.HasProperty(emissionPropertyName))
            {
                Color currentEmission = runtimeMaterial.GetColor(emissionPropertyName);
                Color targetEmissionColor = targetMaterialColor * targetGlow;
                Color nextEmissionColor = Color.Lerp(currentEmission, targetEmissionColor, t);
                runtimeMaterial.SetColor(emissionPropertyName, nextEmissionColor);
            }
        }

        // Lerp Outline
        if (outline != null)
        {
            float t = (lerpTime > 0) ? Time.deltaTime / lerpTime : 1f;
            outline.OutlineColor = Color.Lerp(outline.OutlineColor, targetOutlineColor, t);
            outline.OutlineWidth = Mathf.Lerp(outline.OutlineWidth, targetOutlineWidth, t);
        }
    }

    private LanternState GetCurrentState()
    {
        var lanternTravel = GameManager.Instance.LanternTravel;
        if (lanternTravel != null && lanternTravel.isInsideLantern && lanternTravel.currentLantern == this)
            return LanternState.Inside;
        
        if (activeLantern)
            return isPlayerInside ? LanternState.Close : LanternState.Lit;
        
        return LanternState.Unlit;
    }
}
