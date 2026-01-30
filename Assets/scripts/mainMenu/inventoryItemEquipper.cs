using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class inventoryItemEquipper : MonoBehaviour
{
    public inventory inventory;
    public verticalCubeMenuInventory menu;
    public GameObject pressButtonIndicator;
    [Space]
    public bool isSword = false;
    public bool isLasso = false;

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

        if(isLasso)
        {
            if(!inventory.unlockedLasso)
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

    public void equipLasso()
    {
        menu.lockMenu = true;
        StartCoroutine(selectAssianbleAction(inventory.lasso));
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

        pressButtonIndicator.SetActive(true);

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

        yield return new WaitForEndOfFrame();

        pressButtonIndicator.SetActive(false);

        menu.lockMenu = false;
    }
}
