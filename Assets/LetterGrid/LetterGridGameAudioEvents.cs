using System;
using UnityEngine;

/// <summary>
/// Event channel for all audio/game SFX triggers and settings.
/// Only use these static events to communicate audio intent.
/// </summary>
public class LetterGridGameAudioEvents : MonoBehaviour {
    // ---- Gameplay SFX events ----
    public static event Action OnClick;
    public static event Action OnTileAdded;
    public static event Action OnStartGame;
    public static event Action OnVictory;
    public static event Action OnMoveCorrect;
    public static event Action OnMoveWrong;
    public static event Action OnBackToMenu;

    // ---- Audio settings events ----
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;
    public static event Action<bool> OnMusicMuteChanged;
    public static event Action<bool> OnSFXMuteChanged;
    public static event Action<bool> OnAllMuteChanged;

    // ---- Event Raisers (call from UI/gameplay, never from AudioManager) ----
    public static void RaiseClick() => OnClick?.Invoke();
    public static void RaiseTileAdded() => OnTileAdded?.Invoke();
    public static void RaiseStartGame() => OnStartGame?.Invoke();
    public static void RaiseVictory() => OnVictory?.Invoke();
    public static void RaiseMoveCorrect() => OnMoveCorrect?.Invoke();
    public static void RaiseMoveWrong() => OnMoveWrong?.Invoke();
    public static void RaiseBackToMenu() => OnBackToMenu?.Invoke();

    public static void RaiseMusicVolumeChanged(float v) => OnMusicVolumeChanged?.Invoke(v);
    public static void RaiseSFXVolumeChanged(float v) => OnSFXVolumeChanged?.Invoke(v);
    public static void RaiseMusicMuteChanged(bool m) => OnMusicMuteChanged?.Invoke(m);
    public static void RaiseSFXMuteChanged(bool m) => OnSFXMuteChanged?.Invoke(m);
    public static void RaiseAllMuteChanged(bool m) => OnAllMuteChanged?.Invoke(m);
}
