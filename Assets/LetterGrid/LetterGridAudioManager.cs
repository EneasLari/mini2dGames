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
    [SerializeField] private AudioClip victoryClip;      // Assign in Inspector
    [SerializeField] private AudioClip correctMoveClip;  // Assign in Inspector
    [SerializeField] private AudioClip wrongMoveClip;    // Assign in Inspector

    [Header("🔊 Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool isMusicMuted = false;
    [SerializeField] private bool isSFXMuted = false;
    [SerializeField] private bool isAllMuted = false;

    private int currentTrackIndex = -1;

    // --- Unity Events ---
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Music persists between scenes
        }
        else {
            Destroy(gameObject);
        }
    }

    void OnEnable() {
        LetterGridGameAudioEvents.OnClick += PlayClick;
        LetterGridGameAudioEvents.OnTileAdded += PlayTileAdd;
        LetterGridGameAudioEvents.OnStartGame += PlayGameMusic; // NEW
        LetterGridGameAudioEvents.OnBackToMenu += PlayMenuMusic; // NEW
        LetterGridGameAudioEvents.OnVictory += PlayVictorySFX;      // NEW
        LetterGridGameAudioEvents.OnMoveCorrect += PlayCorrectMoveSFX;  // NEW
        LetterGridGameAudioEvents.OnMoveWrong += PlayWrongMoveSFX;    // NEW
        LetterGridGameAudioEvents.OnMusicVolumeChanged += SetMusicVolume;
        LetterGridGameAudioEvents.OnSFXVolumeChanged += SetSFXVolume;
        LetterGridGameAudioEvents.OnMusicMuteChanged += SetMusicMute;
        LetterGridGameAudioEvents.OnSFXMuteChanged += SetSFXMute;
        LetterGridGameAudioEvents.OnAllMuteChanged += SetAllMute;
    }

    void OnDisable() {
        LetterGridGameAudioEvents.OnClick -= PlayClick;
        LetterGridGameAudioEvents.OnTileAdded -= PlayTileAdd;
        LetterGridGameAudioEvents.OnStartGame -= PlayGameMusic; // NEW
        LetterGridGameAudioEvents.OnBackToMenu -= PlayMenuMusic; // NEW
        LetterGridGameAudioEvents.OnVictory -= PlayVictorySFX;
        LetterGridGameAudioEvents.OnMoveCorrect -= PlayCorrectMoveSFX;
        LetterGridGameAudioEvents.OnMoveWrong -= PlayWrongMoveSFX;
        LetterGridGameAudioEvents.OnMusicVolumeChanged -= SetMusicVolume;
        LetterGridGameAudioEvents.OnSFXVolumeChanged -= SetSFXVolume;
        LetterGridGameAudioEvents.OnMusicMuteChanged -= SetMusicMute;
        LetterGridGameAudioEvents.OnSFXMuteChanged -= SetSFXMute;
        LetterGridGameAudioEvents.OnAllMuteChanged -= SetAllMute;
    }

    private void Start() {
        PlayMenuMusic();
        UpdateVolumes();
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

    private void PlayMenuMusic() => PlayMusicByIndex(0); // [0] = menu
    private void PlayGameMusic() => PlayMusicByIndex(1); // [1] = game

    private void PlayClick() {
        if (sfxSource && clickClip) sfxSource.PlayOneShot(clickClip);
    }
    private void PlayTileAdd() {
        if (sfxSource && tileAddClip) sfxSource.PlayOneShot(tileAddClip);
    }
    private void PlayVictorySFX() {
        if (sfxSource && victoryClip) sfxSource.PlayOneShot(victoryClip);
    }
    private void PlayCorrectMoveSFX() {
        if (sfxSource && correctMoveClip) sfxSource.PlayOneShot(correctMoveClip);
    }
    private void PlayWrongMoveSFX() {
        if (sfxSource && wrongMoveClip) sfxSource.PlayOneShot(wrongMoveClip);
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
        float finalMusicVol = isAllMuted || isMusicMuted ? 0f : musicVolume;
        float finalSFXVol = isAllMuted || isSFXMuted ? 0f : sfxVolume;
        if (bgMusicSource) bgMusicSource.volume = finalMusicVol;
        if (sfxSource) sfxSource.volume = finalSFXVol;
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
}

