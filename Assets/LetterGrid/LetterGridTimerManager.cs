using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour {
    public Text timerText;
    public float gameTime = 60f; // 60 seconds

    private float timeLeft;
    private bool isGameActive = true;

    private void Start() {
        timeLeft = gameTime;
        InvokeRepeating("UpdateTimer", 1f, 1f);
    }

    void UpdateTimer() {
        if (!isGameActive) return;

        timeLeft -= 1;
        timerText.text = "Time: " + timeLeft.ToString("0");

        if (timeLeft <= 0) {
            EndGame();
        }
    }

    void EndGame() {
        isGameActive = false;
        CancelInvoke("UpdateTimer");
        Debug.Log("Game Over! Final Score: " + LetterGridWordManager.instance.scoreText.text);
    }
}
