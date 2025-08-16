using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all audio playback and settings for the LetterGrid game.
/// Responds only to events via LetterGridGameAudioEvents.
/// </summary>
public class LetterGridAudioManager : MonoBehaviour {
    public static LetterGridAudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgMusicSource;      // Assign in Inspector
    [SerializeField] private AudioSource sfxSource;          // Assign in Inspector

    [Header("🎵 Background Music")]
    [SerializeField] private List<AudioClip> bgMusicClips;   // Assign in Inspector

    [Header("🔊 Sound Effects")]
    [SerializeField] private AudioClip clickClip;            // Assign in Inspector
    [SerializeField] private AudioClip tileAddClip;          // Assign in Inspector

    [Header("🎉 Event SFX")]
    [SerializeField] private AudioClip levelSuccessClip;      // Assign in Inspector
    [SerializeField] private AudioClip correctMoveClip;  // Assign in Inspector
    [SerializeField] private AudioClip wrongMoveClip;    // Assign in Inspector
    [SerializeField] private AudioClip tileFlipClip;    // Assign in Inspector

    [Header("🔊 Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool isMusicMuted = false;
    [SerializeField] private bool isSFXMuted = false;
    [SerializeField] private bool isAllMuted = false;

    [SerializeField] private float fadeDuration = 1.0f; // How long fades take (seconds)
    private int currentTrackIndex = -1;
    

    private Coroutine musicFadeCoroutine;
    private Coroutine duckMusicCoroutine;
    private bool isFadingMusic = false;



    // --- Unity Events ---
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // music persists
        }
        else if (Instance != this) {
            Destroy(gameObject);
            return; // <-- skip any further init
        }

        if (bgMusicSource == sfxSource && bgMusicSource != null) {
            Debug.LogError("LetterGridAudioManager: bgMusicSource and sfxSource must NOT be the same AudioSource! Please assign different AudioSources for music and SFX.");
        }
    }

    void OnValidate() {
        if (bgMusicSource == sfxSource && bgMusicSource != null) {
            Debug.LogError("LetterGridAudioManager: bgMusicSource and sfxSource must NOT be the same AudioSource! Please assign different AudioSources for music and SFX.");
        }
    }


    void OnEnable() {
        LetterGridGameAudioEvents.OnClick += PlayClick;
        LetterGridGameAudioEvents.OnTileAdded += PlayTileAdd;
        LetterGridGameAudioEvents.OnStartGame += PlayGameMusic; 
        LetterGridGameAudioEvents.OnBackToMenu += PlayMenuMusic; 
        LetterGridGameAudioEvents.OnLevelSuccess += PlayLevelSuccessSFX;     
        LetterGridGameAudioEvents.OnMoveCorrect += PlayCorrectMoveSFX;  
        LetterGridGameAudioEvents.OnMoveWrong += PlayWrongMoveSFX;  
        LetterGridGameAudioEvents.OnTileFlip += PlayTileFlipSFX;  
        LetterGridGameAudioEvents.OnMusicVolumeChanged += SetMusicVolume;
        LetterGridGameAudioEvents.OnSFXVolumeChanged += SetSFXVolume;
        LetterGridGameAudioEvents.OnMusicMuteChanged += SetMusicMute;
        LetterGridGameAudioEvents.OnSFXMuteChanged += SetSFXMute;
        LetterGridGameAudioEvents.OnAllMuteChanged += SetAllMute;
        LetterGridGameAudioEvents.OnInitRequest += SendInitialSettingsState;
    }

