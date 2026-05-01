using System;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    [SerializeField] private string bottleText;
    [SerializeField] private float fontSize;
    [SerializeField] private float rotationSpeed;

    public static event Action<string, float> OnBottleTriggered;
    public static event Action OnBottleExit;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnBottleTriggered?.Invoke(bottleText, fontSize);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnBottleExit?.Invoke();
        }
    }
}
