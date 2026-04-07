using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ShadowBurn : MonoBehaviour
{
    //Repurposing Josh's burn code for this
    public float burnTime;
    [SerializeField] float currentBurnTime = 0f;
    public Color initialColor;
    public float initalColorBreach;
    [Space]
    public Color middleColor;
    public float middleColorBreach;
    [Space]
    public Color finalColor;
    [Space]
    [SerializeField] Transform player;
    [SerializeField] GameObject key;
    [SerializeField] Transform door;
    bool keyLight;
    [SerializeField] bool doorOpened, moveDoor;
    Renderer objectRenderer;
    Material materialInstance;
    [Space]
    [Tooltip("Check player shadow. Ensures shadow is on desired object.")]
    [SerializeField] ShadowCheck shadowCheck;
    [Tooltip("Check player shadow. Ensures shadow doesn't go past desired size.")]
    [SerializeField] ShadowCheck shadowBoundary;


    void Awake()
    {
        objectRenderer = key.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            materialInstance = objectRenderer.material;
            materialInstance.color = initialColor;
        }
    }
    void Update()
    {
        if (doorOpened && !moveDoor)
            StartCoroutine(OpenDoor(door.transform.position + Vector3.up * 3f));
        else if (!doorOpened)
            CheckForShadow();
    }

    void CheckForShadow()
    {
        //If first ShadowCheck is in shadow and the second is not, the player has correctly positioned the shadow
        if (shadowCheck.IsInShadow() && !shadowBoundary.IsInShadow())
        {
            if (currentBurnTime != 1f)
                ApplyBurn(true);
            else
                doorOpened = true;
        }
        else
        {
            //Lerp color back to start if player moves shadow before door unlocks 
            if (currentBurnTime > 0f)
                ApplyBurn(false);
            else
                currentBurnTime = 0;
        }
    }

    void ApplyBurn(bool inPosition)
    {
        float burnIncrement = Time.deltaTime / burnTime;

        if (inPosition)
            currentBurnTime += burnIncrement;
        else
            currentBurnTime -= burnIncrement;
        currentBurnTime = Mathf.Clamp01(currentBurnTime);

        UpdateMaterial();
    }

    void UpdateMaterial()
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

    IEnumerator OpenDoor(Vector3 target)
    {
        moveDoor = true; // Door stays unlocked once opened
        Vector3 start = door.position;
        Vector3 endPos = target;

        float elapsed = 0f;
        float duration = Vector3.Distance(start, endPos) / 2f;

        // lerp to target
        while (elapsed < duration)
        {
            door.position = Vector3.Lerp(start, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap position
        door.position = endPos;
    }
}

