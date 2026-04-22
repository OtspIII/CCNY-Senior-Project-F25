using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements.Experimental;

public class Burnable : MonoBehaviour
{
    [Header("Burn Settings: ")]
    public bool isMultipleLensesEffected = false;
    public float burnTime;

    public bool destroyOnComplete = true;
    public UnityEvent onBurnComplete;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private Material ropeUnlit, ropeLit;
    [SerializeField] private Color burnStartColor = Color.white;
    [SerializeField] private Color burnEndColor = Color.red;
    [SerializeField] private float burnStartWidth = 2f;
    [SerializeField] private float burnEndWidth = 5f;

    private int hitsThisFrame = 0;
    [SerializeField] private float currentBurnTime = 0f;
    private Renderer objectRenderer;
    private Outline outline;
    private bool completed;
    public bool isBurning { get; private set; }
    private bool wasBurning;

    private Vector3 lastHitPoint;
    public void RegisterHit(Vector3 hitPoint)
    {
        if (completed) return;
        hitsThisFrame++;
        lastHitPoint = hitPoint;
    }


    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.OutlineColor = burnStartColor;
            outline.OutlineWidth = burnStartWidth;
            outline.enabled = false;
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

    private void HandleFPVChange(bool isFPVActive)
    {
        if (outline != null)
        {
            outline.OutlineWidth = isFPVActive ? burnStartWidth : 0f;
        }

        if (objectRenderer == null) return;

        if (isFPVActive)
        {
            if (ropeLit != null)
                objectRenderer.material = ropeLit;
        }
        else
        {
            if (ropeUnlit != null)
                objectRenderer.material = ropeUnlit;
        }
    }

    private void Update()
    {
        if (completed)
        {
            hitsThisFrame = 0;
            isBurning = false;
            UpdateVFX();
            return;
        }

        isBurning = hitsThisFrame > 0;

        if (isBurning && smokeParticles != null)
        {
            smokeParticles.transform.position = lastHitPoint;
        }

        UpdateVFX();

        if (GameManager.Instance.Player.projector != null && !outline.enabled)
        {
            outline.enabled = true;
        }

        if (isBurning)
        {
            ApplyBurn(Time.deltaTime);
        }
        else if (outline.enabled && GameManager.Instance.Player.projector == null && !GameManager.Instance.Player.isAiming)
        {
            outline.enabled = false;
        }

        hitsThisFrame = 0;
    }

    private void UpdateVFX()
    {
        if (smokeParticles == null) return;

        if (isBurning && !wasBurning)
        {
            smokeParticles.Play();
        }
        else if (!isBurning && wasBurning)
        {
            smokeParticles.Stop();
        }
        wasBurning = isBurning;
    }

    private void ApplyBurn(float deltaTime)
    {
        if (!isBurning) return;

        float multiplier = isMultipleLensesEffected ? hitsThisFrame : 1f;
        float burnIncrement = (deltaTime / Mathf.Max(0.001f, burnTime)) * multiplier;

        currentBurnTime = Mathf.Clamp01(currentBurnTime + burnIncrement);

        if (outline != null)
        {
            outline.OutlineColor = Color.Lerp(burnStartColor, burnEndColor, currentBurnTime);
            outline.OutlineWidth = Mathf.Lerp(burnStartWidth, burnEndWidth, currentBurnTime);
        }

        //if (!isMultipleLensesEffected) hitsThisFrame = 1;
        //Debug.Log(hitsThisFrame);

        //Destroy After Threshold Is Met:
        if (currentBurnTime >= 1f && !completed)
        {
            completed = true;
            Debug.Log($"Burn complete on {gameObject.name}");
            onBurnComplete?.Invoke();

            if (destroyOnComplete)
            {
                Destroy(gameObject);
            }
            else
            {
                currentBurnTime = 0f;
                gameObject.SetActive(false);
                outline.OutlineWidth = 0f;
                outline.OutlineColor = Color.white;
                completed = false;
            }
        }
    }
}
