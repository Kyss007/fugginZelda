using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class verticalCubeMenu : MonoBehaviour
{
    public mainMenuInputThing mainMenuInputThing;
    public Canvas menuCanvas;
    public RectTransform selector;
    public RectTransform targetTransform;
    public RectTransform menuOptionsParent;

    public bool isVertical = true;

    public Vector2 verticalOffset;
    public Vector2 horizontalOffset;

    public int currentSelectedValue = 0;
    public float lerpSpeed = 30f;


    [Space]
    public InputActionReference upAction;
    public InputActionReference downAction;
    public InputActionReference leftAction;
    public InputActionReference rightAction;
    public InputActionReference backAction;
    public InputActionReference acceptAction;

    private sliderSelector currentSliderSelector;

    void OnEnable()
    {
        currentSelectedValue = 0;
    }

    void OnDisable()
    {
        mainMenuInputThing.menuLock = false;
        isVertical = true;
    }

    private void Update()
    {
        handleInput();
        updateSelectorTransform();
        mainMenuInputThing.menuLock = !isVertical;
    }

    private void handleInput()
    {
        if (isVertical)
        {
            targetTransform = menuOptionsParent.GetChild(currentSelectedValue).GetComponent<RectTransform>();

            if (upAction.action.triggered)
                menuUp();
            if (downAction.action.triggered)
                menuDown();

            if (acceptAction.action.triggered &&
                menuOptionsParent.GetChild(currentSelectedValue).TryGetComponent(out sliderSelector sliderSel))
            {
                currentSliderSelector = sliderSel;
                isVertical = false;
            }
        }
        else
        {
            if (currentSliderSelector != null)
            {
                targetTransform = currentSliderSelector.handleTransform;

                if (leftAction.action.triggered)
                    currentSliderSelector.slider.value--;
                if (rightAction.action.triggered)
                    currentSliderSelector.slider.value++;

                if (backAction.action.triggered)
                {
                    currentSliderSelector = null;
                    isVertical = true;
                }
            }
        }
    }

    private void updateSelectorTransform()
    {
        Vector3 offset = isVertical ? (Vector3)verticalOffset : (Vector3)horizontalOffset;
        Vector3 targetWorldPos = targetTransform.position + menuCanvas.transform.TransformVector(offset);

        selector.position = Vector3.Lerp(selector.position, targetWorldPos, Time.unscaledDeltaTime * lerpSpeed);

        Quaternion targetRot = Quaternion.Euler(0, 0, isVertical ? 90 : 0);
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
