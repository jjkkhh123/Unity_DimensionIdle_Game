using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonHoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Hold Settings")]
    public float holdDelay = 0.5f;          // 꾹 누르기 시작까지 대기 시간
    public float initialInterval = 0.15f;   // 초기 구매 간격
    public float minInterval = 0.03f;       // 최소 구매 간격 (가속 후)
    public float acceleration = 0.95f;      // 가속도 (0.95 = 5%씩 빨라짐)

    [Header("Events")]
    public UnityEvent onHoldClick;          // 홀드 중 반복 실행될 이벤트

    private bool isHolding = false;
    private bool wasHolding = false;        // 홀드 상태였는지 추적
    private Coroutine holdCoroutine;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        // UnityEvent 초기화
        if (onHoldClick == null)
        {
            onHoldClick = new UnityEvent();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            return;
        }

        isHolding = true;
        wasHolding = false;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
        holdCoroutine = StartCoroutine(HoldCoroutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 홀드 상태가 아니었으면 Button의 기본 onClick이 실행되도록 허용
        // 홀드 상태였으면 클릭 이벤트를 무시 (이미 Hold로 처리됨)
    }

    IEnumerator HoldCoroutine()
    {
        // 초기 지연 (꾹 누르기 시작 전 대기)
        yield return new WaitForSeconds(holdDelay);

        if (!isHolding)
            yield break;

        // 홀드 상태로 진입
        wasHolding = true;

        // 연속 구매 시작
        float currentInterval = initialInterval;

        while (isHolding)
        {
            // 버튼이 비활성화되면 중단
            if (button != null && !button.interactable)
            {
                isHolding = false;
                yield break;
            }

            // 이벤트 실행
            onHoldClick?.Invoke();

            // 다음 실행까지 대기
            yield return new WaitForSeconds(currentInterval);

            // 점점 빨라지기 (가속)
            currentInterval = Mathf.Max(minInterval, currentInterval * acceleration);
        }
    }

    void OnDisable()
    {
        isHolding = false;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }
}
