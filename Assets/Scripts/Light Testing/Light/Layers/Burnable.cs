using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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

    private int hitsThisFrame = 0;
    [SerializeField] private float currentBurnTime = 0f;
    private Renderer objectRenderer;
    private Outline outline;
    private bool completed;
    public bool isBurning { get; private set; }
    private bool wasBurning;

    public void RegisterHit()
    {
        if (completed) return;
        hitsThisFrame++;
    }


    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.OutlineColor = burnStartColor;
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
            outline.OutlineWidth = isFPVActive ? 2f : 0f;
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
        UpdateVFX();

        if (isBurning)
        {
            ApplyBurn(Time.deltaTime);
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
                Destroy(gameObject);
        }
    }
}
