using UnityEngine;
using UnityEngine.UIElements;

public class UIScaleManager : MonoBehaviour
{
    [Header("기준 해상도 - 모바일 세로")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(720, 1280);

    [Header("Prestige 폰트 크기 (기준 해상도 기준)")]
    [SerializeField] private float prestigeTitleSize = 17f;      // 1.2em * 14px = 16.8px
    [SerializeField] private float prestigeSectionSize = 11f;    // 0.8em * 14px = 11.2px
    [SerializeField] private float prestigeBodySize = 8f;        // 0.6em * 14px = 8.4px
    [SerializeField] private float prestigeCardSize = 10f;       // 0.7em * 14px = 9.8px

    private UIDocument uiDocument;
    private VisualElement root;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument를 찾을 수 없습니다!");
            return;
        }

        root = uiDocument.rootVisualElement;
        ApplyScaling();
    }

    void ApplyScaling()
    {
        // 현재 해상도 기반 스케일 팩터 계산 (높이 기준)
        float scale = Screen.height / referenceResolution.y;

        // 최소/최대 스케일 제한
        scale = Mathf.Clamp(scale, 0.5f, 2.0f);

        // 계산된 픽셀 값
        float scaledTitle = prestigeTitleSize * scale;
        float scaledSection = prestigeSectionSize * scale;
        float scaledBody = prestigeBodySize * scale;
        float scaledCard = prestigeCardSize * scale;

        // prestige-root 찾기
        var prestigeRoot = root.Q<VisualElement>("prestige-root");
        if (prestigeRoot == null)
        {
            Debug.LogWarning("prestige-root를 찾을 수 없습니다.");
            return;
        }

        // Title 클래스에 font-size 적용
        ApplyFontSizeToClass(prestigeRoot, "prestige-title", scaledTitle);

        // Section 클래스들
        ApplyFontSizeToClass(prestigeRoot, "prestige-stat-value", scaledSection);
        ApplyFontSizeToClass(prestigeRoot, "prestige-action-title", scaledSection);
        ApplyFontSizeToClass(prestigeRoot, "prestige-button", scaledSection);

        // Body 클래스들
        ApplyFontSizeToClass(prestigeRoot, "prestige-stat-label", scaledBody);
        ApplyFontSizeToClass(prestigeRoot, "prestige-requirement", scaledBody);
        ApplyFontSizeToClass(prestigeRoot, "upgrade-level", scaledBody);
        ApplyFontSizeToClass(prestigeRoot, "upgrade-description", scaledBody);
        ApplyFontSizeToClass(prestigeRoot, "upgrade-effect", scaledBody);
        ApplyFontSizeToClass(prestigeRoot, "upgrade-cost", scaledBody);

        // Card 클래스들
        ApplyFontSizeToClass(prestigeRoot, "prestige-gain", scaledCard);
        ApplyFontSizeToClass(prestigeRoot, "upgrade-name", scaledCard);

        Debug.Log($"[UIScaleManager] 해상도: {Screen.width}x{Screen.height} | 스케일: {scale:F2} | Title: {scaledTitle:F1}px | Section: {scaledSection:F1}px | Body: {scaledBody:F1}px");
    }

    void ApplyFontSizeToClass(VisualElement container, string className, float fontSize)
    {
        var elements = container.Query<VisualElement>(className: className).ToList();
        foreach (var element in elements)
        {
            element.style.fontSize = fontSize;
        }
    }

    // 해상도 변경 감지 (선택사항)
    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyScaling();
        }
    }

    private int lastWidth;
    private int lastHeight;
}
