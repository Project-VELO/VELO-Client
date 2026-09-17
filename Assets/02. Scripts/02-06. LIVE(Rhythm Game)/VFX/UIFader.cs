using UnityEngine;
using UnityEngine.UI;

public class UIFader : MonoBehaviour
{
    [Header("초당 감소할 알파량 (1.0 = 1초 동안 사라짐)")]
    public float fadeSpeed = 1.0f;

    [Header("0이 되었을 때 오브젝트 비활성화 여부")]
    public bool deactivateOnComplete = true; // 파괴 대신 비활성화

    [Header("일시정지(TimeScale=0) 시에도 동일 속도 유지 여부")]
    public bool useUnscaledTime = false;

    private Graphic targetGraphic;

    private void Awake()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    private void Update()
    {
        if (targetGraphic == null) return;

        Color c = targetGraphic.color;

        if (c.a > 0f)
        {
            // FPS와 무관하게 동일한 시간 단위(초)로 감소시키는 핵심 로직
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            c.a -= fadeSpeed * dt;
            c.a = Mathf.Max(0f, c.a);
            targetGraphic.color = c;

            // 투명도가 0에 도달했을 때 비활성화 처리
            if (c.a <= 0f && deactivateOnComplete)
            {
                gameObject.SetActive(false);
            }
        }
    }

    // 외부에서 언제든 다시 투명도 100%로 복구시킬 때 호출하는 함수
    public void ResetAndShow()
    {
        if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();

        Color c = targetGraphic.color;
        c.a = 1f;
        targetGraphic.color = c;
        gameObject.SetActive(true);
    }
}