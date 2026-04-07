using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;
using System;
using Unity.VisualScripting;
using MoreMountains.Tools;

public class PromptTrigger : MonoBehaviour
{
    [SerializeField] TempBurn burn;
    [SerializeField] private GameObject uiElement;
    [SerializeField] private CinemachineCamera fpvCamera;

    [SerializeField] private CharacterSwitcher characterSwitcher;

    private bool isPlayerInside = false;
    private bool isUsingFPV = false;

    public static event Action<bool> OnFPVToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public bool ConsumedFThisFrame {  get; private set; }
    // Update is called once per frame
    void Update()
    {
        ConsumedFThisFrame = false;

        if (isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            ConsumedFThisFrame = true;
            ToggleFPV();
        }
    }
    void ToggleFPV()
    {
        isUsingFPV = !isUsingFPV;
        burn.refraction = !burn.refraction;

        if (isUsingFPV)
        {
            fpvCamera.Priority = 30; // sets the priority high enough to override both freelook and aim cam
            uiElement.SetActive(false);
            MMGameEvent.Trigger("CrosshairOn");
        }
        else
        {
            fpvCamera.Priority = 5;
            uiElement.SetActive(true);
            MMGameEvent.Trigger("CrosshairOff");
        }

        OnFPVToggle?.Invoke(isUsingFPV);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            uiElement.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            isUsingFPV = false;
            fpvCamera.Priority = 5;
            uiElement.SetActive(false);
            MMGameEvent.Trigger("CrosshairOff");
        }
    }

    public void ForceExitFPV()
    {
        if (!isUsingFPV) return;
        ToggleFPV();
    }

    public bool IsPlayerInside()
    {
        return isPlayerInside;
    }

    public void ClearPlayerInside()
    {
        isPlayerInside = false;
    }
}
