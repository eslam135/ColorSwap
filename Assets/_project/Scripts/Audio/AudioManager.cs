using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _buttonClickClip;
    [SerializeField] private AudioClip _dragStartClip;
    [SerializeField] private AudioClip _swapClip;
    [SerializeField] private AudioClip _invalidMoveClip;
    [SerializeField] private AudioClip _levelCompleteClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
        }

        _sfxSource.playOnAwake = false;
    }

    public void PlayButtonClick()
    {
        PlaySfx(_buttonClickClip, 1f);
    }

    public void PlayDragStart()
    {
        PlaySfx(_dragStartClip, 0.75f);
    }

    public void PlaySwap()
    {
        PlaySfx(_swapClip, 1f);
    }

    public void PlayInvalidMove()
    {
        PlaySfx(_invalidMoveClip, 1f);
    }

    public void PlayLevelComplete()
    {
        PlaySfx(_levelCompleteClip, 1f);
    }

    public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || _sfxSource == null)
        {
            return;
        }

        _sfxSource.PlayOneShot(clip, _sfxVolume * volumeMultiplier);
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }
}