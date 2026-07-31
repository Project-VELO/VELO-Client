using UnityEngine;
using VInspector;

/// <summary>
/// 결과 화면의 성취 태그(PERFECT S / NEW BEST / FULL COMBO)를 조건에 맞춰 켭니다(SCREEN-010 13.2).
/// </summary>
public class UI_LiveResultTags : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Tags")]
    [SerializeField]
    private GameObject _perfectSTag;

    [SerializeField]
    private GameObject _newBestTag;

    [SerializeField]
    private GameObject _fullComboTag;

    public void RefreshTags(LiveResultData result)
    {
        SetActive(_perfectSTag, result.Rank == ELiveRank.PERFECT_S);
        SetActive(_newBestTag, result.IsNewBest);

        // FULL COMBO는 BAD 0회가 조건이므로(3-I-8), 완주하지 못한 판에서는 성립하지 않습니다.
        SetActive(_fullComboTag, result.IsClear && result.IsFullCombo);
    }

    private static void SetActive(GameObject tag, bool isActive)
    {
        if (tag == null)
        {
            return;
        }

        tag.SetActive(isActive);
    }
}
