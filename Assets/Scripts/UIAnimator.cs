using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    [Header("Fade Animation")]
    public bool fadeOnEnable = true;
    public float fadeDuration = 0.3f;

    [Header("Scale Animation")]
    public bool scaleOnEnable = false;
    public float scaleDuration = 0.25f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1);

    [Header("Pulse Animation")]
    public bool pulseLoop = false;
    public float pulseDuration = 1.5f;
    public float pulseScale = 1.05f;

    [Header("Slide Animation")]
    public bool slideOnEnable = false;
    public Vector2 slideOffset = new Vector2(0, -50);
    public float slideDuration = 0.4f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && (fadeOnEnable || slideOnEnable))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void OnEnable()
    {
        if (fadeOnEnable)
        {
            StartCoroutine(FadeIn());
        }

        if (scaleOnEnable)
        {
            StartCoroutine(ScaleIn());
        }

        if (slideOnEnable)
        {
            StartCoroutine(SlideIn());
        }

        if (pulseLoop)
        {
            pulseCoroutine = StartCoroutine(PulseLoop());
        }
    }

    void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Reset to original state
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.anchoredPosition = originalPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    IEnumerator ScaleIn()
    {
        float elapsed = 0f;
        Vector3 startScale = originalScale * 0.8f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            float curveValue = scaleCurve.Evaluate(t);
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, originalScale, curveValue);
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    IEnumerator SlideIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPosition + slideOffset;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            float curveValue = slideCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(originalPosition + slideOffset, originalPosition, curveValue);
            canvasGroup.alpha = t;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 1f;
    }

    IEnumerator PulseLoop()
    {
        while (true)
        {
            // Scale up
            float elapsed = 0f;
            while (elapsed < pulseDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration / 2);
                float scale = Mathf.Lerp(1f, pulseScale, Mathf.Sin(t * Mathf.PI / 2));
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < pulseDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration / 2);
                float scale = Mathf.Lerp(pulseScale, 1f, Mathf.Sin(t * Mathf.PI / 2));
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }

            rectTransform.localScale = originalScale;
        }
    }

    // Public methods to trigger animations manually
    public void PlayFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    public void PlayScaleIn()
    {
        StartCoroutine(ScaleIn());
    }

    public void PlaySlideIn()
    {
        StartCoroutine(SlideIn());
    }

    public void PlayPulse()
    {
        StartCoroutine(PlayPulseOnce());
    }

    IEnumerator PlayPulseOnce()
    {
        Vector3 targetScale = originalScale * pulseScale;
        float elapsed = 0f;

        // Scale up
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pulseDuration / 2);
            rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Sin(t * Mathf.PI / 2));
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pulseDuration / 2);
            rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, Mathf.Sin(t * Mathf.PI / 2));
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }
}
