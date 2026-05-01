using UnityEngine;

public class ExitUnlock : MonoBehaviour
{
    public bool shelfIngredientAdded = false;
    public bool panIngredientAdded = false;

    [SerializeField] private GameObject exitDoor;

    public void AddShelfIngredient()
    {
        if (shelfIngredientAdded) return;
        shelfIngredientAdded = true;
        CheckIngredients();
    }

    public void AddPanIngredient()
    {
        if (panIngredientAdded) return;
        panIngredientAdded = true;
        CheckIngredients();
    }

    private void CheckIngredients()
    {
        if (shelfIngredientAdded && panIngredientAdded)
        {
            UnlockExit();
        }
    }

    private void UnlockExit()
    {
        exitDoor.SetActive(true);
    }
}
