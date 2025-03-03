using UnityEngine;
using UnityEngine.UI;

public class LetterGridUIManager : MonoBehaviour {
    public Button restartButton;
    public GameObject gameOverPanel;

    private void Start() {
        restartButton.onClick.AddListener(RestartGame);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver() {
        gameOverPanel.SetActive(true);
    }

    void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