    void OnDisable() {
        LetterGridGameAudioEvents.OnClick -= PlayClick;
        LetterGridGameAudioEvents.OnTileAdded -= PlayTileAdd;
        LetterGridGameAudioEvents.OnStartGame -= PlayGameMusic; 
        LetterGridGameAudioEvents.OnBackToMenu -= PlayMenuMusic; 
        LetterGridGameAudioEvents.OnLevelSuccess -= PlayLevelSuccessSFX;
        LetterGridGameAudioEvents.OnMoveCorrect -= PlayCorrectMoveSFX;
        LetterGridGameAudioEvents.OnMoveWrong -= PlayWrongMoveSFX;
        LetterGridGameAudioEvents.OnTileFlip -= PlayTileFlipSFX;
        LetterGridGameAudioEvents.OnMusicVolumeChanged -= SetMusicVolume;
        LetterGridGameAudioEvents.OnSFXVolumeChanged -= SetSFXVolume;
        LetterGridGameAudioEvents.OnMusicMuteChanged -= SetMusicMute;
        LetterGridGameAudioEvents.OnSFXMuteChanged -= SetSFXMute;
        LetterGridGameAudioEvents.OnAllMuteChanged -= SetAllMute;
        LetterGridGameAudioEvents.OnInitRequest -= SendInitialSettingsState;
    }

    private void Start() {
        PlayMenuMusic();
    }



    // --- Private Event Listeners (Only Accessible By Event Subscription) ---

    private void PlayMusicByIndex(int idx) {
        if (bgMusicClips == null || bgMusicClips.Count == 0 || idx < 0 || idx >= bgMusicClips.Count) return;
        if (bgMusicSource == null) return;
        if (currentTrackIndex == idx && bgMusicSource.isPlaying) return;

        currentTrackIndex = idx;
        bgMusicSource.clip = bgMusicClips[idx];
        bgMusicSource.loop = true;
        bgMusicSource.Play();
        UpdateVolumes();
    }

    private void PlayMenuMusic() => FadeToMusicIndex(0); // [0] = menu
    private void PlayGameMusic() => FadeToMusicIndex(1); // [1] = game

    private void PlayClick() {
        if (sfxSource && clickClip) sfxSource.PlayOneShot(clickClip);
    }
    private void PlayTileAdd() {
        if (sfxSource && tileAddClip) sfxSource.PlayOneShot(tileAddClip);
    }
    private void PlayLevelSuccessSFX() {
        if (sfxSource && levelSuccessClip) {
            // Stop any current ducking to avoid overlap
            if (duckMusicCoroutine != null)
                StopCoroutine(duckMusicCoroutine);

            duckMusicCoroutine = StartCoroutine(DuckMusicWhileSFX(levelSuccessClip));
        }
    }

    private void PlayCorrectMoveSFX() {
        if (sfxSource && correctMoveClip) sfxSource.PlayOneShot(correctMoveClip);
    }
    private void PlayWrongMoveSFX() {
        if (sfxSource && wrongMoveClip) sfxSource.PlayOneShot(wrongMoveClip);
    }

    private void PlayTileFlipSFX() {
        if (sfxSource && tileFlipClip) sfxSource.PlayOneShot(tileFlipClip);
    }

    private void SetMusicVolume(float vol) {
        musicVolume = Mathf.Clamp01(vol);
        UpdateVolumes();
    }
    private void SetSFXVolume(float vol) {
        sfxVolume = Mathf.Clamp01(vol);
        UpdateVolumes();
    }
    private void SetMusicMute(bool muted) {
        isMusicMuted = muted;
        UpdateVolumes();
    }
    private void SetSFXMute(bool muted) {
        isSFXMuted = muted;
        UpdateVolumes();
    }
    private void SetAllMute(bool muted) {
        isAllMuted = muted;
        UpdateVolumes();
    }

    private void UpdateVolumes() {
        if (isFadingMusic) return; // Don't update during a fade!
        float finalMusicVol = isAllMuted || isMusicMuted ? 0f : musicVolume;
        float finalSFXVol = isAllMuted || isSFXMuted ? 0f : sfxVolume;
        if (bgMusicSource) bgMusicSource.volume = finalMusicVol;
        if (sfxSource) sfxSource.volume = finalSFXVol;
    }

