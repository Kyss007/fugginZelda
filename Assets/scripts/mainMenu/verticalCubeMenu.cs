using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // For ScrollRect

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


    public ScrollRect scrollRect;
    public float entryHeight = 150f;
    public int visibleItems = 5;

    [Space]
    public InputActionReference pauseAction;
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
        scrollToCurrent();
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
            {
                menuUp();
                ensureVisible();
            }
            if (downAction.action.triggered)
            {
                menuDown();
                ensureVisible();
            }

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

                if (upAction.action.triggered)
                {
                    int index = currentSelectedValue - 1;
                    if (index < 0)
                        index = menuOptionsParent.childCount - 1;

                    if (menuOptionsParent.GetChild(index).TryGetComponent(out sliderSelector sliderSel))
                    {
                        currentSelectedValue = index;
                        currentSliderSelector = sliderSel;
                        ensureVisible();
                    }
                }

                if (downAction.action.triggered)
                {
                    int index = currentSelectedValue + 1;
                    if (index > menuOptionsParent.childCount - 1)
                        index = 0;

                    if (menuOptionsParent.GetChild(index).TryGetComponent(out sliderSelector sliderSel))
                    {
                        currentSelectedValue = index;
                        currentSliderSelector = sliderSel;
                        ensureVisible();
                    }
                }

                if (backAction.action.triggered || pauseAction.action.triggered)
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

    private void ensureVisible()
    {
        if (scrollRect == null) return;

        RectTransform content = scrollRect.content;

        float viewportHeight = entryHeight * visibleItems;
        float scrollY = content.anchoredPosition.y;

        float itemY = currentSelectedValue * entryHeight;

        if (itemY > scrollY + viewportHeight - entryHeight)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, itemY - (visibleItems - 1) * entryHeight);
        }

        else if (itemY < scrollY)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, itemY);
        }
    }

    private void scrollToCurrent()
    {
        if (scrollRect == null) return;
        RectTransform content = scrollRect.content;
        content.anchoredPosition = new Vector2(0, currentSelectedValue * entryHeight);
    }
}
