using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 화면의 스토리 카드입니다(개발 기획서 v1.15 2장 H02·H03).
///
/// 제목과 버튼 문구는 고정이고, 여기서 갱신하는 것은 챕터·회차 한 줄과 진행 중 배지입니다.
/// 아직 완료하지 않은 가장 앞선 회차를 적어, 이어하기를 눌렀을 때 어디로 이어지는지 미리 보이게 합니다.
/// 이어하기가 실제로 들어가는 회차도 NextStory를 그대로 씁니다. 카드에 적힌 회차와 들어가는 회차가
/// 따로 계산되면 둘이 어긋날 수 있기 때문입니다.
///
/// 다음 화를 앞에서부터 훑어 찾는 것은 목록 순서가 곧 진행 순서이기 때문입니다
/// (기획서 3-F-3-1, MasterDataQuery.GetAllStoriesInDisplayOrder가 챕터 순서 → 회차 순서를 보장).
/// 완료 여부를 뒤에서부터 세면 중간에 건너뛰고 본 회차가 있을 때 엉뚱한 챕터를 가리킵니다.
/// </summary>
public class UI_HomeStoryBox : MonoBehaviour
{
    /// <summary>
    /// 스토리 선택 화면의 회차 카드(UI_SelectStoryEpisodeItem)와 같은 원문입니다. 번역표는 한국어 원문을
    /// 키로 쓰므로, 한 글자라도 다르면 일본어에서 이 자리만 "화"가 그대로 남습니다.
    /// </summary>
    private const string EPISODE_FORMAT = "{0}화";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _chapterText;

    [SerializeField]
    private GameObject _inProgressBadge;

    /// <summary>
    /// 이어서 볼 회차입니다. 전부 완료했다면 마지막 회차이고, 스토리가 하나도 없으면 비어 있습니다.
    /// </summary>
    public StoryData NextStory { get; private set; }

    public void Refresh()
    {
        NextStory = FindNextStory();
        _chapterText.text = BuildLabel(NextStory);
        RefreshInProgressBadge();
    }

    /// <summary>
    /// "CHAPTER 0 2화"처럼 챕터와 회차를 함께 적습니다.
    ///
    /// 챕터만 적으면 한 챕터를 보는 동안 이 줄이 한 번도 바뀌지 않아, 어디까지 봤는지
    /// 상자만 보고는 알 수 없습니다. 회차 번호는 stories.json이 0부터 매기며 화면 표기와 같습니다.
    /// </summary>
    private static string BuildLabel(StoryData story)
    {
        string chapter = StoryChapterDisplayName.Get(story?.ChapterId);

        if (ReferenceEquals(story, null))
        {
            return chapter;
        }

        return $"{chapter} {string.Format(UiText.Localize(EPISODE_FORMAT), story.EpisodeNumber)}";
    }

    /// <summary>
    /// 들어갔다가 끝내지 않은 회차에만 "진행 중"을 붙입니다(개발 기획서 5장 회차 상태).
    ///
    /// NEW 배지는 감상 화면에 들어가는 순간 내려가므로, NEW가 아니면서 완료하지 않은 회차가 곧 진행 중입니다.
    /// 처음 보는 회차·잠긴 회차·전부 완료한 뒤에 쓸 배지 문구는 아직 정해지지 않아 그때는 숨깁니다.
    /// </summary>
    private void RefreshInProgressBadge()
    {
        IStoryProgress progress = ReferenceEquals(NextStory, null)
            ? null
            : GameProgressService.Instance.GetStoryProgress(NextStory.StoryId);

        bool isInProgress = !ReferenceEquals(progress, null) && progress.CanEnter && !progress.IsNew && !progress.IsCompleted;
        _inProgressBadge.SetActive(isInProgress);
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
