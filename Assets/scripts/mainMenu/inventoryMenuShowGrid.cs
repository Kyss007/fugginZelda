using UnityEngine;

public class inventoryMenuShowGrid : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public bool useUnscaledTime = true;

    public CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    public GameObject showGridThing;
    private bool lastActiveState;

    private void Start()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        lastActiveState = showGridThing.activeSelf;
    }

    void OnEnable()
    {
        foreach (Transform transform in transform.GetChild(0).transform)
        {
            transform.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if(showGridThing.activeSelf && !lastActiveState)
        {
            Show();
        }

        if(!showGridThing.activeSelf && lastActiveState)
        {
            Hide();
        }

        lastActiveState = showGridThing.activeSelf;
    }

    public void Show()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeAlpha(1f));
    }

    public void Hide()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAlpha(0f));
    }

    private System.Collections.IEnumerator FadeAlpha(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        // Enable interaction when fading in
        if (targetAlpha > 0)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // Disable interaction immediately when starting fade out
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}