using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class SunWheelController : MonoBehaviour
{
    public static SunWheelController Instance;
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player rotateFeedback;
    [SerializeField] private MMF_Player setupUIFeedback;

    [Header("Spike References")]
    [SerializeField] private SunSpike[] spikes = new SunSpike[5]; // 0-4 positions

    [Header("Rotation Settings")]
    [SerializeField] private float rotationAmount = 35f; // Amount to rotate each spike
    [SerializeField] private float wrapRotationAmount = 220f; // Amount for wrapping spike (4->0 or 0->4)

    [Header("Unlocked Abilities")]
    public bool telescopeUnlocked = false;

    public List<SunSpike.SunSpikeType> unlockedAbilities = new List<SunSpike.SunSpikeType>();
    public int centerIndex = 0; // Tracks which ability is at center

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildUnlockedAbilitiesList();
        InitializeWheel();
    }

    void Update()
    {
        // TODO: Remove test input when proper input system is implemented
        if (Input.GetKeyDown(KeyCode.E)) MoveLeft();
        if (Input.GetKeyDown(KeyCode.R)) MoveRight();
    }

    private void BuildUnlockedAbilitiesList()
    {
        unlockedAbilities.Clear();

        // Lock is always available
        unlockedAbilities.Add(SunSpike.SunSpikeType.Lock);

        if (telescopeUnlocked)
        {
            unlockedAbilities.Add(SunSpike.SunSpikeType.Telescope);
        }

        // TODO: Add more abilities as they're created
        // if (spyglassUnlocked) unlockedAbilities.Add(SunSpike.SunSpikeType.Spyglass);
    }

    private void InitializeWheel()
    {
        centerIndex = 0;

        // Set each spike to its initial type and rotation
        for (int i = 0; i < spikes.Length; i++)
        {
            SunSpike.SunSpikeType typeForThisSpike = GetTypeForPosition(i);
            spikes[i].SetSunSpikeType(typeForThisSpike);
            spikes[i].SetInitialRotation(i);
        }

        // Set initial sizes (position 2 is center)
        spikes[2].Enlarge();
        spikes[1].Shrink();
        spikes[3].Shrink();

        if (setupUIFeedback != null)
        {
            setupUIFeedback.PlayFeedbacks();
        }
    }

    private void MoveRight()
    {
        // Update center index in circular list
        centerIndex = (centerIndex + 1) % unlockedAbilities.Count;

        // Update spike sizes
        spikes[1].Enlarge(); // Position 1 becomes new center
        spikes[2].Shrink();

        // Rotate spike references (position 4 wraps to position 0)
        SunSpike temp = spikes[4];
        for (int i = 4; i > 0; i--)
        {
            spikes[i] = spikes[i - 1];
        }
        spikes[0] = temp;

        // Rotate all spikes counter-clockwise (subtract from Z rotation)
        // Spike at position 0 (was at position 4) needs to wrap around
        SunSpike.SunSpikeType newType = GetTypeForPosition(0);
        spikes[0].RotateByAmount(-wrapRotationAmount, newType);

        for (int i = 1; i < spikes.Length; i++)
        {
            spikes[i].RotateByAmount(-rotationAmount);
        }

        // Play feedback
        rotateFeedback.Initialization();
        rotateFeedback.PlayFeedbacks();
    }

    private void MoveLeft()
    {
        // Update center index in circular list
        centerIndex = (centerIndex - 1 + unlockedAbilities.Count) % unlockedAbilities.Count;

        // Update spike sizes
        spikes[3].Enlarge(); // Position 3 becomes new center
        spikes[2].Shrink();

        // Rotate spike references (position 0 wraps to position 4)
        SunSpike temp = spikes[0];
        for (int i = 0; i < 4; i++)
        {
            spikes[i] = spikes[i + 1];
        }
        spikes[4] = temp;

        // Rotate all spikes clockwise (add to Z rotation)
        // Spike at position 4 (was at position 0) needs to wrap around
        SunSpike.SunSpikeType newType = GetTypeForPosition(4);
        spikes[4].RotateByAmount(wrapRotationAmount, newType);

        for (int i = 0; i < 4; i++)
        {
            spikes[i].RotateByAmount(rotationAmount);
        }

        // Play feedback
        rotateFeedback.Initialization();
        rotateFeedback.PlayFeedbacks();
    }

    private SunSpike.SunSpikeType GetTypeForPosition(int position)
    {
        // Calculate offset from center (position 2)
        int offset = position - 2;

        // Calculate which ability should display at this position
        int abilityIndex = (centerIndex + offset + unlockedAbilities.Count * 100) % unlockedAbilities.Count;

        return unlockedAbilities[abilityIndex];
    }

    public void UnlockAbility(SunSpike.SunSpikeType abilityType)
    {
        if (!unlockedAbilities.Contains(abilityType))
        {
            unlockedAbilities.Add(abilityType);
            InitializeWheel(); // Refresh display with new ability
        }
    }
}