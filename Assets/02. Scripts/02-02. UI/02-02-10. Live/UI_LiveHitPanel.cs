using UnityEngine;
using TMPro;
using VInspector;

public class UI_LiveHitPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private TMP_Text _judgmentText;

    public void SetJudgmentText(string text)
    {
        _judgmentText.text = text;
    }
}
