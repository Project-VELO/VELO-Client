using System.Collections.Generic;

/// <summary>
/// 마스터 데이터 검사 결과를 모읍니다.
///
/// 오류와 경고를 나누는 기준은 "1주차 데모 루프를 끝까지 돌 수 있는가"입니다.
/// 진행이 막히면 오류, 화면 표시가 어색해지는 정도면 경고입니다.
/// </summary>
public class MasterDataValidationReport
{
    private readonly List<string> _errors = new List<string>();
    private readonly List<string> _warnings = new List<string>();
    private readonly List<string> _summaries = new List<string>();

    public IReadOnlyList<string> Errors => _errors;
    public IReadOnlyList<string> Warnings => _warnings;
    public IReadOnlyList<string> Summaries => _summaries;

    public bool HasError => 0 < _errors.Count;

    public void AddError(string message)
    {
        _errors.Add(message);
    }

    public void AddWarning(string message)
    {
        _warnings.Add(message);
    }

    public void AddSummary(string message)
    {
        _summaries.Add(message);
    }
}
