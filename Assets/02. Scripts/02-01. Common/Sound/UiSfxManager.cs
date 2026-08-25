using UnityEngine;
using VInspector;

/// <summary>
/// 버튼 조작음을 재생합니다.
///
/// BgmManager와 나눈 이유는 소리의 규칙이 다르기 때문입니다. BGM은 한 곡을 이어 틀고
/// 다음 곡이 앞 곡을 밀어내지만, 조작음은 짧게 여러 개가 겹쳐 울립니다.
/// 한 AudioSource에 clip을 갈아 끼우는 방식으로는 앞 소리가 잘립니다. PlayOneShot을 씁니다.
///
/// PersistentScene에 상주하는 것은 BGM과 같은 이유입니다. 화면마다 두면 씬을 오갈 때
/// 재생기가 사라졌다 생기고, 화면 전환을 일으킨 버튼의 소리가 자기 화면과 함께 잘립니다.
/// </summary>
public class UiSfxManager : MonoBehaviourSingleton<UiSfxManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private AudioSource _source;

    [Foldout("Project")]
    [Header("EUiSfx에 대응하는 음원을 채웁니다")]
    [SerializeField]
    private SerializableDictionary<EUiSfx, AudioClip> _clips = new SerializableDictionary<EUiSfx, AudioClip>();

    [Foldout("Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _volume = 0.7f;

    protected override void Awake()
    {
        base.Awake();

        _source.loop = false;
        _source.playOnAwake = false;
    }

    /// <summary>
    /// 소리를 한 번 울립니다. 음원이 없는 값은 조용히 넘깁니다.
    /// 조작음이 없다고 버튼이 동작하지 않을 이유는 없습니다.
    /// </summary>
    public void Play(EUiSfx sfx)
    {
        if (sfx == EUiSfx.NONE || !_clips.TryGetValue(sfx, out AudioClip clip) || clip == null)
        {
            return;
        }

        _source.PlayOneShot(clip, _volume);
    }

    /// <summary>
    /// 음량을 바꿉니다. 설정 화면이 생기면 여기로 들어옵니다.
    /// </summary>
    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
    }
}
