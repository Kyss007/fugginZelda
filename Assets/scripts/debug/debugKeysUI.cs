using UnityEngine;
using TMPro;

public class debugKeysUI : MonoBehaviour
{
    private inventory inventory;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        inventory = FindFirstObjectByType<inventory>();
    }

    void Update()
    {
        if (inventory.keys == 0)
        {
            text.text = "";
        }
        else
        {
            text.text = "Keys: " + inventory.keys;
        }
    }
}
