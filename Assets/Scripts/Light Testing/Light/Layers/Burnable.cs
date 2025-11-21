using Unity.VisualScripting;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    [Header("Burn Settings: ")]
    public bool isMultipleLensesEffected = false;
    public float burnTime;
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
    private void Update()
    {
        if (hitsThisFrame > 0)
        {
            ApplyBurn(Time.deltaTime);
            hitsThisFrame = 0; 
        }
    }

    public void ApplyBurn(float deltaTime)
    {
        float burnIncrement = deltaTime / burnTime * (isMultipleLensesEffected ? hitsThisFrame : 1);

        currentBurnTime += burnIncrement;
        currentBurnTime = Mathf.Clamp01(currentBurnTime);
        if (!isMultipleLensesEffected) hitsThisFrame = 1;
        Debug.Log(hitsThisFrame);

        UpdateMaterial();

        //Destroy After Threshold Is Met:
        if (currentBurnTime >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateMaterial()
    {
        if (objectRenderer == null) return;

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
