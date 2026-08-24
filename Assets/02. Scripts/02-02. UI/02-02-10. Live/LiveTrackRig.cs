using UnityEngine;
using VInspector;

/// <summary>
/// 평평한 트랙과 원근 카메라를 외주 디자인 수치 그대로 배치하는 계산기입니다.
///
/// 트랙은 실제로는 폭이 일정한 평평한 띠이고, 사다리꼴 모양은 카메라의 원근 투영이 만들어 냅니다.
/// 따라서 노트는 월드 공간에서 등속으로 다가오기만 하면 되고, 화면상의 가속과 확대는 카메라가 처리합니다.
///
/// 카메라 파라미터는 하드코딩하지 않고 디자인 수치(판정선 폭/높이, 트랙 상단 폭)에서 역산합니다.
/// </summary>
[ExecuteAlways]
public class LiveTrackRig : MonoBehaviour
{
    // 트랙 폭의 절반을 캔버스 좌표 그대로 월드 단위로 쓰면 판정선 높이의 깊이가 초점거리와 같아져 계산이 단순해집니다.
    private const float CANVAS_UNITS_PER_WORLD_UNIT = 1f;

    [Header("Design Reference (2D 씬과 동일해야 함)")]
    [Tooltip("디자인 기준 화면 세로 해상도입니다.")]
    [SerializeField]
    private float _designScreenHeight = 1080f;

    [Tooltip("화면 아래에서 판정선까지의 높이입니다.")]
    [SerializeField]
    private float _hitLineScreenY = 125.99f;

    [Tooltip("판정선 높이에서의 트랙 폭입니다.")]
    [SerializeField]
    private float _hitLineWidth = 1274.91f;

    [Tooltip("화면 최상단에서의 트랙 폭입니다.")]
    [SerializeField]
    private float _topScreenWidth = 252f;

    [Header("Camera")]
    [Tooltip("세로 화각입니다. 화면에 맺히는 트랙 모양은 화각과 무관하게 동일하며, 월드 단위 스케일만 달라집니다.")]
    [Range(10f, 70f)]
    [SerializeField]
    private float _fieldOfView = 40f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private RectTransform _trackRoot;

    [SerializeField]
    private UI_LiveTrackLanes _lanes;

    private readonly LiveTrackCameraBinder _cameraBinder = new LiveTrackCameraBinder();

    public float Pitch { get; private set; }
    public float CameraHeight { get; private set; }
    public float NearDepth { get; private set; }
    public float FarDepth { get; private set; }
    public float HitLineDepth { get; private set; }
    public float TrackWidth => _hitLineWidth;

    /// <summary>
    /// 평평한 띠 위에서 판정선이 놓이는 세로 비율입니다. 깊이에 정비례하므로 화면상 비율과는 다릅니다.
    /// </summary>
    public float HitLineRatio => Mathf.Approximately(FarDepth, NearDepth)
        ? 0f
        : (HitLineDepth - NearDepth) / (FarDepth - NearDepth);

    private void Awake()
    {
        RefreshRig();
    }

    // 카메라는 상주 씬의 것을 빌려 쓰므로 씬을 떠날 때 원래 자세로 돌려놓습니다. 편집 중에는 자세가 튀어 확인이 어려우므로 제외합니다.
    private void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _cameraBinder.Restore();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshRig();
    }
