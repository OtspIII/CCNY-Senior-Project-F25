using UnityEngine;

public class CameraAiming : MonoBehaviour
{
    public Camera mainCamera;
    public float rotationSpeed = 10f;
    public Vector3 rotationOffset = new Vector3(-90f, 0f, 0f);
    private Quaternion cachedOffset;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        cachedOffset = Quaternion.Euler(rotationOffset);
    }
    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        //Vector3 targetDir = mainCamera.transform.forward;
        //if (targetDir.sqrMagnitude < 0.001f) return;

        //THANK YOU, JOSH// 

        Vector3 targetDir1 = mainCamera.transform.forward;
        targetDir1.y = 0f;
        Vector3 targetDir2 = mainCamera.transform.forward;
        targetDir2.x = 0f;

        if (targetDir1.sqrMagnitude < 0.001f || targetDir2.sqrMagnitude < 0.001) return;

        Quaternion targetRot1 = Quaternion.LookRotation(targetDir1);
        Quaternion targetRot2 = Quaternion.LookRotation(targetDir2);

        Quaternion targetRot = targetRot1 * targetRot2 * cachedOffset;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot2, rotationSpeed * Time.deltaTime);

        // Quaternion targetRot = Quaternion.LookRotation(targetDir) * cachedOffset;
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }


}