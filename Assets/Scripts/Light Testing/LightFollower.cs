using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFollower : MonoBehaviour
{
    [Header("References: ")]
    public GameObject followerObject;
    public LightReflection lightReflection;
    public PlayerMovement player;
    public LightModeToggle lightModeToggle;


    [Header("Movement Settings: ")]
    public float moveSpeed = 5f;
    public KeyCode startKey = KeyCode.Q;
    public KeyCode moveForwardKey = KeyCode.W;
    public KeyCode moveBackwardKey = KeyCode.S;
    public KeyCode exitLanternKey = KeyCode.Space;
    [Space]


    public bool loop = false;


    private Lantern currentLantern = null;
    private bool isInsideLantern = false;

    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentIndex = 0;
    private bool isMovingAlongPath = false;

    void OnDisable()
    {
        transform.localPosition = Vector3.zero;
        if (lightModeToggle != null) lightModeToggle.enabled = true;
        if (lightReflection != null) lightReflection.enabled = true;


        isMovingAlongPath = false;
        isInsideLantern = false;
    }

    void Update()
    {
        //Not Inside Lantern:
        if (!isInsideLantern)
        {
            //Entering Lantern:
            if (lightReflection != null && lightReflection.lanternHit)
            {
                if (Input.GetKeyDown(startKey))
                {
                    currentLantern = lightReflection.currentLanternHit;

                    if (currentLantern != null && currentLantern.lanternCore != null)
                    {
                        //Disable Player HitBoxes & Other Components:
                        player.enabled = false;
                        if (lightModeToggle != null) lightModeToggle.enabled = false;
                        if (lightReflection != null) lightReflection.enabled = false;

                        //Move Inside LanternCore Position:
                        transform.position = currentLantern.lanternCore.position;
                        isInsideLantern = true;
                        isMovingAlongPath = false;
                    }
                }
            }
            //Entering Normal Light Traversal:
            else if (lightReflection != null && lightReflection.lensHit)
            {
                if (Input.GetKeyDown(startKey) && !isMovingAlongPath)
                {
                    if (followerObject == null) return;

                    pathPoints = new List<Vector3>(lightReflection.laserPoints);
                    if (pathPoints == null || pathPoints.Count == 0) return;

                    currentIndex = 0;
                    Vector3 startPos = pathPoints[currentIndex];

                    startPos.z = followerObject.transform.position.z;
                    followerObject.transform.position = startPos;

                    //Disable Player HitBoxes & Other Components:
                    player.enabled = false;
                    if (lightModeToggle != null) lightModeToggle.enabled = false;
                    if (lightReflection != null) lightReflection.enabled = false;

                    isMovingAlongPath = true;
                }
            }
        }
        //Inside Lantern:
        else
        {
            //Exit Lantern:
            if (Input.GetKeyDown(exitLanternKey))
            {
                ExitLanternMode();
            }

            //Lantern Traversal Forward:
            if (Input.GetKeyDown(moveForwardKey) && currentLantern.nextLantern != null)
            {
                StartCoroutine(MoveToLantern(currentLantern.nextLantern));
            }
            //Lantern Traversal Backward:
            else if (Input.GetKeyDown(moveBackwardKey) && currentLantern.previousLantern != null)
            {
                StartCoroutine(MoveToLantern(currentLantern.previousLantern));
            }
        }

        //Normal Light Traversal:
        if (isMovingAlongPath && pathPoints.Count > 1)
        {
            //Not At The Last Index:
            if (currentIndex < pathPoints.Count - 1)
            {
                Vector3 target = pathPoints[currentIndex + 1];
                followerObject.transform.position = Vector3.MoveTowards
                (
                    followerObject.transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(followerObject.transform.position, target) < 0.01f) currentIndex++;
            }
            //At The Last Index:
            else
            {
                //Enable Player HitBoxes & Other Components:
                player.enabled = true;
                if (lightModeToggle != null) lightModeToggle.enabled = true;
                if (lightReflection != null) lightReflection.enabled = true;


                if (loop)
                    currentIndex = 0;
                else
                    isMovingAlongPath = false;
            }
        }
    }

    private IEnumerator MoveToLantern(Lantern targetLantern)
    {
        if (targetLantern == null || targetLantern.lanternCore == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetLantern.lanternCore.position;

        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, endPos) / moveSpeed;

        //Lerp While Traversing Lantern:
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Re-Assign Current Position & Lantern:
        transform.position = endPos;
        currentLantern = targetLantern;
    }

    private void ExitLanternMode()
    {
        isInsideLantern = false;

        //Enable Player HitBoxes & Other Components:
        player.enabled = true;
        if (lightModeToggle != null) lightModeToggle.enabled = true;
        if (lightReflection != null) lightReflection.enabled = true;


        //Restore Default Position:
        if (followerObject != null)
        {
            transform.position = followerObject.transform.position;
        }

        //Clear Current Lantern:
        currentLantern = null;
    }
}
