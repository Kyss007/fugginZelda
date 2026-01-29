using UnityEngine;

public class inventoryItemEquipper : MonoBehaviour
{
    public inventory inventory;

    public void equipSword()
    {
        if(inventory.unlockedSword)
        {
            inventory.swordEquipped = !inventory.swordEquipped;
            inventory.loadSword();
        }
    }
}
