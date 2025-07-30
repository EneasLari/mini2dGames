using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to any UnityEngine.UI.Button to automatically play a UI click SFX via the audio event system.
/// Relies on LetterGridGameAudioEvents.RaiseClick().
/// 
/// HOW TO USE:
/// - Add this component to your button prefab or to any UI Button GameObject.
/// - On click, it raises the UI click event—no manual wiring needed.
/// - Audio is handled by the event-driven audio manager. No direct references to audio code required.
/// 
/// TIP: Add this script to your base Button prefab to ensure all game buttons play consistent SFX.
/// </summary>
[RequireComponent(typeof(Button))]
public class LetterGridUIButtonClickSFX : MonoBehaviour
{
    void Awake() {
        // Subscribe once; safer than adding listeners repeatedly
        GetComponent<Button>().onClick.AddListener(RaiseClickSoundEvent);
    }
    /// <summary>
    /// Raises the global UI click event for audio manager to play the SFX.
    /// </summary>
    private void RaiseClickSoundEvent() {
        LetterGridGameAudioEvents.RaiseButtonClick();
    }

    void OnDestroy() {
        // Clean up to avoid memory leaks
        GetComponent<Button>().onClick.RemoveListener(RaiseClickSoundEvent);
    }
}
