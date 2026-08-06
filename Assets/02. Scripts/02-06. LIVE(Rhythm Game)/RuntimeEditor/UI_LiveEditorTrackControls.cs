using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 스냅 분박 선택과 현재 마디 표시를 담당하는 UGUI 패널입니다.
/// 배속과 하이스피드는 버튼으로 미세 조절해야 해서 각각 전용 컨트롤 클래스가 담당합니다.
/// </summary>
public class UI_LiveEditorTrackControls : MonoBehaviour
{
    private const int UNBOUND_BAR_INDEX = -1;

    private static readonly List<ESnapDivision> SnapDivisions = new List<ESnapDivision>
    {
        ESnapDivision.Quarter,
        ESnapDivision.Eighth,
        ESnapDivision.Twelfth,
        ESnapDivision.Sixteenth,
        ESnapDivision.ThirtySecond,
    };

    private static readonly List<string> SnapOptions = new List<string>
    {
        "스냅: 1/4박", "스냅: 1/8박", "스냅: 1/12박", "스냅: 1/16박", "스냅: 1/32박",
    };

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Dropdown _snapDropdown;

    [SerializeField]
    private TMP_Text _barIndicatorText;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    private int _lastShownBarIndex = UNBOUND_BAR_INDEX;
    private int _lastShownBarCount = UNBOUND_BAR_INDEX;

    private void Awake()
    {
        _snapDropdown.ClearOptions();
        _snapDropdown.AddOptions(SnapOptions);
        _snapDropdown.SetValueWithoutNotify(SnapDivisions.IndexOf(_timeline.SnapDivision));
        _snapDropdown.onValueChanged.AddListener(OnSnapChanged);
    }

    private void Update()
    {
        RefreshBarIndicator();
    }

    /// <summary>
    /// 매 프레임 문자열을 새로 만들면 GC가 발생하므로, 표시 중인 마디나 총 마디 수가 실제로 바뀌었을 때만 갱신합니다.
    /// </summary>
    private void RefreshBarIndicator()
    {
        if (!_timeline.BarLayout.IsBuilt)
        {
            return;
        }

        int barIndex = _timeline.GetCurrentBarIndex();
        int barCount = _timeline.BarLayout.BarCount;

        if (barIndex == _lastShownBarIndex && barCount == _lastShownBarCount)
        {
            return;
        }

        _lastShownBarIndex = barIndex;
        _lastShownBarCount = barCount;
        _barIndicatorText.text = $"마디: {barIndex + 1} / {barCount}";
    }

    private void OnSnapChanged(int index)
    {
        if (index < 0 || SnapDivisions.Count <= index)
        {
            return;
        }

        _timeline.SnapDivision = SnapDivisions[index];
    }
}
