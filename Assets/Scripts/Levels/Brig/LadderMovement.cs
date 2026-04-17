using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LadderMovement : MonoBehaviour
{
    PlayerMovement playerReference;
    GameObject player;
    Transform currentJumpPoint;
    [SerializeField] List<Transform> jumpPoints = new List<Transform>();
    public bool playerInside, isMoving, isRotating;
    int direction = 1;
    bool turnAround, canMove = true;
    [SerializeField] bool removeStart, removeEnd;

    void Start()
    {
        playerReference = GameManager.Instance.Player;

        // Add all children as the jump points
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            jumpPoints.Add(t);
        }

        // Remove first point
        jumpPoints.Remove(jumpPoints[0]);
        if (removeStart) jumpPoints.Remove(jumpPoints[0]);
        if (removeEnd) jumpPoints.Remove(jumpPoints[jumpPoints.Count - 1]);
    }

    void Update()
    {
        // Exit ladder if at start of end
        if (currentJumpPoint == jumpPoints[jumpPoints.Count - 1] || (currentJumpPoint == jumpPoints[0] && !removeStart))
            ResetPlayerValues();

        if (!playerInside) return;

        //Get direction 
        float dot = Vector3.Dot(player.transform.forward, playerReference.camOrientation.forward);
        //Debug.Log(dot);

        if (Input.GetKey(KeyCode.W) && !isMoving)
        {
            // Rotate player if facing opposite direction
            if (dot <= -0.5f && !turnAround)
                ChangeDirection();

            // Checks to see if player can move forward
            if (removeStart && currentJumpPoint == jumpPoints[0] && !turnAround) return;

            // Find and move to next step
            Transform nextStep = jumpPoints[jumpPoints.IndexOf(currentJumpPoint) + direction];
            StartCoroutine(MoveToStep(nextStep));
        }

        if (Input.GetKey(KeyCode.S) && !isMoving)
        {
            if (dot > -0.5f && !turnAround)
                ChangeDirection();

            // Find and move to next step
            Transform nextStep = jumpPoints[jumpPoints.IndexOf(currentJumpPoint) + direction];
            StartCoroutine(MoveToStep(nextStep));
        }
    }

    IEnumerator MoveToStep(Transform step)
    {
        isMoving = true;

        Vector3 startPos = player.transform.position;

        float distanceBetweenSteps = Vector3.Distance(startPos, step.position);
        Vector3 direction = step.position - startPos;
        direction.Normalize();
        Vector3 halfwayTarget = startPos + direction * distanceBetweenSteps / 2f;
        halfwayTarget.y += 1.1f;

        Vector3 target = step.position + Vector3.up;

        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            float time = elapsed / duration;
            player.transform.position = Vector3.Lerp(startPos, halfwayTarget, time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            float time = elapsed / duration;
            player.transform.position = Vector3.Lerp(halfwayTarget, target, time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = target;
        currentJumpPoint = step;
        if (removeStart && currentJumpPoint == jumpPoints[0]) canMove = false;
        isMoving = false;
    }

    IEnumerator RotatePlayer(Vector3 rot, bool changeDir)
    {
        isRotating = true;
        if (changeDir) direction *= -1;

        Vector3 startRot = player.transform.localEulerAngles;
        Vector3 targetRot = new Vector3(player.transform.localEulerAngles.x, rot.y, player.transform.localEulerAngles.z);

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            float time = elapsed / duration;
            player.transform.localEulerAngles = Vector3.Lerp(startRot, targetRot, time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.localEulerAngles = targetRot;

        turnAround = false;
        isRotating = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (playerInside) return;

        if (col.gameObject.CompareTag("Player") && !playerInside)
        {
            playerInside = true;
            direction = 1;
            player = col.gameObject;
            playerReference.rb.isKinematic = true;
            playerReference.ladder = this;
            Transform step = ClosestStep();
            StartCoroutine(MoveToStep(step));
            bool atEnd = jumpPoints[jumpPoints.IndexOf(step) + 1].CompareTag("Ladder End Point");
            Vector3 rot = transform.localEulerAngles;
            if (atEnd) rot.y += 180f;
            //Debug.Log(atEnd);
            StartCoroutine(RotatePlayer(rot, atEnd));
        }
    }

    void ChangeDirection()
    {
        turnAround = true;
        Vector3 rot = transform.localEulerAngles;
        if (direction == 1) rot.y += 180f;
        StartCoroutine(RotatePlayer(rot, true));
    }

    public void ResetPlayerValues()
    {
        currentJumpPoint = null;
        playerInside = false;
        player = null;
        playerReference.rb.isKinematic = false;
        playerReference.ladder = null;
    }

    // Get closest step // 
    // code courtesty of: https://discussions.unity.com/t/clean-est-way-to-find-nearest-object-of-many-c/409917
    Transform ClosestStep()
    {
        Transform min = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = player.transform.position;

        foreach (Transform t in jumpPoints)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist && t.CompareTag("Untagged"))
            {
                min = t;
                minDist = dist;
            }
        }

        return min;
    }
}
