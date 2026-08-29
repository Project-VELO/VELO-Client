using System;
using UnityEngine;

/// <summary>
/// 감상 화면에서 인물 한 명을 어떻게 세울지 정하는 값입니다.
///
/// 그림마다 인물이 차지하는 비율이 달라 한 크기로 묶을 수 없습니다.
/// 일러스트가 있는 인물은 1200px 폭으로 화면 끝에 붙지만, 실루엣만 있는 인물이나
/// 사람이 아닌 인물은 그보다 작고 끝에서 얼마쯤 띄워야 화면에 어울립니다.
/// </summary>
[Serializable]
public struct StoryCharacterLayout
{
    /// <summary>
    /// 화면에 그릴 너비입니다. 0이면 그림의 원본 너비를 그대로 씁니다.
    /// 높이는 원본 비율에서 계산하므로 따로 적지 않습니다.
    /// </summary>
    [Min(0)]
    public int Width;

    /// <summary>
    /// 자기가 선 쪽 화면 끝에서 띄우는 거리입니다. 가운데 자리에는 쓰이지 않습니다.
    /// </summary>
    [Min(0)]
    public int EdgeOffset;
}
