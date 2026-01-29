using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class inventoryItemEquipper : MonoBehaviour
{
    public inventory inventory;
    public verticalCubeMenuInventory menu;
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

    public void equipDebug()
    {
        menu.lockMenu = true;
        StartCoroutine(selectAssianbleAction(inventory.debugObject));
    }

    public IEnumerator selectAssianbleAction(GameObject targetObject)
    {
        selectableActionItem actionItem = targetObject.GetComponent<selectableActionItem>();
        if (actionItem == null) { menu.lockMenu = false; yield break; }

        // --- NEW: Enable all potential actions first ---
        foreach (var actionRef in actionItem.inputActions)
        {
            actionRef.action.Enable();
        }

        bool inputFound = false;
        InputActionReference selectedAction = null;

        while (!inputFound)
        {
            foreach (var actionRef in actionItem.inputActions)
            {
                // Use triggered or WasPressedThisFrame()
                if (actionRef.action.triggered)
                {
                    selectedAction = actionRef;
                    inputFound = true;
                    break;
                }
            }
            yield return null; 
        }

        actionItem.assignAction(selectedAction);
        menu.lockMenu = false;
    }
}