#endif

    /// <summary>
    /// 디자인 수치에서 카메라 자세와 트랙 길이를 역산해 씬에 반영합니다.
    /// </summary>
    public void RefreshRig()
    {
        _camera = _cameraBinder.Resolve(gameObject, _camera);

        if (_camera == null || _trackRoot == null)
        {
            return;
        }

        float halfScreenHeight = _designScreenHeight * 0.5f;
        float focalPixels = halfScreenHeight / Mathf.Tan(_fieldOfView * 0.5f * Mathf.Deg2Rad);

        // 트랙 폭이 0이 되는 화면 높이가 곧 소실선이며, 카메라를 얼마나 숙여야 하는지를 결정합니다.
        float widthSlope = (_topScreenWidth - _hitLineWidth) / (_designScreenHeight - _hitLineScreenY);
        float horizonScreenY = _hitLineScreenY - _hitLineWidth / widthSlope;

        Pitch = Mathf.Atan((horizonScreenY - halfScreenHeight) / focalPixels) * Mathf.Rad2Deg;

        float worldHalfWidth = _hitLineWidth * 0.5f * CANVAS_UNITS_PER_WORLD_UNIT;

        NearDepth = ToDepth(0f, focalPixels, widthSlope, worldHalfWidth);
        FarDepth = ToDepth(_designScreenHeight, focalPixels, widthSlope, worldHalfWidth);
        HitLineDepth = ToDepth(_hitLineScreenY, focalPixels, widthSlope, worldHalfWidth);

        float pitchRad = Pitch * Mathf.Deg2Rad;
        float hitLineViewDepth = ToViewDepth(_hitLineScreenY, widthSlope, focalPixels, worldHalfWidth);
        float hitLineSlope = (_hitLineScreenY - halfScreenHeight) / focalPixels;
        CameraHeight = hitLineViewDepth * (Mathf.Sin(pitchRad) - hitLineSlope * Mathf.Cos(pitchRad));

        ApplyToScene();
    }

    /// <summary>
    /// 화면 높이 비율을 평평한 띠 위의 세로 비율로 옮깁니다.
    /// 3D에서는 비율이 깊이에 정비례하므로, 2D 씬에서 쓰던 화면 기준 값과 눈금이 다릅니다.
    /// </summary>
    public float ScreenRatioToTrackRatio(float screenRatio)
    {
        if (Mathf.Approximately(FarDepth, NearDepth))
        {
            return screenRatio;
        }

        float halfScreenHeight = _designScreenHeight * 0.5f;
        float focalPixels = halfScreenHeight / Mathf.Tan(_fieldOfView * 0.5f * Mathf.Deg2Rad);
        float widthSlope = (_topScreenWidth - _hitLineWidth) / (_designScreenHeight - _hitLineScreenY);
        float worldHalfWidth = _hitLineWidth * 0.5f * CANVAS_UNITS_PER_WORLD_UNIT;

        float depth = ToDepth(screenRatio * _designScreenHeight, focalPixels, widthSlope, worldHalfWidth);
        return Mathf.Clamp01((depth - NearDepth) / (FarDepth - NearDepth));
    }

    /// <summary>
    /// 카메라는 원점 높이 CameraHeight에 두고 트랙은 +Z로 뻗게 하며, 평평한 띠는 그 사이에 눕혀 놓습니다.
    /// </summary>
    private void ApplyToScene()
    {
        _camera.orthographic = false;
        _camera.fieldOfView = _fieldOfView;
        _camera.transform.localPosition = new Vector3(0f, CameraHeight, 0f);
        _camera.transform.localRotation = Quaternion.Euler(Pitch, 0f, 0f);

        // 소실선 너머까지 그릴 필요가 없으므로 원경은 트랙 끝에 여유만 두고 잘라 깊이 정밀도를 확보합니다.
        _camera.nearClipPlane = Mathf.Max(0.01f, NearDepth * 0.25f);
        _camera.farClipPlane = FarDepth * 1.5f;

        float length = FarDepth - NearDepth;
        _trackRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
        _trackRoot.localPosition = new Vector3(0f, 0f, (NearDepth + FarDepth) * 0.5f);
        _trackRoot.sizeDelta = new Vector2(_hitLineWidth, length);

        if (_lanes == null)
        {
            return;
        }

        // 판정선 위치와 원근 단축 보정에 필요한 값은 카메라 자세에서만 나오므로 리그가 트랙에 내려 줍니다.
        _lanes.HitLineRatio = HitLineRatio;
        _lanes.SetViewDepths(ToViewDepthAtDepth(NearDepth), ToViewDepthAtDepth(FarDepth));
    }

    /// <summary>
    /// 바닥면 위의 거리를 광축 방향 거리로 옮깁니다. 겉보기 크기는 이 값에 반비례합니다.
    /// </summary>
    private float ToViewDepthAtDepth(float depth)
    {
        float pitchRad = Pitch * Mathf.Deg2Rad;
        return CameraHeight * Mathf.Sin(pitchRad) + depth * Mathf.Cos(pitchRad);
    }

    /// <summary>
    /// 화면 높이에 대응하는 광축 방향 거리입니다. 겉보기 폭이 이 거리에 반비례한다는 원근의 성질에서 바로 나옵니다.
    /// </summary>
    private float ToViewDepth(float screenY, float widthSlope, float focalPixels, float worldHalfWidth)
    {
        float widthAtScreenY = _hitLineWidth + widthSlope * (screenY - _hitLineScreenY);
        return 2f * focalPixels * worldHalfWidth / widthAtScreenY;
    }

    /// <summary>
    /// 화면 높이에 대응하는 바닥면 위의 거리입니다. 광축 방향 거리를 카메라 자세만큼 되돌려 구합니다.
    /// </summary>
    private float ToDepth(float screenY, float focalPixels, float widthSlope, float worldHalfWidth)
    {
        float viewDepth = ToViewDepth(screenY, widthSlope, focalPixels, worldHalfWidth);
        float pitchRad = Pitch * Mathf.Deg2Rad;
        float slope = (screenY - _designScreenHeight * 0.5f) / focalPixels;

        return viewDepth * (Mathf.Cos(pitchRad) + slope * Mathf.Sin(pitchRad));
    }
}
