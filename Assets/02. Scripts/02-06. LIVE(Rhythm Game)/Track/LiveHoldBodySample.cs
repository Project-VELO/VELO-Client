/// <summary>
/// 롱노트 몸통을 이루는 가로 한 줄입니다. 좌표는 노트 원점(머리 마커의 중심) 기준의 로컬 좌표입니다.
/// 트랙이 사다리꼴이라 줄마다 폭도 중심도 달라지므로, 길이 하나로는 몸통 모양을 적을 수 없어 줄 단위로 넘깁니다.
/// </summary>
public readonly struct LiveHoldBodySample
{
    /// <summary>
    /// 몸통 하나가 가질 수 있는 최대 줄 수입니다. 만드는 쪽과 그리는 쪽이 이 크기로 버퍼를 미리 잡아 두어야
    /// 매 프레임 다시 채우는 동안 내부 배열이 새로 할당되지 않습니다.
    /// </summary>
    public const int MAX_COUNT = 33;

    public readonly float LeftX;
    public readonly float RightX;
    public readonly float LocalY;

    /// <summary>
    /// 몸통 텍스처의 세로 좌표입니다. 머리에서부터 잰 화면 거리를 타일 길이로 나눈 값이라 무늬가 노트에 붙어 따라옵니다.
    /// </summary>
    public readonly float V;

    public LiveHoldBodySample(float leftX, float rightX, float localY, float v)
    {
        LeftX = leftX;
        RightX = rightX;
        LocalY = localY;
        V = v;
    }
}
