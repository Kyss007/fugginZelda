using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class verticalCubeMenuInventory : MonoBehaviour
{
    [Header("References")]
    public inventoryMenuInputThing cubeMenu;
    public inventoryMenuInputThing inventoryMenuInputThing;
    public Canvas menuCanvas;
    public RectTransform selector;
    public RectTransform menuOptionsParent;

    [Header("Settings")]
    public Vector2 verticalOffset;
    public int currentSelectedValue = 0;
    public float lerpSpeed = 30f;
    public bool lockMenu = false;

    [Header("Input")]
    public InputActionReference upAction;
    public InputActionReference downAction;
    public InputActionReference leftAction;
    public InputActionReference rightAction;
    public InputActionReference acceptAction;

    private RectTransform targetTransform;


    void OnEnable()
    {
        // When menu opens, validate that the current selection is actually active
        ValidateCurrentSelection();
    }

    void OnDisable()
    {
        if (inventoryMenuInputThing != null)
            inventoryMenuInputThing.menuLock = false;
    }

    private void Update()
    {
        if (cubeMenu != null)
            cubeMenu.menuLock = lockMenu;

        // Ensure we are always pointing at an active object
        ValidateCurrentSelection();

        handleInput();
        updateSelectorTransform();
    }

    /// <summary>
    /// Checks if the currently selected object is active. 
    /// If not, searches for the next available active object.
    /// </summary>
    private void ValidateCurrentSelection()
    {
        if (menuOptionsParent.childCount == 0)
        {
            targetTransform = null;
            return;
        }

        // Update the transform reference based on the index
        Transform child = menuOptionsParent.GetChild(currentSelectedValue);
        
        // If the current child is inactive, try to find a new valid index
        if (!child.gameObject.activeInHierarchy)
        {
            // Attempt to find the next active neighbor
            bool foundNew = FindNextActive(true); 
            
            // If we couldn't find ANY active objects, target is null
            if (!foundNew)
            {
                targetTransform = null;
                return;
            }
        }

        // Refetch in case the index changed during validation
        targetTransform = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<RectTransform>();
    }

    private void handleInput()
    {
        if (menuOptionsParent.childCount == 0) return;
        if (lockMenu) return;

        // If we have no valid target (everything is inactive), we cannot accept input
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy) return;

        menuNavigationHint hint = targetTransform.GetComponent<menuNavigationHint>();

        if (upAction.action.triggered)
        {
            if (hint != null && hint.up != null && hint.up.activeInHierarchy) 
                SetSelectionByObject(hint.up);
            else 
                menuUp();
        }
        
        if (downAction.action.triggered)
        {
            if (hint != null && hint.down != null && hint.down.activeInHierarchy) 
                SetSelectionByObject(hint.down);
            else 
                menuDown();
        }

        if (leftAction != null && leftAction.action.triggered && hint != null && hint.left != null && hint.left.activeInHierarchy)
        {
            SetSelectionByObject(hint.left);
        }

        if (rightAction != null && rightAction.action.triggered && hint != null && hint.right != null && hint.right.activeInHierarchy)
        {
            SetSelectionByObject(hint.right);
        }

        if (acceptAction.action.triggered)
        {
            var clickable = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<clickableObject>();
            if (clickable != null) clickable.onClicked.Invoke();
        }
    }

    private void SetSelectionByObject(GameObject target)
    {
        if (target == null || !target.activeInHierarchy) return;
        
        int index = target.transform.GetSiblingIndex();
        
        if (target.transform.parent == menuOptionsParent)
        {
            currentSelectedValue = index;
        }
    }

    private void updateSelectorTransform()
    {
        // If target is null or inactive, hide the selector
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
        {
            if(selector.gameObject.activeSelf) selector.gameObject.SetActive(false);
            return;
        }

        // If we have a valid target, ensure selector is visible
        if (!selector.gameObject.activeSelf) selector.gameObject.SetActive(true);

        Vector3 targetWorldPos = targetTransform.position + menuCanvas.transform.TransformVector((Vector3)verticalOffset);
        selector.position = Vector3.Lerp(selector.position, targetWorldPos, Time.unscaledDeltaTime * lerpSpeed);

        Quaternion targetRot = Quaternion.Euler(0, 0, 90);
        selector.rotation = Quaternion.Lerp(selector.rotation, targetRot, Time.unscaledDeltaTime * lerpSpeed);
    }

    /// <summary>
    /// Helper to find the next active index. Returns true if one was found.
    /// </summary>
    private bool FindNextActive(bool moveDown)
    {
        int startIndex = currentSelectedValue;
        int count = menuOptionsParent.childCount;
        int nextIndex = startIndex;

        // Loop through all children once to find an active one
        for (int i = 0; i < count; i++)
        {
            if (moveDown)
            {
                nextIndex++;
                if (nextIndex >= count) nextIndex = 0;
            }
            else
            {
                nextIndex--;
                if (nextIndex < 0) nextIndex = count - 1;
            }

            if (menuOptionsParent.GetChild(nextIndex).gameObject.activeInHierarchy)
            {
                currentSelectedValue = nextIndex;
                return true;
            }
        }

        // Check the original index as a fallback (in case it was the only one and active)
        if (menuOptionsParent.GetChild(startIndex).gameObject.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    [ContextMenu("menuUp")]
    public void menuUp()
    {
        FindNextActive(false); // false = move up/backwards
    }

    [ContextMenu("menuDown")]
    public void menuDown()
    {
        FindNextActive(true); // true = move down/forwards
    }
}