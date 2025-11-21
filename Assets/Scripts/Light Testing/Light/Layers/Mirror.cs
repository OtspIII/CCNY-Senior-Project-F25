using UnityEngine;

public class Mirror : MonoBehaviour
{
    //Uses Default Surface Normal:
    public bool useSurfaceNormal = true;

    //For Custom Rotation Axis:
    [Range(0f, 360f)] public float reflectionAngle = 0f;
}
