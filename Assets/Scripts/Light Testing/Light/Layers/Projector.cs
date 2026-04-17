using UnityEngine;

public class Projector : MonoBehaviour
{
    [Header("References: ")]
    public Transform ParentObject;
    public Transform PlayerObject;
    [Space]
    [HideInInspector] public Vector3 baseRotationEuler;
    public Quaternion lightRotationOffset = Quaternion.identity;
    public Quaternion cameraRotationOffset = Quaternion.identity;
    [Space]
    public Transform beamRoot;
    public Transform PivotPosition;
    [SerializeField] GameObject promptText;

    [Header("Beam Settings: ")]
    public LightReflection beamLight;
    [Space]
    public float maxAngle = 90f;
    [Space]
    public float beamWidth = 0.2f;
    public float beamHeight = 0.2f;
    [Space]
    public float lengthPerHit = 1f;

    [Header("Projector Control: ")]
    public bool enterable = true;           //Can the player enter this projector?
    public bool isPlayerInside = false;     //Is the player currently inside this projector?
    [Space]
    public float fixedBeamDistance = 5f;
    public float maxVerticalRotatation = 30f;
    public float maxHorizontalRotatation = 45f;


    //# of hits:
    [HideInInspector] public int hitsThisFrame = 0;
    [HideInInspector] public Vector3 finalBeamDir = Vector3.up;
    [Space]
    private int projectionVersion = 0;
    private int activeProjectionVersion = 0;

    public enum ProjectionMode
    {
        None,
        Raycast,
        Traversal
    }
    public ProjectionMode CurrentDriver { get; private set; } = ProjectionMode.None;
    public bool TrySetDriver(ProjectionMode driver)
    {
        if (CurrentDriver != ProjectionMode.None && CurrentDriver != driver)
            return false;

        CurrentDriver = driver;
        return true;
    }
    public void ClearDriver()
    {
        CurrentDriver = ProjectionMode.None;
    }


    private void Awake()
    {
        if (beamRoot != null)
            beamRoot.localPosition = Vector3.zero;

        if (beamRoot != null)
            beamRoot.gameObject.SetActive(false);

        //Store Base Rotation Euler Angles (Inspector Values):
        baseRotationEuler = lightRotationOffset.eulerAngles;
    }

    private void LateUpdate()
    {
        UpdateBeam();
        hitsThisFrame = 0;
        ClearDriver();
    }

    public void RegisterHit()
    {
        hitsThisFrame++;
        if (hitsThisFrame > 1)
        {
            hitsThisFrame = 1;
        }
    }

    private void UpdateBeam()
    {
        if (beamRoot == null)
            return;

        //No Hits, Hide Beam:
        if (hitsThisFrame <= 0)
        {
            if (beamRoot.gameObject.activeSelf)
                beamRoot.gameObject.SetActive(false);

            beamRoot.localPosition = Vector3.zero;
            beamRoot.localRotation = Quaternion.identity;
            return;
        }

        //Ensure Beam is Active:
        if (!beamRoot.gameObject.activeSelf)
            beamRoot.gameObject.SetActive(true);

        //Align Beam Root with Projector Transform:
        beamRoot.position = transform.position;
        beamRoot.rotation = transform.rotation;
        beamRoot.localScale = Vector3.one;

        //Calculate Raw Beam Length:
        float rawLength;
        rawLength = Mathf.Max(fixedBeamDistance, 0.001f);

        //Adjust For Lossy Scale of Parent Transforms:
        Vector3 lossyScale = transform.lossyScale;

        //Corrected Dimensions:
        float correctedLength = rawLength / Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f);
        float correctedWidth = beamWidth / Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f);
        float correctedHeight = beamHeight / Mathf.Max(Mathf.Abs(lossyScale.z), 0.0001f);
    }

    public bool UpdateYOffeset()
    {
        //Null Checks:
        Transform playerTransform = PlayerObject ?? GameManager.Instance.Player?.transform;
        if (playerTransform == null || ParentObject == null) return false;

        //Calculate Direction to Player on XZ Plane:
        Vector3 toPlayer = playerTransform.position - ParentObject.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        //Calculate Parent's Back Direction on XZ Plane:
        Vector3 parentBack = ParentObject.right;
        parentBack.y = 0f;
        parentBack.Normalize();

        //Calculate Angle Between Parent's Back and Direction to Player:
        float angleToPlayer = Vector3.SignedAngle(parentBack, toPlayer, Vector3.up);

        //Check if Angle Exceeds Max Angle:
        if (angleToPlayer < -maxAngle || angleToPlayer > maxAngle) return false;

        //Apply Rotation Offset:
        lightRotationOffset = Quaternion.Euler(baseRotationEuler.x, (angleToPlayer + baseRotationEuler.y), baseRotationEuler.z);

        //Visualization:
        Debug.DrawRay(ParentObject.position, parentBack * 5f, Color.blue);
        Debug.DrawRay(ParentObject.position, toPlayer * 5f, Color.green);

        return true;
    }

    public int RequestProjectionUpdate()
    {
        projectionVersion++;
        activeProjectionVersion = projectionVersion;
        return projectionVersion;
    }

    public bool IsProjectionValid(int version)
    {
        return version == activeProjectionVersion;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!enterable) return;

        if (col.gameObject.CompareTag("Player") && GameManager.Instance.Player.projector == null)
        {
            GameManager.Instance.Player.projector = this;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (!enterable) return;

        if (col.gameObject.CompareTag("Player") && GameManager.Instance.Player.projector != null)
        {
            GameManager.Instance.Player.projector = null;
        }
    }
}
