using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LightButton : MonoBehaviour {
    public int x, y; // Position in grid
    private LightsOutGame gameManager;
    private bool isOn = true; // Light state
    private Button button;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleLights);
        gameManager = FindFirstObjectByType<LightsOutGame>();
        UpdateColor();
    }

    void ToggleLights() {
        gameManager.ToggleLight(x, y);
    }

    public void SetState(bool state) {
        isOn = state;
        UpdateColor();
    }

    private void UpdateColor() {
        GetComponent<Image>().color = isOn ? Color.yellow : Color.grey;
        GetComponentInChildren<TMP_Text>().text = isOn ? "ON" : "OFF";
    }

    public bool IsOn() {
        return isOn;
    }
}
