using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LanternTravel : MonoBehaviour
{
    [Header("Total Lantern Information: (No Need To Touch In Inspector)")]
    public List<Lantern> ActivatedLanterns;
    [SerializeField] private List<Lantern> visibleLanterns = new List<Lantern>();
    [Space]
    [SerializeField] public Lantern currentLantern = null;
    [SerializeField] private Lantern target;
    [Space]
    [SerializeField] public bool isInsideLantern = false;
    [Space]
    public LanternTravel lanternTravel;

    [Header("References: ")]
    PlayerMovement player; //*Adjust For Exisiting Player Schemtic We Use*
    public Rigidbody rb;
    [Space]
    public GameObject followerObject;
    public GameObject playerModel;
    public LightReflection lightReflection;
    public LightModeToggle lightModeToggle;
    [SerializeField] private LayerMask lanternMask;
    [SerializeField] private LineRenderer travelLine;
    [SerializeField] private float lineWidthStart = 0f;
    [SerializeField] private float lineWidthEnd = 0.5f;
    [SerializeField] private float lineStartWidthMultiplier = 1f;
    [SerializeField] private float lineEndWidthMultiplier = 0.5f;
    [SerializeField] private float lineLerpTime = 0.2f;

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
    public bool isTraveling;
    private float currentLineLerp = 0f;
    private Lantern lastTarget = null;


    private void Awake()
    {
        //if (Instance == null) Instance = this;
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        if (travelLine != null)
        {
            travelLine.positionCount = 2;
            travelLine.enabled = false;
        }
    }


    private void Update()
    {
        //Clear Visible Lantern List:
        UpdateVisibleLanterns();

        if (isInsideLantern && Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("---- LANTERN DEBUG ----");

            if (cameraTransform == null)
            {
                Debug.Log("cameraTransform is NULL (GetLanternInView will fail).");
                return;
            }

            Vector3 start = cameraTransform.position;
            Vector3 end = start + cameraTransform.forward * capsuleLength;

            Debug.Log($"Camera: {cameraTransform.name} pos={start} fwd={cameraTransform.forward}");
            Debug.Log($"Capsule: start={start} end={end} r={capsuleRadius}");

            Debug.Log($"Visible lanterns ({visibleLanterns.Count}):");
            foreach (var l in visibleLanterns)
                Debug.Log($"  - {l.name}");

            // Show what the capsule is actually hitting
            Collider[] hits = Physics.OverlapCapsule(start, end, capsuleRadius, lanternMask, QueryTriggerInteraction.Collide);

            Debug.Log($"Capsule hits ({hits.Length}):");
            foreach (var h in hits)
            {
                Lantern lit = h.GetComponentInParent<Lantern>();
                Debug.Log($"  - collider: {h.name} (layer {LayerMask.LayerToName(h.gameObject.layer)}) parentLantern={(lit ? lit.name : "NONE")}");
            }

            Lantern targetCheck = GetLanternInView();
            Debug.Log($"Lantern In View (GetLanternInView): {(targetCheck ? targetCheck.name : "NONE")}");

            var hitsAll = Physics.OverlapCapsule(start, end, capsuleRadius, ~0, QueryTriggerInteraction.Collide);
            Debug.Log($"Capsule hits ALL ({hitsAll.Length})");

            var hitsLantern = Physics.OverlapCapsule(start, end, capsuleRadius, lanternMask, QueryTriggerInteraction.Collide);
            Debug.Log($"Capsule hits LANTERN MASK ({hitsLantern.Length})");

            foreach (var l in ActivatedLanterns)
            {
                float d = Vector3.Distance(followerObject.transform.position, l.lanternCore.position);
                Debug.Log($"Dist from follower to {l.name}: {d:F2} (range {travelRange})");
            }

        }

        //Initial Lantern Entry:
        if (!isInsideLantern)
        {
            //if (lightReflection.lanternHit && Input.GetKeyDown(enterLanternKey))
            if (player.lantern != null && Input.GetKeyDown(enterLanternKey) && !isTraveling)
            {
                currentLantern = GameManager.Instance.Player.lantern;
                //currentLantern = lightReflection.currentLanternHit;

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
        if (Input.GetKeyDown(exitLanternKey) && !isTraveling)
        {
            ExitLanternMode();
            return;
        }

        //Lantern -> Lantern Traversal:
        target = GetLanternInView();

        // Line drawing logic:
        if (isInsideLantern && currentLantern != null && target != null && travelLine != null)
        {
            if (!travelLine.enabled || target != lastTarget)
            {
                currentLineLerp = 0f;
                travelLine.startWidth = lineWidthStart;
                travelLine.endWidth = lineWidthStart;
            }

            travelLine.enabled = true;
            travelLine.SetPosition(0, currentLantern.lanternCore.position);
            travelLine.SetPosition(1, target.lanternCore.position);

            if (currentLineLerp < 1f)
            {
                currentLineLerp += Time.deltaTime / lineLerpTime;
                float baseWidth = Mathf.Lerp(lineWidthStart, lineWidthEnd, currentLineLerp);
                travelLine.startWidth = baseWidth * lineStartWidthMultiplier;
                travelLine.endWidth = baseWidth * lineEndWidthMultiplier;
            }
        }
        else if (travelLine != null)
        {
            travelLine.enabled = false;
        }

        lastTarget = target;

        if (target != null && Input.GetKeyDown(moveLanternKey) && !isTraveling)
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


    /* private Lantern GetLanternInView()
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
             Lantern lit = hit.GetComponentInParent<Lantern>();

             //Accept Only if => [IN RANGE LIST] & [NOT CURRENT LANTERN]:
             if (lit != null && visibleLanterns.Contains(lit) && lit != currentLantern) return lit;
         }
         return null;
     }*/

    private Lantern GetLanternInView()
    {
        if (cameraTransform == null) return null;

        Vector3 origin = cameraTransform.position;
        Lantern best = null;
        float bestScore = float.NegativeInfinity;

        foreach (Lantern lit in visibleLanterns)
        {
            if (lit == null) continue;
            if (lit == currentLantern) continue;

            Vector3 to = (lit.lanternCore.position - origin).normalized;
            float score = Vector3.Dot(cameraTransform.forward, to);

            if (score > 0.5f && score > bestScore)
            {
                bestScore = score;
                best = lit;
            }
        }

        return best;
    }
    private IEnumerator MoveToLantern(Lantern targetLantern)
    {
        if (targetLantern == null || targetLantern.lanternCore == null) yield break;

        isTraveling = true;
        if (currentLantern != null & currentLantern.aimCollider != null)
            currentLantern.aimCollider.enabled = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetLantern.lanternCore.position;

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
        player.lantern = currentLantern;

        if (currentLantern != null && currentLantern.aimCollider != null)
            currentLantern.aimCollider.enabled = false;

        isTraveling = false;
    }

    public void RegisterActivatedLantern(Lantern lantern)
    {
        if (lantern == null) return;

        if (ActivatedLanterns == null)
            ActivatedLanterns = new List<Lantern>();

        if (!ActivatedLanterns.Contains(lantern))
        {
            ActivatedLanterns.Add(lantern);
            Debug.Log($"[LanternTravel] Registered: {lantern.name} (total {ActivatedLanterns.Count})");
        }
    }


    private void EnterLanternMode()
    {
        if (currentLantern != null && currentLantern.aimCollider != null)
            currentLantern.aimCollider.enabled = false;
        //Disable Player HitBoxes & Other Components:
        player.enabled = false;
        rb.isKinematic = true;
        //if (lightModeToggle != null) lightModeToggle.enabled = false;
        if (!lightModeToggle.inLantern) lightModeToggle.inLantern = true;
        if (lightReflection != null) lightReflection.enabled = false;

        if (playerModel != null) playerModel.SetActive(false);

        isInsideLantern = true;
    }


    private void ExitLanternMode()
    {
        isInsideLantern = false;

        if (travelLine != null)
            travelLine.enabled = false;

        if (currentLantern != null && currentLantern.aimCollider != null)
            currentLantern.aimCollider.enabled = true;

        //Enable Player HitBoxes & Other Components:
        player.enabled = true;
        rb.isKinematic = false;
        //if (lightModeToggle != null) lightModeToggle.enabled = true;
        if (lightModeToggle.inLantern) lightModeToggle.inLantern = false;
        if (lightReflection != null) lightReflection.enabled = true;

        if (playerModel != null) playerModel.SetActive(true);


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
