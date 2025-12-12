using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class SunSpike : MonoBehaviour {
    public enum SunSpikeType {
        Lock,
        Telescope
    }

    [Header("Components")]
    [SerializeField] private RectTransform spikeRectTransform;
    [SerializeField] private MMSpringScale springScale;
    [SerializeField] private MMSpringRotation springRotation;
    [SerializeField] private MMF_Player sizeChangeFeedback;

    [Header("Type-Specific Objects")]
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject telescopeObject;
    [SerializeField] private Animator telescopeAnimator;

    [Header("Settings")]
    [SerializeField] private Vector3 largeScaleSize = Vector3.one;
    [SerializeField] private Vector3 smallScaleSize = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector2 largePivot = new Vector2(0.5f, -1.2f);
    [SerializeField] private Vector2 smallPivot = new Vector2(0.5f, -2.4f);
    [SerializeField] private float pivotTransitionDuration = 0.3f;
    [SerializeField] private MMTween.MMTweenCurve pivotEaseType = MMTween.MMTweenCurve.EaseOutCubic;

    // Initial rotation positions for each spike position
    private readonly float[] initialRotations = new float[]
    {
        115f,  // Position 0
        80f,   // Position 1
        45f,   // Position 2 (center)
        10f,   // Position 3
        335f   // Position 4
    };

    private SunSpikeType currentSunSpikeType = SunSpikeType.Lock;
    private Coroutine currentPivotTween;

    public void Enlarge() {
        // Stop any existing pivot tween
        if (currentPivotTween != null) {
            StopCoroutine(currentPivotTween);
        }

        // Smoothly transition pivot using MMTween
        currentPivotTween = StartCoroutine(TweenPivot(largePivot));

        springScale.MoveTo(largeScaleSize);

        if (currentSunSpikeType == SunSpikeType.Telescope) {
            telescopeAnimator.SetBool("Open", true);
        }

        sizeChangeFeedback.PlayFeedbacks();
    }

    public void Shrink() {
        // Stop any existing pivot tween
        if (currentPivotTween != null) {
            StopCoroutine(currentPivotTween);
        }

        // Smoothly transition pivot using MMTween
        currentPivotTween = StartCoroutine(TweenPivot(smallPivot));

        springScale.MoveTo(smallScaleSize);

        if (currentSunSpikeType == SunSpikeType.Telescope) {
            telescopeAnimator.SetBool("Open", false);
        }

        sizeChangeFeedback.PlayFeedbacks();
    }

    private System.Collections.IEnumerator TweenPivot(Vector2 targetPivot) {
        Vector2 startPivot = spikeRectTransform.pivot;
        float elapsed = 0f;

        while (elapsed < pivotTransitionDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pivotTransitionDuration);
            float easedT = MMTween.Tween(t, 0f, 1f, 0f, 1f, pivotEaseType);
            spikeRectTransform.pivot = Vector2.Lerp(startPivot, targetPivot, easedT);
            yield return null;
        }

        spikeRectTransform.pivot = targetPivot;
    }

    // Set initial rotation for this spike based on its position
    public void SetInitialRotation(int position) {
        if (position >= 0 && position < initialRotations.Length) {
            springRotation.MoveTo(new Vector3(0, 0, initialRotations[position]));
        }
    }

    // Rotate by an additive amount (positive = clockwise, negative = counter-clockwise)
    public void RotateByAmount(float amount) {
        springRotation.MoveToAdditive(new Vector3(0, 0, amount));
    }

    // Rotate by an additive amount and change type
    public void RotateByAmount(float amount, SunSpikeType newType) {
        springRotation.MoveToAdditive(new Vector3(0, 0, amount));
        SetSunSpikeType(newType);
    }

    public void SetSunSpikeType(SunSpikeType newType) {
        currentSunSpikeType = newType;

        switch (newType) {
            case SunSpikeType.Lock:
                lockObject.SetActive(true);
                telescopeObject.SetActive(false);
                break;
            case SunSpikeType.Telescope:
                lockObject.SetActive(false);
                telescopeObject.SetActive(true);
                break;
        }
    }

    private void OnDestroy() {
        // Clean up tween on destroy
        if (currentPivotTween != null) {
            StopCoroutine(currentPivotTween);
        }
    }
}
//Intake a list of five spike prefabs
// 0, left offscreen
// 1, left
// 2, enlarged center
// 3, right
// 4, right offscreen

//bools for each item unlocked (telescope unlocked ex.)

//Intake a list of all player abilities based on that scaled up(scaling up infinitely)
//can be as small as 1 or as big as possible

//Set the prefabs to start centering on at 1 in player abilities list
//call their SetSunType function respectively
//Call an arbitray "SetupUI function MMF

//On Move Right
//1 scales up
//2 scales down
//4 calls movetozero and set it to the respective element in playerabilities list
// Call MMF

//On Move Left
//3 scales up
//2 scales down
//0 calls movetozero and set it to the respective element in playerabilities list
// Call MMF


//Example:
// If no items unlocked, the only unlocked item is Lock. so every spike is a lock forever

//Example:
//If telescope is unlocked
//0:T 1:L 2:T 3:L 4:T
//Where T is a tele icon and L is a lock icon

//Example:
//Three
// 0:Q 1:L 2:T 3:Q 4:L
//If shifted right
// 0:L 1:Q 2:L 3:T 3:Q