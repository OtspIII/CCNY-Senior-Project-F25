using UnityEngine;

public class Projector : MonoBehaviour
{
    [Header("References: ")]
    public Transform ParentObject;
    public Transform PlayerObject;
    [Space]
    private Vector3 baseRotationEuler;
    public Quaternion lightRotationOffset = Quaternion.identity;
    [Space]
    public Transform beamRoot;       
    public Transform beamMesh;

    [Header("Beam Settings: ")]
    public LightReflection beamLight;
    [Space]
    public float maxAngle = 90f;
    [Space]
    public float beamWidth = 0.2f;
    public float beamHeight = 0.2f;
    [Space]
    public float lengthPerHit = 1f;

    //# of hits:
    [HideInInspector] public int hitsThisFrame = 0;

    private void Awake()
    {
        if (beamRoot != null)
            beamRoot.localPosition = Vector3.zero;

        if (beamMesh != null)
            beamMesh.localPosition = Vector3.zero;

        if (beamRoot != null)
            beamRoot.gameObject.SetActive(false);

        //Store Base Rotation Euler Angles (Inspector Values):
        baseRotationEuler = lightRotationOffset.eulerAngles;
    }

    private void LateUpdate()
    {
        UpdateBeam();
        hitsThisFrame = 0;
    }

    public void RegisterHit()
    {
        hitsThisFrame++;
    }


    private void UpdateBeam()
    {
        if (beamRoot == null || beamMesh == null)
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

        //Calculate Beam Length Based on Hits:
        float rawLength = Mathf.Max(hitsThisFrame * lengthPerHit, 0.001f);

        //Adjust For Lossy Scale of Parent Transforms:
        Vector3 lossyScale = transform.lossyScale;

        //Corrected Dimensions:
        float correctedLength = rawLength / Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f);
        float correctedWidth = beamWidth / Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f);
        float correctedHeight = beamHeight / Mathf.Max(Mathf.Abs(lossyScale.z), 0.0001f);

        //Apply Scale to Beam Mesh:
        beamMesh.localScale = new Vector3(correctedLength, correctedWidth, correctedHeight);

        //Position Beam Mesh to Extend Forward from Beam Root (-x):
        beamMesh.localPosition = new Vector3(-correctedLength * 0.5f, 0f, 0f);
        
    }

    public bool UpdateYOffeset()
    {
        //Null Checks:
        if (PlayerObject == null || ParentObject == null) return false;

        //Calculate Direction to Player on XZ Plane:
        Vector3 toPlayer = PlayerObject.position - ParentObject.position;
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
}
