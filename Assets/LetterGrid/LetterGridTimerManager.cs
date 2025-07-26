using TMPro;
using UnityEngine;

public class LetterGridTimerManager : MonoBehaviour {
    [Header("🕒 Timer Settings")]
    public TMP_Text timerText;
    public float roundDuration = 60f; // seconds
    public Color warningColor = Color.red;
    public float warningThreshold = 10f; // seconds

    private float timeLeft;
    private bool isTimerActive = false;
    private Color defaultColor;

    private void Start() {

    }

    private void Update() {
        if (!isTimerActive) return;

        timeLeft -= Time.deltaTime;
        UpdateTimerUI();

        if (timeLeft <= 0f) {
            EndRound();
        }
    }

    public void StartNewRound(float customDuration = -1f) {
        timeLeft = customDuration > 0f ? customDuration : roundDuration;
        isTimerActive = true;
        if (timerText != null)
            defaultColor = timerText.color;
        UpdateTimerUI();
    }

    public void StopTimer() {
        isTimerActive = false;
    }

    private void UpdateTimerUI() {
        int displayTime = Mathf.CeilToInt(timeLeft);
        timerText.text = $"Time: {displayTime}";

        if (timeLeft <= warningThreshold) {
            timerText.color = warningColor;
        }
        else {
            timerText.color = defaultColor;
        }
    }

    private void EndRound() {
        isTimerActive = false;
        timerText.text = "Time: 0";

        // ✅ Use centralized game end logic
        LetterGridGameManager.Instance.EndGame("Time's up! Try again.");
    }

}
