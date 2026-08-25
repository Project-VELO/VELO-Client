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
    /// 지금 흐르고 있는 곡입니다. 같은 곡을 다시 트는 것을 막는 기준입니다.
    /// </summary>
    private EBgm _current = EBgm.NONE;

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
        if (bgm == _current)
        {
            return;
        }

        _current = bgm;

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
    /// 음량을 바꿉니다. 설정 화면이 생기면 여기로 들어옵니다.
    /// </summary>
    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
        _source.volume = _volume;
    }
}
