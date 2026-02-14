using UnityEngine;

public class LightModeToggle : MonoBehaviour
{
    [Header("Default Mode: ")]
    public LightAuraFollower lightAuraFollower;


    [Header("Aiming Mode: ")]
    public Transform lightAimingTool;
    public bool inLantern;


    void Update()
    {
        if (inLantern || Input.GetMouseButton(1))
        {
            //Using M2 to match Aiming Key From Player Controller:
            //if (lightAuraFollower != null) lightAuraFollower.SetAuraActive(false);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(true);
        }
        else
        {
            //if (lightAuraFollower != null && !inLantern) lightAuraFollower.SetAuraActive(true);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(false);
        }
    }
}
