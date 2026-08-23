using UnityEngine;
using VInspector;

/// <summary>
/// 대본의 소리 ID를 실제 클립으로 바꾸고, 스토리 화면이 쓰는 재생 채널을 들고 있습니다.
/// 스스로 판단하는 것은 없습니다.
///
/// 표를 마스터 데이터 JSON이 아니라 인스펙터에 둔 이유는 클립이 에셋 참조이기 때문입니다.
/// JsonUtility로는 AudioClip을 직렬화할 수 없어, JSON으로 가면 경로 문자열과 런타임 로더가 따라와야 합니다.
/// 배경·초상을 StoryVisualBinder가 같은 방식으로 들고 있어 관례도 맞습니다.
///
/// 채널을 둘로 나눈 것은 수명이 다르기 때문입니다. BGM은 다음 지시까지 이어지고,
/// 효과음은 한 번 울리고 끝나며 서로 겹칠 수 있습니다.
/// </summary>
public class StoryAudioBinder : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("다음 지시까지 이어지는 채널")]
    [SerializeField]
    private AudioSource _bgmSource;

    [Header("겹쳐 울릴 수 있는 단발 채널")]
    [SerializeField]
    private AudioSource _sfxSource;

    [Foldout("Project")]
    [Header("실제 음원이 들어오면 여기부터 채웁니다")]
    [SerializeField]
    private SerializableDictionary<string, AudioClip> _bgmClips = new SerializableDictionary<string, AudioClip>();

    [SerializeField]
    private SerializableDictionary<string, AudioClip> _sfxClips = new SerializableDictionary<string, AudioClip>();

    [Foldout("Settings")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _bgmVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField]
    private float _sfxVolume = 1f;

    /// <summary>
    /// 효과음 채널을 켤지 여부입니다. 음원이 다 들어오기 전까지 꺼 둡니다.
    ///
    /// 대본은 이미 효과음을 지시하고 있는데 음원은 여섯 개뿐입니다. 그대로 켜 두면 있는 소리만
    /// 드문드문 울려 오히려 어색하고, 없는 ID마다 경고가 쌓여 진짜 문제를 덮습니다.
    ///
    /// 볼륨을 0으로 두지 않고 스위치를 따로 두는 이유는, 0이면 "소리를 줄여 둔 것"인지
    /// "아직 넣지 않은 것"인지 구분되지 않아서입니다. 다 들어오면 이 값만 켜면 됩니다.
    /// </summary>
    [SerializeField]
    private bool _isSfxEnabled;

    /// <summary>
    /// BGM이 바뀔 때 이전 곡을 줄이고 새 곡을 올리는 데 쓰는 시간입니다.
    /// 연출표가 BGM 전환을 대개 "서서히"·"디졸브"로 적어 두므로 즉시 교체하지 않습니다.
    /// </summary>
    [SerializeField]
    private float _bgmFadeSeconds = 0.8f;

    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;
    public float BgmFadeSeconds => _bgmFadeSeconds;
    public bool IsSfxEnabled => _isSfxEnabled;

    public AudioSource BgmSource => _bgmSource;

    private void Awake()
    {
        // 대본이 곡을 지정하기 전까지는 아무 소리도 나지 않아야 합니다.
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = 0f;

        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = _sfxVolume;

        // 재생을 막는 것과 별개로 소스도 함께 음소거합니다. 인스펙터만 보고도
        // 이 채널이 꺼져 있다는 것을 알 수 있어야 합니다.
        _sfxSource.mute = !_isSfxEnabled;
    }

    public AudioClip GetBgm(string bgmId)
    {
        return TryGet(_bgmClips, bgmId);
    }

    public AudioClip GetSfx(string sfxId)
    {
        return TryGet(_sfxClips, sfxId);
    }

    /// <summary>
    /// 효과음을 한 번 울립니다. PlayOneShot을 쓰는 것은 겹쳐 울리는 소리를 위해서입니다.
    /// clip을 대입해 Play하면 앞서 울리던 소리가 끊기고, 소스를 늘리려면 오브젝트를 동적으로 만들어야 합니다.
    /// </summary>
    public void PlaySfx(AudioClip clip)
    {
        if (!_isSfxEnabled)
        {
            return;
        }

        _sfxSource.PlayOneShot(clip, _sfxVolume);
    }

    private AudioClip TryGet(SerializableDictionary<string, AudioClip> table, string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return table.TryGetValue(id, out AudioClip clip) ? clip : null;
    }
}
