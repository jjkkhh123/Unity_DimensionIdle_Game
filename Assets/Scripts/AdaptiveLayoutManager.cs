using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 화면 비율에 따라 UI 요소의 배치를 동적으로 조정하는 매니저
/// - PC(가로 긴 화면): 요소들을 가로로 배치
/// - 모바일(세로 긴 화면): 요소들을 세로로 배치
/// </summary>
public class AdaptiveLayoutManager : MonoBehaviour
{
    [Header("레이아웃 전환 임계값")]
    [SerializeField] private float landscapeThreshold = 1.0f; // width/height 비율이 이 값 이상이면 가로 모드 (1.0 = 정사각형 이상)

    [Header("디버그")]
    [SerializeField] private bool showDebugLog = true;

    private UIDocument uiDocument;
    private VisualElement root;
    private int lastWidth;
    private int lastHeight;
    private bool isLandscapeMode;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[AdaptiveLayoutManager] UIDocument 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        root = uiDocument.rootVisualElement;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        // 초기 레이아웃 적용
        ApplyLayout();
    }

    void Update()
    {
        // 화면 크기 변경 감지
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyLayout();
        }
    }

    void ApplyLayout()
    {
        if (root == null) return;

        // 화면 비율 계산
        float aspectRatio = (float)Screen.width / Screen.height;
        bool shouldBeLandscape = aspectRatio >= landscapeThreshold;

        // 모드 변경이 있을 때만 레이아웃 업데이트
        if (shouldBeLandscape != isLandscapeMode)
        {
            isLandscapeMode = shouldBeLandscape;
            UpdateDimensionPanels();
            UpdatePrestigePanels();

            if (showDebugLog)
            {
                Debug.Log($"[AdaptiveLayoutManager] 레이아웃 변경: {Screen.width}x{Screen.height} " +
                          $"(비율: {aspectRatio:F2}) → {(isLandscapeMode ? "가로 모드" : "세로 모드")}");
            }
        }
    }

    /// <summary>
    /// Dimension 패널들의 레이아웃 조정
    /// </summary>
    void UpdateDimensionPanels()
    {
        // 8개의 Dimension 패널 처리
        for (int i = 1; i <= 8; i++)
        {
            string panelName = $"Dimension{i}Panel";
            var panel = root.Q<VisualElement>(panelName);

            if (panel != null)
            {
                if (isLandscapeMode)
                {
                    // 가로 모드: 모든 요소를 한 줄로 배치
                    // 패널 자체는 row 유지하고, 내부 컨테이너들도 row로 변경
                    panel.style.flexDirection = FlexDirection.Row;
                    panel.style.alignItems = Align.Center;
                }
                else
                {
                    // 세로 모드: 기본 row 유지 (UXML 기본값)
                    panel.style.flexDirection = FlexDirection.Row;
                    panel.style.alignItems = Align.Stretch;
                }

                // Information 영역 (타이틀 + 배수)
                var information = root.Q<VisualElement>($"Dimension{i}Information");
                if (information != null)
                {
                    if (isLandscapeMode)
                    {
                        // 가로 모드: 타이틀과 배수를 가로로 나열
                        information.style.flexDirection = FlexDirection.Row;
                        information.style.justifyContent = Justify.FlexStart;
                        information.style.alignItems = Align.Center;
                        information.style.width = StyleKeyword.Auto;

                        // 타이틀과 배수 사이 간격
                        var title = root.Q<Label>($"Dimension{i}Title");
                        var multiplier = root.Q<Label>($"Dimension{i}Multiplier");
                        if (title != null) title.style.marginRight = 10;
                        if (multiplier != null) multiplier.style.marginRight = 10;
                    }
                    else
                    {
                        // 세로 모드: 세로 배치
                        information.style.flexDirection = FlexDirection.Column;
                        information.style.justifyContent = Justify.FlexStart;
                        information.style.alignItems = Align.Stretch;
                        information.style.width = new StyleLength(new Length(25, LengthUnit.Percent));

                        var title = root.Q<Label>($"Dimension{i}Title");
                        var multiplier = root.Q<Label>($"Dimension{i}Multiplier");
                        if (title != null) title.style.marginRight = StyleKeyword.Auto;
                        if (multiplier != null) multiplier.style.marginRight = StyleKeyword.Auto;
                    }
                }

                // Owned Container 영역 (보유량 + /s)
                var ownedContainer = root.Q<VisualElement>($"Dimension{i}OwnedContainer");
                if (ownedContainer != null)
                {
                    if (isLandscapeMode)
                    {
                        // 가로 모드: 보유량과 /s를 가로로 나열
                        ownedContainer.style.flexDirection = FlexDirection.Row;
                        ownedContainer.style.justifyContent = Justify.FlexStart;
                        ownedContainer.style.alignItems = Align.Center;
                        ownedContainer.style.width = StyleKeyword.Auto;

                        // 보유량과 /s 사이 간격
                        var amount = root.Q<Label>($"Dimension{i}Amount");
                        var perSec = root.Q<Label>($"Dimension{i}PerSec");
                        if (amount != null) amount.style.marginRight = 10;
                        if (perSec != null) perSec.style.marginRight = 15;
                    }
                    else
                    {
                        // 세로 모드: 세로 배치
                        ownedContainer.style.flexDirection = FlexDirection.Column;
                        ownedContainer.style.justifyContent = Justify.FlexStart;
                        ownedContainer.style.alignItems = Align.Stretch;
                        ownedContainer.style.width = new StyleLength(new Length(35, LengthUnit.Percent));

                        var amount = root.Q<Label>($"Dimension{i}Amount");
                        var perSec = root.Q<Label>($"Dimension{i}PerSec");
                        if (amount != null) amount.style.marginRight = StyleKeyword.Auto;
                        if (perSec != null) perSec.style.marginRight = StyleKeyword.Auto;
                    }
                }

                // Button Container 조정
                var buttonContainer = root.Q<VisualElement>($"Dimension{i}ButtonContainer");
                if (buttonContainer != null)
                {
                    if (isLandscapeMode)
                    {
                        // 가로 모드: 버튼 컨테이너 크기 조정
                        buttonContainer.style.width = StyleKeyword.Auto;
                        buttonContainer.style.flexGrow = 1;
                    }
                    else
                    {
                        // 세로 모드: 기본값
                        buttonContainer.style.width = new StyleLength(new Length(40, LengthUnit.Percent));
                        buttonContainer.style.flexGrow = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Prestige 패널 레이아웃 조정
    /// </summary>
    void UpdatePrestigePanels()
    {
        var prestigeRoot = root.Q<VisualElement>("prestige-root");
        if (prestigeRoot == null) return;

        var upgradesContainer = prestigeRoot.Q<VisualElement>("upgrades-container");
        if (upgradesContainer != null)
        {
            if (isLandscapeMode)
            {
                // 가로 모드: 3-4열로 배치
                upgradesContainer.style.flexDirection = FlexDirection.Row;
                upgradesContainer.style.flexWrap = Wrap.Wrap;
                upgradesContainer.style.justifyContent = Justify.SpaceBetween;

                // 각 카드의 너비를 조정 (3열 배치)
                var cards = upgradesContainer.Query<VisualElement>(className: "upgrade-card").ToList();
                foreach (var card in cards)
                {
                    card.style.width = new StyleLength(new Length(32, LengthUnit.Percent));
                }
            }
            else
            {
                // 세로 모드: 2열로 배치
                upgradesContainer.style.flexDirection = FlexDirection.Row;
                upgradesContainer.style.flexWrap = Wrap.Wrap;
                upgradesContainer.style.justifyContent = Justify.SpaceBetween;

                // 각 카드의 너비를 조정 (2열 배치)
                var cards = upgradesContainer.Query<VisualElement>(className: "upgrade-card").ToList();
                foreach (var card in cards)
                {
                    card.style.width = new StyleLength(new Length(48, LengthUnit.Percent));
                }
            }
        }
    }

    /// <summary>
    /// 특정 화면 비율로 강제 전환 (테스트용)
    /// </summary>
    [ContextMenu("가로 모드로 강제 전환")]
    public void ForceLandscapeMode()
    {
        isLandscapeMode = true;
        UpdateDimensionPanels();
        UpdatePrestigePanels();
        Debug.Log("[AdaptiveLayoutManager] 가로 모드로 강제 전환됨");
    }

    [ContextMenu("세로 모드로 강제 전환")]
    public void ForcePortraitMode()
    {
        isLandscapeMode = false;
        UpdateDimensionPanels();
        UpdatePrestigePanels();
        Debug.Log("[AdaptiveLayoutManager] 세로 모드로 강제 전환됨");
    }
}
