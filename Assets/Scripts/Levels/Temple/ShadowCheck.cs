using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowCheck : MonoBehaviour
{
    // Raycast from all corners of an object toward player to determine whether its in shadow 
    // Source: https://discussions.unity.com/t/regarding-the-shadow-how-can-i-tell-if-the-player-enters-the-shadow/923294/6
    [Header("References")]
    [SerializeField] Transform playerLight;
    [Tooltip("Automatically fills with children of object.")]
    [SerializeField] Transform[] corners;
    [SerializeField] bool[] isHittingPlayer;
    [Space]
    [SerializeField] bool hasBurnables;
    bool removedPillars;
    [Tooltip("Leave empty if Has Burnables is turned off.")]
    [SerializeField] GameObject[] pillars;
    [SerializeField] bool requiresP2, leftCheck;
    [Tooltip("Leave empty if Requires P2 is turned off.")]
    [SerializeField] List<Transform> playerLights;
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

        if (playerLight == null)
            playerLight = GameManager.Instance.Player.GetComponentInChildren<Light>().transform;

        playerLights.Add(playerLight);

    }

    void Update()
    {
        // Update reference to current player when switching characters
        Transform currentPlayer = GameManager.Instance.Player.GetComponentInChildren<Light>().transform;
        if (currentPlayer != playerLight)
        {
            if (playerLights.Count == 1)
                playerLights.Add(currentPlayer);

            playerLight = currentPlayer;
        }

        if (requiresP2)
        {
            if (playerLights.Count > 1) // Check to see if two players are active

                if (playerLights[1].gameObject.activeInHierarchy)
                {
                    // Check to see which player is on left side of puzzle
                    if (playerLights[0].position.x <= playerLights[1].position.x)
                    {
                        playerLight = !leftCheck ? playerLights[0] : playerLights[1];
                    }
                    else
                    {
                        playerLight = !leftCheck ? playerLights[1] : playerLights[0];
                    }
                }

            CheckForShadow();
            return;
        }
        if (hasBurnables && !removedPillars)
        {
            // Only check for shadow once all burnables have been removed
            bool pillarsDestroyed = true;
            for (int i = 0; i < pillars.Length; i++)
            {
                if (pillars[i].activeInHierarchy) pillarsDestroyed = false;
            }
            if (pillarsDestroyed) removedPillars = true;
            else return;
        }
        CheckForShadow();
    }

    void CheckForShadow()
    {
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
