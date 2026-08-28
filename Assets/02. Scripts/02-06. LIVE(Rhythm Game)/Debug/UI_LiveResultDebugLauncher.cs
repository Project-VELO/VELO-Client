using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 결과 화면을 바로 띄우는 개발용 버튼 묶음입니다.
///
/// 랭크별 화면과 실패 화면을 확인하려면 매번 곡을 끝까지 쳐야 하고, 원하는 랭크가 나오도록
/// 일부러 틀리는 것은 사실상 불가능합니다. 결과 화면을 손볼 때마다 다시 필요하므로 남겨 둡니다.
///
/// 정식 빌드에서는 Awake가 스스로 꺼 주므로 그대로 두어도 출시본에 따라가지 않습니다.
/// </summary>
public class UI_LiveResultDebugLauncher : MonoBehaviour
{
    [Serializable]
    private struct RankButton
    {
        public Button Button;
        public ELiveRank Rank;
    }

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<RankButton> _rankButtons = new List<RankButton>();

    private void Awake()
    {
        // 정식 빌드에 임시 버튼이 따라가면 안 됩니다. 에디터와 개발 빌드에서만 남깁니다.
        if (!Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }

        InitButtons();
    }

    private void InitButtons()
    {
        for (int i = 0; i < _rankButtons.Count; i++)
        {
            RankButton entry = _rankButtons[i];

            if (entry.Button == null)
            {
                continue;
            }

            // 람다가 반복 변수를 붙잡지 않도록 값을 복사해 넘깁니다.
            ELiveRank rank = entry.Rank;

            entry.Button.onClick.AddListener(() => ShowResult(rank));
        }
    }

    private void ShowResult(ELiveRank rank)
    {
        LiveResultContext.Instance.SetResult(LiveResultDebugSample.Create(rank));

        LiveSceneNavigator.LoadScene(ESceneNames.LiveResultScene,
            this.GetCancellationTokenOnDestroy(), nameof(UI_LiveResultDebugLauncher));
    }
}
