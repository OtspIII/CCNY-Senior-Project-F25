using UnityEngine;

[DefaultExecutionOrder(200)]
public class Test_CamDistance : MonoBehaviour
{
    public Transform playerTarget;
    [SerializeField] float minDisFromPlayer = 1.5f;
    [SerializeField] float pushBackSpeed = 10f;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (playerTarget == null || cam == null) return;
        Vector3 toCam = cam.transform.position - playerTarget.position;
        float dist = toCam.magnitude;

        if (dist >= minDisFromPlayer) return;

        Vector3 direction = dist > 0.001f ? toCam / dist : -cam.transform.forward;

        Vector3 targetPosition = playerTarget.position + direction * minDisFromPlayer;
        cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPosition, pushBackSpeed * Time.deltaTime);
    }

    public void SetPlayerTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }
}
