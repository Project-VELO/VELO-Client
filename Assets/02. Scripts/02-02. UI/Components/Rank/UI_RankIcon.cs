using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 랭크를 등급 아트로 보여 줍니다.
///
/// 아트에 등급 글자가 이미 그려져 있으므로 글자를 위에 얹지 않습니다. 아트와 글자를 함께
/// 띄우면 같은 글자가 두 겹으로 찍힙니다. 대신 아트가 없는 등급(FAILED)만 글자로 알립니다.
/// </summary>
public class UI_RankIcon : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _iconImage;

    /// <summary>
    /// 아트가 없는 등급을 대신 알리는 글자입니다. 비워 두면 그런 등급에서 아무것도 표시하지 않습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _fallbackText;

    [Foldout("Project")]
    [Header("Rank Icons")]
    [SerializeField]
    private Sprite _perfectSIcon;

    [SerializeField]
    private Sprite _sIcon;

    [SerializeField]
    private Sprite _aIcon;

    [SerializeField]
    private Sprite _bIcon;

    [SerializeField]
    private Sprite _cIcon;

    public void RefreshRank(ELiveRank rank)
    {
        Sprite icon = GetIcon(rank);

        SetIcon(icon);
        SetFallbackText(icon == null ? rank.ToString() : string.Empty);
    }

    /// <summary>
    /// 아직 등급이 없는 대상(한 번도 클리어하지 않은 곡 등)의 자리를 비웁니다.
    /// FAILED와 달리 알릴 등급 자체가 없으므로 대체 글자도 띄우지 않습니다.
    /// </summary>
    public void Clear()
    {
        SetIcon(null);
        SetFallbackText(string.Empty);
    }

    /// <summary>
    /// 빈 스프라이트를 남기면 흰 사각형이 그대로 노출되므로 컴포넌트를 함께 끕니다.
    /// </summary>
    private void SetIcon(Sprite icon)
    {
        _iconImage.sprite = icon;
        _iconImage.enabled = icon != null;
    }

    private void SetFallbackText(string value)
    {
        if (_fallbackText == null)
        {
            return;
        }

        _fallbackText.text = value;
    }

    private Sprite GetIcon(ELiveRank rank)
    {
        switch (rank)
        {
            case ELiveRank.PERFECT_S:
                return _perfectSIcon;

            case ELiveRank.S:
                return _sIcon;

            case ELiveRank.A:
                return _aIcon;

            case ELiveRank.B:
                return _bIcon;

            case ELiveRank.C:
                return _cIcon;

            default:
                return null;
        }
    }
}
