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

        Vector3 targetDir = mainCamera.transform.forward;
        if (targetDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(targetDir) * cachedOffset;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}