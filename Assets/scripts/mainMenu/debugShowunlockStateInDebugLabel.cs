using TMPro;
using UnityEngine;

public class debugShowunlockStateInDebugLabel : MonoBehaviour
{
    public inventory inventory;

    public TextMeshProUGUI text;

    public string ogString;

    public bool isSword;
    public bool isLasso;
    public bool isHookshot;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        ogString = text.text;
    }

    void Start()
    {
        inventory = FindFirstObjectByType<inventory>();
    }

    void Update()
    {
        if(isSword)
        {
            if(inventory.unlockedSword)
            {
                text.text = ogString + " [x]";
            }
            else
            {
                text.text = ogString + " [ ]";
            }
        }

        if(isLasso)
        {
            if(inventory.unlockedLasso)
            {
                text.text = ogString + " [x]";
            }
            else
            {
                text.text = ogString + " [ ]";
            }
        }

        if(isHookshot)
        {
            if(inventory.unlockedHookShot)
            {
                text.text = ogString + " [x]";
            }
            else
            {
                text.text = ogString + " [ ]";
            }
        }
    }
}
