using System.Runtime.CompilerServices;
using UnityEngine;

public class inventory : MonoBehaviour
{
    public float keys = 0;
    [Space]
    public GameObject sword;
    public bool unlockedSword = true;
    public bool swordEquipped = true;

    [Space]
    public GameObject lasso;
    public bool unlockedLasso = true;

    [Space]
    public GameObject hookShot;
    public bool unlockedHookShot = true;

    public bool gay;

    void Start()
    {
        reloadInventory();
    }

    public void reloadInventory()
    {
        loadSword();
        loadLasso();
    }

    public void loadSword()
    {
        if(unlockedSword && swordEquipped)
            sword.SetActive(true);
        else
            sword.SetActive(false);
    }

    public void loadLasso()
    {
        lasso.SetActive(unlockedLasso);
    }

    public void loadHookshot()
    {
        hookShot.SetActive(unlockedHookShot);
    }
}
