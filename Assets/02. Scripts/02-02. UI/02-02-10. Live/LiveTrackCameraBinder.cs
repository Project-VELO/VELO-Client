using UnityEngine;

/// <summary>
/// 라이브 트랙이 쓸 카메라를 찾아 주고, 그 카메라의 원래 상태를 되돌려 주는 중개자입니다.
///
/// 카메라는 PersistentScene에 상주하며 모든 화면이 함께 씁니다. 트랙이 원근으로 눕혀 놓은 채
/// 라이브 씬을 떠나면 다음 화면이 그 자세를 그대로 물려받으므로, 처음 빌리기 직전의 값을 기록해 둡니다.
///
/// 씬에 올라오지 않은 프리팹 에셋에도 OnValidate가 전달되는데, 그 오브젝트는 파괴 시점이 없어
/// 카메라를 빌린 뒤 되돌릴 기회가 영영 없습니다. 그래서 씬에 속한 오브젝트에만 카메라를 내어 줍니다.
/// </summary>
public class LiveTrackCameraBinder
{
    private Camera _camera;

    private bool _isOrthographic;
    private float _fieldOfView;
    private float _orthographicSize;
    private float _nearClipPlane;
    private float _farClipPlane;
    private Vector3 _localPosition;
    private Quaternion _localRotation;

    /// <summary>
    /// 이 오브젝트가 쓸 카메라를 돌려줍니다. 지정된 카메라가 없으면 그 씬의 메인 카메라를 빌리며,
    /// 빌리는 순간의 상태를 한 번만 기록해 둡니다. 빌릴 수 없는 상황이면 null입니다.
    /// </summary>
    public Camera Resolve(GameObject owner, Camera serializedCamera)
    {
        if (!owner.scene.IsValid())
        {
            return null;
        }

        Camera camera = serializedCamera != null ? serializedCamera : Camera.main;

        if (camera != null)
        {
            Capture(camera);
        }

        return camera;
    }

    public void Restore()
    {
        if (_camera == null)
        {
            return;
        }

        _camera.orthographic = _isOrthographic;
        _camera.fieldOfView = _fieldOfView;
        _camera.orthographicSize = _orthographicSize;
        _camera.nearClipPlane = _nearClipPlane;
        _camera.farClipPlane = _farClipPlane;
        _camera.transform.localPosition = _localPosition;
        _camera.transform.localRotation = _localRotation;

        _camera = null;
    }

    private void Capture(Camera camera)
    {
        if (_camera != null)
        {
            return;
        }

        _camera = camera;
        _isOrthographic = camera.orthographic;
        _fieldOfView = camera.fieldOfView;
        _orthographicSize = camera.orthographicSize;
        _nearClipPlane = camera.nearClipPlane;
        _farClipPlane = camera.farClipPlane;
        _localPosition = camera.transform.localPosition;
        _localRotation = camera.transform.localRotation;
    }
}
