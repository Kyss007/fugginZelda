using Unity.Mathematics;
using UnityEngine;

public class debugFunctions : MonoBehaviour
{
    public void unlockSword()
    {
        inventory inventory = FindFirstObjectByType<inventory>();

        inventory.unlockedSword = !inventory.unlockedSword;

        inventory.reloadInventory();

        inventoryItemEquipper[] itemEquippers = FindObjectsByType<inventoryItemEquipper>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var itemEquipper in itemEquippers)
        {
            itemEquipper.checkIfUnlocked();
        }
    }

    public void unlockLasso()
    {
        inventory inventory = FindFirstObjectByType<inventory>();

        inventory.unlockedLasso = !inventory.unlockedLasso;

        inventory.reloadInventory();

        inventoryItemEquipper[] itemEquippers = FindObjectsByType<inventoryItemEquipper>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var itemEquipper in itemEquippers)
        {
            itemEquipper.checkIfUnlocked();
        }
    }

    public void summonObject(GameObject prefab)
    {
        Transform playerPos = FindFirstObjectByType<keanusCharacterController>().transform;

        Instantiate(prefab, playerPos.position + new Vector3(0, 3, 0) + (playerPos.forward * 10),quaternion.identity);
    }
}
