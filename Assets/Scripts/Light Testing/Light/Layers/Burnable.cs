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
    [HideInInspector] public int hitsThisFrame = 0;
    [SerializeField] private float currentBurnTime = 0f;


    [Header("Burn Color Settings: ")]
    public Color initialColor;
    public float initalColorBreach;
    [Space]
    public Color middleColor;
    public float middleColorBreach;
    [Space]
    public Color finalColor;

    private Renderer objectRenderer;
    private Material materialInstance;
    private bool completed;


    private void Awake()
    {
        //Get Reference To Current Object Render:
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            materialInstance = objectRenderer.material;
            materialInstance.color = initialColor;
        }
    }

    public void RegisterHit(Vector3 hitPoint)
    {
        hitsThisFrame++;
    }
    private void Update()
    {
        if (completed)
        {
            hitsThisFrame = 0;
            return;
        }
        if (hitsThisFrame > 0)
        {
            ApplyBurn(Time.deltaTime);
            
        }
        hitsThisFrame = 0; 
    }

    public void ApplyBurn(float deltaTime)
    {
        float multiplier = isMultipleLensesEffected ? hitsThisFrame : 1f;
        float burnIncrement = (deltaTime / Mathf.Max(0.001f,burnTime)) * multiplier;
        
        currentBurnTime = Mathf.Clamp01(currentBurnTime + burnIncrement);
        UpdateMaterial();
        
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
                Debug.Log($"Destroying {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }
    

    private void UpdateMaterial()
    {
        if (objectRenderer == null || materialInstance == null) return;

    if (currentBurnTime < initalColorBreach)
        {
            float time = currentBurnTime / initalColorBreach;
            materialInstance.color = Color.Lerp(initialColor, middleColor, time);
        }
        else if (currentBurnTime < middleColorBreach)
        {
            float time = (currentBurnTime - initalColorBreach) / (middleColorBreach - initalColorBreach);
            materialInstance.color = Color.Lerp(middleColor, finalColor, time);
        }
        else
        {
            materialInstance.color = finalColor;
        }
    }
}
