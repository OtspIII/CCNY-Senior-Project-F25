using UnityEngine;

public class LightModeToggle : MonoBehaviour
{
    [Header("Default Mode: ")]
    public LightAuraFollower lightAuraFollower;


    [Header("Aiming Mode: ")]
    public Transform lightAimingTool;
    public bool inLantern;
    public bool inProjector;


    void Update()
    {
        // Toggle visibility of Light Aura and Aiming Tool based on whether the player is in a lantern or projector, and whether the aiming key (M2) is pressed.
        if (inLantern || inProjector)
        {
            //Using M2 to match Aiming Key From Player Controller:
            if (lightAuraFollower != null) lightAuraFollower.SetAuraActive(false);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(false);
        }
        else
        {
            if (lightAuraFollower != null && !inLantern) lightAuraFollower.SetAuraActive(true);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(true);
        }

        //Using M2 to match Aiming Key From Player Controller:
        if (Input.GetMouseButton(1) && !inLantern && !inProjector)
        {
            if (lightAuraFollower != null) lightAuraFollower.SetAuraActive(false);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(true);
        }
        else
        {
            if (lightAuraFollower != null && !inLantern) lightAuraFollower.SetAuraActive(true);
            if (lightAimingTool != null) lightAimingTool.gameObject.SetActive(false);
        }
    }
}
