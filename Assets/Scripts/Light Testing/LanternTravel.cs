using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternTravel : MonoBehaviour
{
    [Header("Total Lantern Information: (No Need To Touch In Inspector)")]
    public List<Lantern> ActivatedLanterns;
    [SerializeField] private List<Lantern> visibleLanterns = new List<Lantern>();
    [Space]
    [SerializeField] private Lantern currentLantern = null;
    [SerializeField] private Lantern target;
    [Space]
    [SerializeField] private bool isInsideLantern = false;
    [Space]
    public static LanternTravel Instance;



    [Header("References: ")]
    public PlayerMovement player; //*Adjust For Exisiting Player Schemtic We Use*
    public Rigidbody rb;
    [Space]
    public GameObject followerObject;
    public LightReflection lightReflection;
    public LightModeToggle lightModeToggle;

    [Header("Travel Parameters: ")]
    public Transform cameraTransform;
    [Space]
    public Color LanternTravelRangeColor;
    public float travelRange = 5f;
    [Space]
    public Color LanternTargetTraversalColor;
    public float capsuleLength = 3f;
    public float capsuleRadius = 1f;

    [Header("Movement Settings: ")]
    public float moveSpeed = 5f;
    public KeyCode enterLanternKey = KeyCode.Q;
    public KeyCode exitLanternKey = KeyCode.Space;
    public KeyCode moveLanternKey = KeyCode.Mouse0;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        //Clear Visible Lantern List:
        UpdateVisibleLanterns();

        //Initial Lantern Entry:
        if (!isInsideLantern)
        {
            if (lightReflection.lanternHit && Input.GetKeyDown(enterLanternKey))
            {
                currentLantern = lightReflection.currentLanternHit;

                if (currentLantern != null)
                {
                    EnterLanternMode();
                    StartCoroutine(MoveToLantern(currentLantern));
                }
            }
            return;
        }
        else
        {
            if (followerObject.transform.position != currentLantern.lanternCore.transform.position)
            {
                followerObject.transform.position = currentLantern.lanternCore.transform.position;
            }
        }

        //Lantern Exit:
        if (Input.GetKeyDown(exitLanternKey))
        {
            ExitLanternMode();
            return;
        }

        //Lantern -> Lantern Traversal:
        target = GetLanternInView();
        if (target != null && Input.GetKeyDown(moveLanternKey))
        {
            StartCoroutine(MoveToLantern(target));
        }
    }


    private void UpdateVisibleLanterns()
    {
        //Clear Range List:
        visibleLanterns.Clear();

        foreach (var lantern in ActivatedLanterns)
        {
            if (lantern == null || lantern.lanternCore == null) continue;

            //Calculate Range Distance:
            float dist = Vector3.Distance(followerObject.transform.position, lantern.lanternCore.position);

            //If Range Distance is within Travel Range:
            if (dist <= travelRange)
            {
                //Add To Range List:
                visibleLanterns.Add(lantern);
            }
        }
    }


    private Lantern GetLanternInView()
    {
        //Variables For Capsule Range:
        Vector3 start = cameraTransform.position;
        Vector3 end = start + cameraTransform.forward * capsuleLength;

        //Capsule Creation:
        Collider[] hits = Physics.OverlapCapsule
        (
            start,
            end,
            capsuleRadius
        );

        //Capsule Hit Detection:
        foreach (var hit in hits)
        {
            Lantern lit = hit.GetComponent<Lantern>();

            //Accept Only if => [IN RANGE LIST] & [NOT CURRENT LANTERN]:
            if (lit != null && visibleLanterns.Contains(lit) && lit != currentLantern) return lit;
        }
        return null;
    }


    private IEnumerator MoveToLantern(Lantern targetLantern)
    {
        if (targetLantern == null || targetLantern.lanternCore == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetLantern.lanternCore.position.x, targetLantern.lanternCore.position.y - 2.5f, targetLantern.lanternCore.position.z);

        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, endPos) / moveSpeed;

        //Lerp While Traversing Lantern:
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Re-Assign Current Position & Lantern:
        transform.position = endPos;
        currentLantern = targetLantern;
    }


    private void EnterLanternMode()
    {
        //Disable Player HitBoxes & Other Components:
        player.enabled = false;
        rb.isKinematic = true;
        if (lightModeToggle != null) lightModeToggle.enabled = false;
        if (lightReflection != null) lightReflection.enabled = false;

        isInsideLantern = true;
    }

    private void ExitLanternMode()
    {
        isInsideLantern = false;

        //Enable Player HitBoxes & Other Components:
        player.enabled = true;
        rb.isKinematic = false;
        if (lightModeToggle != null) lightModeToggle.enabled = true;
        if (lightReflection != null) lightReflection.enabled = true;


        //Restore Default Position:
        if (followerObject != null)
        {
            transform.position = followerObject.transform.position;
        }

        //Clear Current Lantern:
        currentLantern = null;
    }


    private void OnDrawGizmos()
    {
        //Visible Lantern Range:
        Color travelColor = LanternTravelRangeColor;
        if (travelColor.a == 0f) travelColor.a = 1f;
        Gizmos.color = travelColor;

        //if else null check:
        Vector3 pos = followerObject != null
            ? followerObject.transform.position
            : transform.position;

        //Visualize:
        Gizmos.DrawWireSphere(pos, travelRange);

        //-------------------------------------------------------

        //Lantern In View:
        Color targetColor = LanternTargetTraversalColor;
        if (targetColor.a == 0f) targetColor.a = 1f;
        Gizmos.color = targetColor;

        if (cameraTransform == null) return;

        Vector3 start = cameraTransform.position;
        Vector3 end = start + cameraTransform.forward * capsuleLength;

        //Visualize:
        DrawWireCapsule(start, end, capsuleRadius);
    }


    //Helper Function To Visualize Capsule:
    private void DrawWireCapsule(Vector3 start, Vector3 end, float radius)
    {
        //Sphere's At End Points:
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);


        //Distance From End Points:
        Vector3 dir = (end - start).normalized;


        Vector3 right = Vector3.Cross(dir, Vector3.up).normalized * radius;
        //Right Side Line:
        Gizmos.DrawLine(start + right, end + right);
        //Left Side Line:
        Gizmos.DrawLine(start - right, end - right);


        Vector3 forward = Vector3.Cross(dir, right).normalized * radius;
        //Top Side Line:
        Gizmos.DrawLine(start + forward, end + forward);
        //Bottom Side Line:
        Gizmos.DrawLine(start - forward, end - forward);
    }
}
