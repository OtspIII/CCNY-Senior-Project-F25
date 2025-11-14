using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AbilitySelectorUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] abilityImages;

    [Header("Ability Sprites")]
    public Sprite[] abilitySprites;

    [Header("Ability Colors")]
    public Color[] abilityColors;

    private int currentIndex = 0;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.SwitchAbility.performed += ctx => CycleRight();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleRight();
        }
    }

    private void CycleRight()
    {
        currentIndex++;
        if (currentIndex >= abilitySprites.Length) 
            currentIndex = 0;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (abilitySprites == null || abilitySprites.Length == 0) return;

        int leftIndex = (currentIndex - 1 + abilitySprites.Length)  % abilitySprites.Length;
        int rightIndex = (currentIndex + 1) % abilitySprites.Length;

        abilityImages[0].sprite = abilitySprites[leftIndex];
        abilityImages[1].sprite = abilitySprites[currentIndex];
        abilityImages[2].sprite = abilitySprites[rightIndex];

        // assign matching colors from abilityColors
        abilityImages[0].color = abilityColors[leftIndex];
        abilityImages[1].color = abilityColors[currentIndex];
        abilityImages[2].color = abilityColors[rightIndex];

        for (int i = 0; i < abilityImages.Length; i++)
            abilityImages[i].transform.localScale = Vector3.one * 0.5f;

        abilityImages[1].transform.localScale = Vector3.one;
    }
}
