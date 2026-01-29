using UnityEngine;

public class inventoryItemEquipper : MonoBehaviour
{
    public inventory inventory;
    [Space]
    public bool isSword = false;

    void OnEnable()
    {
        checkIfUnlocked();
    }

    public void checkIfUnlocked()
    {
        if(isSword)
        {
            if(!inventory.unlockedSword)
            {
                gameObject.SetActive(false);
            }
        }
    }    

    public void equipSword()
    {
        if(inventory.unlockedSword)
        {
            inventory.swordEquipped = !inventory.swordEquipped;
            inventory.loadSword();
        }
    }
}
