using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class verticalCubeMenuInventory : MonoBehaviour
{
    public inventoryMenuInputThing inventoryMenuInputThing;
    public Canvas menuCanvas;
    public RectTransform selector;
    public RectTransform menuOptionsParent;

    public Vector2 verticalOffset;
    public int currentSelectedValue = 0;
    public float lerpSpeed = 30f;
    public bool lockMenu = false;

    [Space]
    public InputActionReference upAction;
    public InputActionReference downAction;
    public InputActionReference acceptAction;

    private RectTransform targetTransform;

    void OnEnable()
    {
        currentSelectedValue = 0;
        // Immediate snap to first item on enable if desired
        if (menuOptionsParent.childCount > 0)
            targetTransform = menuOptionsParent.GetChild(0).GetComponent<RectTransform>();
    }

    void OnDisable()
    {
        inventoryMenuInputThing.menuLock = false;
    }

    private void Update()
    {
        handleInput();
        updateSelectorTransform();
    }

    private void handleInput()
    {
        // Safety check for empty menus
        if (menuOptionsParent.childCount == 0) return;

        if(lockMenu)
            return;

        targetTransform = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<RectTransform>();

        if (upAction.action.triggered)
        {
            menuUp();
        }
        
        if (downAction.action.triggered)
        {
            menuDown();
        }

        if (acceptAction.action.triggered)
        {
            menuOptionsParent.GetChild(currentSelectedValue).GetComponent<clickableObject>().onClicked.Invoke();
        }
    }

    private void updateSelectorTransform()
    {
        if (targetTransform == null) return;

        // Apply vertical offset relative to the canvas/menu rotation
        Vector3 targetWorldPos = targetTransform.position + menuCanvas.transform.TransformVector((Vector3)verticalOffset);

        // Smoothly follow the target button
        selector.position = Vector3.Lerp(selector.position, targetWorldPos, Time.unscaledDeltaTime * lerpSpeed);

        // Standard vertical rotation (90 degrees)
        Quaternion targetRot = Quaternion.Euler(0, 0, 90);
        selector.rotation = Quaternion.Lerp(selector.rotation, targetRot, Time.unscaledDeltaTime * lerpSpeed);
    }

    [ContextMenu("menuUp")]
    public void menuUp()
    {
        currentSelectedValue--;
        if (currentSelectedValue < 0)
            currentSelectedValue = menuOptionsParent.childCount - 1;
    }

    [ContextMenu("menuDown")]
    public void menuDown()
    {
        currentSelectedValue++;
        if (currentSelectedValue > menuOptionsParent.childCount - 1)
            currentSelectedValue = 0;
    }
}