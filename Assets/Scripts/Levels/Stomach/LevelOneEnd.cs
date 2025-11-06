using Unity.VisualScripting;
using UnityEngine;

public class LevelOneEnd : MonoBehaviour
{
    [SerializeField] int numberToWin = 3;
    [SerializeField] int currentNumber = 0;
    bool fin;

    void Update()
    {
        if (currentNumber == numberToWin && !fin)
        {
            Debug.Log("YOU WIN");
            fin = true;
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player" || (col.gameObject.tag == "Soul" && !col.isTrigger)) currentNumber++;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player" || (col.gameObject.tag == "Soul" && !col.isTrigger)) currentNumber--;
    }
}