    private void SendInitialSettingsState() {
        LetterGridGameAudioEvents.RaiseMusicVolumeInit(musicVolume);
        LetterGridGameAudioEvents.RaiseSFXVolumeInit(sfxVolume);
        LetterGridGameAudioEvents.RaiseMusicMuteInit(isMusicMuted);
        LetterGridGameAudioEvents.RaiseSFXMuteInit(isSFXMuted);
    }

    // --- Optionally, you can keep StopMusic private or remove if not used by events ---
    private void StopMusic() {
        if (bgMusicSource != null) bgMusicSource.Stop();
        currentTrackIndex = -1;
    }

    // --- If you want to support name-based play (private for now) ---
    private void PlayMusicByName(string name) {
        int idx = bgMusicClips.FindIndex(c => c != null && c.name == name);
        if (idx != -1) PlayMusicByIndex(idx);
    }

    private void FadeToMusicIndex(int idx) {
        // If nothing is playing, fade in only (not out)
        if (!bgMusicSource.isPlaying || bgMusicSource.clip == null) {
            StartCoroutine(FadeInMusicCoroutine(idx));
            return;
        }
        if (currentTrackIndex == idx && bgMusicSource.isPlaying) {
            UpdateVolumes();
            return;
        }
        // Otherwise, fade out/in between tracks
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(idx));
    }

    // Simple fade-in on start
    private IEnumerator FadeInMusicCoroutine(int idx) {
        isFadingMusic = true;
        currentTrackIndex = idx;
        bgMusicSource.clip = bgMusicClips[idx];
        bgMusicSource.loop = true;
        bgMusicSource.volume = 0f;
        bgMusicSource.Play();

        float t = 0f;
        float targetVol = isAllMuted || isMusicMuted ? 0f : musicVolume;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            bgMusicSource.volume = Mathf.Lerp(0f, targetVol, t / fadeDuration);
            yield return null;
        }
        bgMusicSource.volume = targetVol;
        isFadingMusic = false;
    }




    private IEnumerator FadeMusicCoroutine(int newIdx) {
        isFadingMusic = true;
        if (bgMusicClips == null || bgMusicClips.Count == 0 || newIdx < 0 || newIdx >= bgMusicClips.Count)
            yield break;

        if (bgMusicSource == null)
            yield break;

        // Fade out current music
        float startVol = bgMusicSource.volume;
        float targetVol = 0f;
        float t = 0f;

        while (t < fadeDuration && bgMusicSource.isPlaying) {
            t += Time.deltaTime;
            bgMusicSource.volume = Mathf.Lerp(startVol, targetVol, t / fadeDuration);
            yield return null;
        }
        bgMusicSource.volume = 0f;
        bgMusicSource.Stop();

        // Switch track and fade in
        currentTrackIndex = newIdx;
        bgMusicSource.clip = bgMusicClips[newIdx];
        bgMusicSource.loop = true;
        bgMusicSource.Play();

        t = 0f;
        startVol = 0f;
        targetVol = isAllMuted || isMusicMuted ? 0f : musicVolume;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            bgMusicSource.volume = Mathf.Lerp(startVol, targetVol, t / fadeDuration);
            yield return null;
        }
        bgMusicSource.volume = targetVol;
        isFadingMusic = false;
    }

    private IEnumerator DuckMusicWhileSFX(AudioClip sfxClip) {
        float prevMusicVol = bgMusicSource ? bgMusicSource.volume : 1f;
        float sfxLength = sfxClip.length;

        // Instantly mute music (or you can fade out if you want)
        if (bgMusicSource) bgMusicSource.volume = 0f;

        // Play SFX
        sfxSource.PlayOneShot(sfxClip);

        // Wait for SFX to finish
        yield return new WaitForSeconds(sfxLength);

        // Restore music volume only if not muted globally
        UpdateVolumes();
    }

}

