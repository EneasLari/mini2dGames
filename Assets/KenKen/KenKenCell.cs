using UnityEngine;
using TMPro;

public class KenKenCell : MonoBehaviour {
    public int row;
    public int column;
    public TMP_InputField inputField;

    // Returns the entered value (defaulting to 0 if parsing fails)
    public int Value {
        get {
            int.TryParse(inputField.text, out int val);
            return val;
        }
    }
}
