using UnityEngine;
using System.Collections;

public class FakeAbilitySelectionUI : MonoBehaviour
{
    public float rotationAmount = 30f;
    public float rotationSpeed = 90f;

    private RectTransform rect;
    private bool isRotating = false;
    private int count = 1;

    public JuicyLock squashLock;
    public JuicySpyglass squashSpyglass;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isRotating)
        {
            count = (count % 12) + 1;
            Debug.Log("Current slot: " + count);

            switch (count)
            {
                case 1:
                    Debug.Log("Slot 1 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    //squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;

                case 2:
                    Debug.Log("Slot 2 selected");
                    squashLock.Lock.enabled = false;
                    if (squashSpyglass != null)
                    {
                        squashSpyglass.PopOut();
                    }
                    break;

                case 3:
                    Debug.Log("Slot 3 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 4:
                    Debug.Log("Slot 4 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    //squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 5:
                    Debug.Log("Slot 5 selected");
                    squashLock.Lock.enabled = false;
                    if (squashSpyglass != null)
                    {
                        squashSpyglass.PopOut();
                    }
                    break;
                case 6:
                    Debug.Log("Slot 6 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 7:
                    Debug.Log("Slot 7 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    //squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 8:
                    Debug.Log("Slot 8 selected");
                    squashLock.Lock.enabled = false;
                    if (squashSpyglass != null)
                    {
                        squashSpyglass.PopOut();
                    }
                    break;
                case 9:
                    Debug.Log("Slot 9 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 10:
                    Debug.Log("Slot 10 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    //squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
                case 11:
                    Debug.Log("Slot 11 selected");
                    squashLock.Lock.enabled = false;
                    if (squashSpyglass != null)
                    {
                        squashSpyglass.PopOut();
                    }
                    break;
                case 12:
                    Debug.Log("Slot 12 selected");
                    squashSpyglass.Spyglass.enabled = false;
                    squashSpyglass.SpyglassBkg.enabled = false;
                    squashSpyglass.Opening.SetTrigger("Close");
                    if (squashLock != null)
                    {
                        squashLock.PopOut();
                    }
                    break;
            }

            StartCoroutine(RotateUI());
        }
    }

    IEnumerator RotateUI()
    {
        isRotating = true;

        Quaternion startRot = rect.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, rotationAmount);

        float elapsed = 0f;
        float duration = Mathf.Abs(rotationAmount) / rotationSpeed;

        while (elapsed < duration)
        {
            rect.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.rotation = endRot;
        isRotating = false;
    }
}