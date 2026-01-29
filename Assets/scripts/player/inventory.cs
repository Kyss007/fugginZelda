using UnityEngine;

public class inventory : MonoBehaviour
{
    public float keys = 0;
    [Space]
    public GameObject sword;
    public bool unlockedSword = true;
    public bool swordEquipped = true;

    void Start()
    {
        reloadInventory();
    }

    public void reloadInventory()
    {
        loadSword();
    }

    public void loadSword()
    {
        if(unlockedSword && swordEquipped)
            sword.SetActive(true);
        else
            sword.SetActive(false);
    }
}
