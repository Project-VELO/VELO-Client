using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 화면의 스토리 바로가기 상자입니다(기획서 SCREEN-001).
///
/// 제목과 버튼 문구는 그림으로 박혀 있어 바뀌지 않고, 여기서 갱신하는 것은 챕터 표기 한 줄뿐입니다.
/// 마지막으로 본 회차의 다음 화 — 곧 아직 완료하지 않은 가장 앞선 회차 — 가 속한 챕터를 적어,
/// 상자를 눌렀을 때 어디로 이어지는지 미리 보이게 합니다.
///
/// 다음 화를 앞에서부터 훑어 찾는 것은 목록 순서가 곧 진행 순서이기 때문입니다
/// (기획서 3-F-3-1, MasterDataQuery.GetAllStoriesInDisplayOrder가 챕터 순서 → 회차 순서를 보장).
/// 완료 여부를 뒤에서부터 세면 중간에 건너뛰고 본 회차가 있을 때 엉뚱한 챕터를 가리킵니다.
/// </summary>
public class UI_HomeStoryBox : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _chapterText;

    public void Refresh()
    {
        if (_chapterText == null)
        {
            return;
        }

        StoryData nextStory = FindNextStory();
        _chapterText.text = StoryChapterDisplayName.Get(nextStory?.ChapterId);
    }

    /// <summary>
    /// 아직 완료하지 않은 첫 회차를 돌려줍니다. 전부 완료했다면 마지막 회차를 그대로 씁니다.
    ///
    /// 완주 뒤에 빈칸으로 두면 상자에 이유를 알 수 없는 공백이 생기고, "완료" 같은 문구를 새로
    /// 만들면 기획에 없는 문자열이 화면에 나옵니다. 마지막 챕터를 남겨 두면 상자를 눌렀을 때
    /// 도착하는 목록의 끝자락과도 어긋나지 않습니다.
    /// </summary>
    private static StoryData FindNextStory()
    {
        List<StoryData> stories = MasterDataQuery.GetAllStoriesInDisplayOrder();

        if (stories.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < stories.Count; i++)
        {
            IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(stories[i].StoryId);

            if (progress == null || !progress.IsCompleted)
            {
                return stories[i];
            }
        }

        return stories[stories.Count - 1];
    }
}
