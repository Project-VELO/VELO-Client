using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레인 번호에 대응하는 노트 스프라이트 묶음입니다. 디자인상 노트 색은 노트 종류가 아니라 레인으로 정해지므로,
/// 종류별 프리팹이 아니라 이 표에서 색을 가져옵니다. 6번 레인이 곧 귀신 레인이라 귀신 노트도 자동으로 구분됩니다.
///
/// 노트 풀과 렌더러가 POCO라 인스펙터에 직접 노출될 수 없으므로,
/// LiveNoteRenderSettings와 마찬가지로 LiveTrackScroller가 이 묶음을 직렬화해 생성 시 넘깁니다.
/// </summary>
[Serializable]
public class LiveNoteSpriteTable
{
    [Tooltip("레인 1번부터 순서대로 넣습니다. 비어 있는 칸은 노트 프리팹의 기본 표시를 그대로 씁니다.")]
    public List<Sprite> LaneSprites = new List<Sprite>();

    [Tooltip("롱노트 몸통입니다. 세로로 반복되므로 아틀라스가 아닌 Wrap Mode = Repeat 단독 텍스처여야 합니다.")]
    public List<Sprite> HoldBodySprites = new List<Sprite>();

    [Tooltip("롱노트 꼬리입니다.")]
    public List<Sprite> HoldTailSprites = new List<Sprite>();

    public Sprite GetSprite(int lane)
    {
        return GetSpriteAt(LaneSprites, lane);
    }

    public Sprite GetHoldBodySprite(int lane)
    {
        return GetSpriteAt(HoldBodySprites, lane);
    }

    public Sprite GetHoldTailSprite(int lane)
    {
        return GetSpriteAt(HoldTailSprites, lane);
    }

    private static Sprite GetSpriteAt(List<Sprite> sprites, int lane)
    {
        int index = lane - LiveLane.FIRST;

        if (index < 0 || sprites.Count <= index)
        {
            return null;
        }

        return sprites[index];
    }
}
