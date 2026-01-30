using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class verticalCubeMenuInventory : MonoBehaviour
{
    public inventoryMenuInputThing cubeMenu;
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
    public InputActionReference leftAction;
    public InputActionReference rightAction;
    public InputActionReference acceptAction;

    private RectTransform targetTransform;

    void Start()
    {
        currentSelectedValue = 0;
    }

    void OnEnable()
    {
        //currentSelectedValue = 0;
        if (menuOptionsParent.childCount > 0)
            targetTransform = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<RectTransform>();
    }

    void OnDisable()
    {
        inventoryMenuInputThing.menuLock = false;
    }

    private void Update()
    {
        cubeMenu.menuLock = lockMenu;

        handleInput();
        updateSelectorTransform();
    }

    private void handleInput()
    {
        if (menuOptionsParent.childCount == 0) return;
        if (lockMenu) return;

        targetTransform = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<RectTransform>();
        
        menuNavigationHint hint = targetTransform.GetComponent<menuNavigationHint>();

        if (upAction.action.triggered)
        {
            if (hint != null && hint.up != null) SetSelectionByObject(hint.up);
            else menuUp();
        }
        
        if (downAction.action.triggered)
        {
            if (hint != null && hint.down != null) SetSelectionByObject(hint.down);
            else menuDown();
        }

        if (leftAction != null && leftAction.action.triggered && hint != null && hint.left != null)
        {
            SetSelectionByObject(hint.left);
        }

        if (rightAction != null && rightAction.action.triggered && hint != null && hint.right != null)
        {
            SetSelectionByObject(hint.right);
        }

        if (acceptAction.action.triggered)
        {
            menuOptionsParent.GetChild(currentSelectedValue).GetComponent<clickableObject>().onClicked.Invoke();
        }
    }

    private void SetSelectionByObject(GameObject target)
    {
        if (target == null) return;
        int index = target.transform.GetSiblingIndex();
        
        if (target.transform.parent == menuOptionsParent)
        {
            currentSelectedValue = index;
        }
    }

    private void updateSelectorTransform()
    {
        if (targetTransform == null) return;

        Vector3 targetWorldPos = targetTransform.position + menuCanvas.transform.TransformVector((Vector3)verticalOffset);
        selector.position = Vector3.Lerp(selector.position, targetWorldPos, Time.unscaledDeltaTime * lerpSpeed);

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