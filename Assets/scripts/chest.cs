using UnityEngine;

public class chest : MonoBehaviour
{
    public GameObject chestHinge;

    private inventory inventory;

    private void Start()
    {
        inventory = FindFirstObjectByType<inventory>();
    }

    public void giveKey()
    {
        inventory.keys++;
    }

    public void unlockSword()
    {
        inventory.unlockedSword = true;
    }

    public void debugOpenChest()
    {
        chestHinge.transform.localRotation = Quaternion.Euler(45, 0, 0);
    }
}
