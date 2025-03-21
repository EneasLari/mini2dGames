using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlipper : MonoBehaviour {
    public float flipDuration = 0.5f;
    public GameObject frontImageObject; // assign in inspector
    public GameObject backImageObject;  // assign in inspector
    private bool isFlipped = false;

    void Start() {
        // Set the initial rotation of the front image to be flipped
        frontImageObject.transform.rotation = Quaternion.Euler(0, 180, 0);
        // Show back image initially
        frontImageObject.SetActive(false);
        backImageObject.SetActive(true);
    }

    // Call this on button click
    public void OnCardClicked() {
        print("Clicked");
        StartCoroutine(FlipCard());
    }

    IEnumerator FlipCard() {
        float time = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 180, 0);

        while (time < 1f) {
            time += Time.deltaTime / flipDuration;
            transform.rotation = Quaternion.Lerp(startRot, endRot, time);
            // When halfway, toggle the images
            if (time >= 0.5f && !isFlipped) {
                isFlipped = !isFlipped;
                frontImageObject.SetActive(!frontImageObject.activeSelf);
                backImageObject.SetActive(!backImageObject.activeSelf);
            }
            yield return null;
        }

        // Toggle the isFlipped state after the flip is complete
        isFlipped = !isFlipped;
    }
}
