using System.Collections.Generic;
using UnityEngine;

public class ShadowCheck : MonoBehaviour
{
    // Raycast from all corners of an object toward player to determine whether its in shadow 
    // Source: https://discussions.unity.com/t/regarding-the-shadow-how-can-i-tell-if-the-player-enters-the-shadow/923294/6

    [SerializeField] Transform[] corners;
    [SerializeField] bool[] isHittingPlayer;
    [SerializeField] Transform playerLight;
    public bool isInShadow;
    [SerializeField] bool hasBurnables;
    bool removedPillars;
    [SerializeField] GameObject[] pillars;
    LayerMask layerMask;

    void Awake()
    {
        if (hasBurnables)
            layerMask = LayerMask.GetMask("Player", "Shadows", "Burnable");
        else
            layerMask = LayerMask.GetMask("Player", "Shadows");
    }
    void Start()
    {
        corners = new Transform[transform.childCount];
        isHittingPlayer = new bool[corners.Length];

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.GetChild(i);
            isHittingPlayer[i] = true;
        }
    }

    void Update()
    {
        if (!removedPillars)
        {
            bool pillarsDestroyed = true;
            for (int i = 0; i < pillars.Length; i++)
            {
                if (pillars[i].activeInHierarchy) pillarsDestroyed = false;
            }
            if (pillarsDestroyed) removedPillars = true;
            else return;
        }

        for (int i = 0; i < corners.Length; i++)
        {
            //Raycast form all corners of gameObject toward player 
            Vector3 direction = playerLight.position - corners[i].position;
            direction.Normalize();
            Debug.DrawRay(corners[i].position, direction * 30f, Color.magenta);

            RaycastHit hit; //***
            if (Physics.Raycast(corners[i].position, direction, out hit, Mathf.Infinity, layerMask))
            {
                // True if raycast is able to hit player
                if (hit.transform.CompareTag("Player") && !isHittingPlayer[i]) isHittingPlayer[i] = true;
                else if (!hit.transform.CompareTag("Player") && isHittingPlayer[i]) isHittingPlayer[i] = false;
            }
        }
    }

    public bool IsInShadow()
    {
        //If no corners are hitting the player, then the object must be in shadow
        for (int i = 0; i < isHittingPlayer.Length; i++)
        {
            if (isHittingPlayer[i]) return false;
        }

        return true;
    }
}
