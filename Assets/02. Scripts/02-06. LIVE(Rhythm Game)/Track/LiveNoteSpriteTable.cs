using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레인 번호에 대응하는 노트 스프라이트 묶음입니다. 노트 그림은 레인마다 폭이 달라(시안 기준 239~243px)
/// 종류별 프리팹 하나로는 맞출 수 없으므로, 종류가 아니라 레인으로 그림을 고릅니다.
/// 6번 레인이 곧 귀신 레인이라 귀신 노트도 자동으로 구분됩니다.
/// 현재 들어와 있는 6종은 색이 모두 같고 폭만 다릅니다.
///
/// 노트 풀과 렌더러가 POCO라 인스펙터에 직접 노출될 수 없으므로,
/// LiveNoteRenderSettings와 마찬가지로 LiveTrackScroller가 이 묶음을 직렬화해 생성 시 넘깁니다.
/// </summary>
[Serializable]
public class LiveNoteSpriteTable
{
    [Tooltip("레인 1번부터 순서대로 넣습니다. 비어 있는 칸은 노트 프리팹의 기본 표시를 그대로 씁니다.")]
    public List<Sprite> LaneSprites = new List<Sprite>();

    [Tooltip("롱노트 몸통입니다. 세로로 반복되므로 아틀라스가 아닌 Wrap Mode = Repeat 단독 텍스처여야 합니다. 몸통 아트가 아직 없어 비어 있는 것이 정상이며, 그동안은 P_UI_Live_NoteLong의 Body Color가 띠 색을 냅니다. 레인 노트 스프라이트는 Wrap Mode가 Clamp이고 모서리가 둥근 평행사변형이라 여기에 넣으면 안 됩니다.")]
    public List<Sprite> HoldBodySprites = new List<Sprite>();

    public Sprite GetSprite(int lane)
    {
        return GetSpriteAt(LaneSprites, lane);
    }

    public Sprite GetHoldBodySprite(int lane)
    {
        return GetSpriteAt(HoldBodySprites, lane);
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
