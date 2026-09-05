using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 밖 화면의 배경음을 재생합니다.
///
/// PersistentScene에 상주하는 이유는 화면을 오갈 때 곡이 끊기지 않아야 하기 때문입니다.
/// 화면마다 재생기를 두면 홈에서 스페이스로 갔다 돌아올 때마다 전주가 다시 나옵니다.
///
/// 같은 곡을 다시 틀라는 지시는 무시합니다. 화면마다 자기가 쓸 곡을 말하는 구조라
/// 무시하지 않으면 화면을 옮길 때마다 곡이 처음으로 돌아갑니다.
///
/// 스토리와 라이브는 여기를 쓰지 않고 자기 재생기를 갖습니다. 두 화면은 들어올 때
/// EBgm.NONE으로 이 곡을 멈춥니다.
/// </summary>
public class BgmManager : MonoBehaviourSingleton<BgmManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private AudioSource _source;

    [Foldout("Project")]
    [Header("EBgm에 대응하는 음원을 채웁니다")]
    [SerializeField]
    private SerializableDictionary<EBgm, AudioClip> _clips = new SerializableDictionary<EBgm, AudioClip>();

    [Foldout("Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _volume = 0.5f;

    /// <summary>
    /// 미리듣기가 시작에서 차오르고 끝에서 잦아드는 데 걸리는 시간입니다.
    /// 짧은 음원을 반복해 트는 자리라 매 바퀴 이 봉투가 다시 그려집니다.
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float _previewFadeSeconds = 1.5f;

    /// <summary>
    /// 지금 흐르고 있는 곡입니다. 같은 곡을 다시 트는 것을 막는 기준입니다.
    /// </summary>
    private EBgm _current = EBgm.NONE;

    /// <summary>
    /// 표에 없는 음원이 대신 흐르고 있는지입니다(곡 선택 화면의 미리듣기).
    ///
    /// 이 값이 없으면 미리듣기 뒤에 원래 곡을 되돌릴 수 없습니다. _current는 미리듣기 중에도
    /// 원래 곡을 가리키고 있어, 같은 곡을 다시 틀라는 지시로 보여 무시되기 때문입니다.
    /// </summary>
    private bool _isPreviewPlaying;

    private BgmPreviewFade _previewFade;

    protected override void Awake()
    {
        base.Awake();

        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = _volume;
    }

    /// <summary>
    /// 이 곡으로 바꿉니다. 이미 같은 곡이 흐르고 있으면 아무것도 하지 않습니다.
    /// 음원이 아직 없는 값이 들어오면 조용히 멈춥니다. 곡이 준비되기 전에도 화면은 돌아가야 합니다.
    /// </summary>
    public void Play(EBgm bgm)
    {
        if (bgm == _current && !_isPreviewPlaying)
        {
            return;
        }

        _current = bgm;
        _isPreviewPlaying = false;

        if (bgm == EBgm.NONE || !_clips.TryGetValue(bgm, out AudioClip clip) || clip == null)
        {
            _source.Stop();
            _source.clip = null;
            return;
        }

        _source.clip = clip;
        _source.volume = _volume;
        _source.Play();
    }

    public void Stop()
    {
        Play(EBgm.NONE);
    }

    /// <summary>
    /// 표에 없는 음원을 대신 틉니다. 곡 선택 화면에서 고른 곡을 들려줄 때 씁니다.
    ///
    /// 원래 곡이 무엇이었는지는 _current가 그대로 들고 있으므로, RestoreScreenBgm으로 되돌립니다.
    /// </summary>
    public void PlayPreview(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        _isPreviewPlaying = true;

        _source.clip = clip;
        _source.Play();

        GetPreviewFade().Begin(_volume, this.GetCancellationTokenOnDestroy());
    }


    /// <summary>
    /// 미리듣기를 멈추고 이 화면이 원래 쓰던 곡으로 되돌립니다.
    /// 미리듣기 중이 아니면 아무것도 하지 않아, 흐르던 곡이 처음으로 돌아가지 않습니다.
    /// </summary>
    public void RestoreScreenBgm()
    {
        if (!_isPreviewPlaying)
        {
            return;
        }

        GetPreviewFade().Stop();

        EBgm screenBgm = _current;

        // 같은 값이라 무시되지 않도록 비워 두고 다시 지시합니다.
        _current = EBgm.NONE;
        _isPreviewPlaying = false;

        Play(screenBgm);
    }

    /// <summary>
    /// 음량 곡선을 그리는 쪽입니다. 인스펙터 값이 바뀔 수 있어 처음 쓸 때 만듭니다.
    /// </summary>
    private BgmPreviewFade GetPreviewFade()
    {
        return _previewFade ??= new BgmPreviewFade(_source, _previewFadeSeconds);
    }

    /// <summary>
    /// 음량을 바꿉니다. 설정 화면이 생기면 여기로 들어옵니다.
    /// </summary>
    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
        _source.volume = _volume;
    }
}
