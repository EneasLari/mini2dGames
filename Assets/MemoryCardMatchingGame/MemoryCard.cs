using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MemoryCard : MonoBehaviour {
    public float flipDuration = 0.5f;
    public GameObject frontImageObject;  // assign in inspector
    public GameObject backImageObject;   // assign in inspector

    private bool isFlipped = false;

    private CardData cardData;  // Reference to the CardData ScriptableObject for this card

    private int cardIndex;  // To store the index of the front image

    public Button cardButton;  // Reference to the Button component

    void Awake() {
        if (cardButton != null) {
            // Set up the click event
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void InitializeCard() {
        // Initialize front and back images
        frontImageObject.transform.rotation = Quaternion.Euler(0, 180, 0);  // Start with back showing
        frontImageObject.SetActive(false);
        backImageObject.SetActive(true);

        // Set the back image (shared across all cards)
        backImageObject.GetComponent<Image>().sprite = cardData.backImage;

        // Ensure that this card gets its unique front image
        frontImageObject.GetComponent<Image>().sprite = cardData.frontImages[cardIndex];
    }

    // Call this on button click
    public void OnCardClicked() {
        if (isFlipped || MemoryGameManager.Instance.isFlipping) return; // Prevent double clicking on the same card or flipping during another flip
        print("Card clicked: " + cardIndex);
        // Flip the card
        StartCoroutine(FlipCard());

        // Notify the game manager that this card was flipped
        MemoryGameManager.Instance.OnCardFlipped(this);
    }

    public IEnumerator FlipCard() {
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

    // Disable the card (called after a match)
    public void DisableCard() {
        //frontImageObject.SetActive(false);
        //backImageObject.SetActive(false);
        //this.gameObject.SetActive(false);  // Disable the whole card
        cardButton.interactable = false;  // Disable button interaction
    }

    // Get the front image index
    public int GetCardIndex() {
        return cardIndex;
    }

    // Set the front image index (used by MemoryGameManager)
    public void SetCardIndex(int index) {
        cardIndex = index;
    }

    public void SetCardData(CardData data) {
        cardData = data;
    }
}
