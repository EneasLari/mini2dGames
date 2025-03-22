using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour {
    public Transform cardsParentTransform;  // The parent object for all cards
    public CardData cardData;          // Reference to the CardData ScriptableObject

    private GameObject[] allGameObjectCards;      // Reference to all card GameObjects
    private MemoryCard[] allCards;      // Reference to all card GameObjects   
    private MemoryCard firstFlippedCard = null;  // The first card the player flips
    private MemoryCard secondFlippedCard = null; // The second card the player flips

    public bool isFlipping = false;     // Prevent multiple flips at the same time

    private static MemoryGameManager _instance;

    public static MemoryGameManager Instance {
        get {
            if (_instance == null) {
                _instance = FindFirstObjectByType<MemoryGameManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    void Start() {
        allGameObjectCards = new GameObject[cardsParentTransform.childCount];
        for (int i = 0; i < cardsParentTransform.childCount; i++) {
            allGameObjectCards[i] = cardsParentTransform.GetChild(i).gameObject;
        }
        // Initialize the game with shuffled cards
        AssignAllCards();
        ShuffleCards();
    }

    void AssignAllCards() {
        // Find all MemoryCard components in the children of the GameObjects in allCards
        List<MemoryCard> memoryCards = new List<MemoryCard>();
        foreach (var cardObject in allGameObjectCards) {
            MemoryCard memoryCard = cardObject.GetComponentInChildren<MemoryCard>();
            if (memoryCard != null) {
                memoryCards.Add(memoryCard);
            }
        }
        allCards = memoryCards.ToArray();
    }

    public void OnCardFlipped(MemoryCard cardFlipped) {
        if (isFlipping) return;  // Prevent flipping cards while waiting for previous check

        if (firstFlippedCard == null) {
            // First card flipped
            firstFlippedCard = cardFlipped;
            return;
        }

        if (secondFlippedCard == null) {
            // Second card flipped
            secondFlippedCard = cardFlipped;
            isFlipping = true;  // Set the flag to prevent further flipping
            StartCoroutine(CheckForMatch());
        }
    }

    IEnumerator CheckForMatch() {
        // Wait for the flip animation to complete
        yield return new WaitForSeconds(firstFlippedCard.flipDuration);

        // Check if the cards match
        if (firstFlippedCard.GetCardIndex() == secondFlippedCard.GetCardIndex()) {
            // Match found, disable the cards
            Debug.Log("Match Found!");
            firstFlippedCard.DisableCard();
            secondFlippedCard.DisableCard();
            CheckForGameOver();  // Check if all cards are matched
        } else {
            // No match, flip back
            Debug.Log("No Match!");
            StartCoroutine(firstFlippedCard.FlipCard());
            StartCoroutine(secondFlippedCard.FlipCard());
        }

        // Reset the flipped cards and the flag
        firstFlippedCard = null;
        secondFlippedCard = null;
        isFlipping = false;
    }

    // Shuffle cards randomly and assign each a front image
    void ShuffleCards() {
        // Calculate the number of pairs needed
        int totalCards = allCards.Length;
        int totalImages = cardData.frontImages.Length;
        int pairsPerImage = totalCards / totalImages;

        // Create a list of image indices where each index appears pairsPerImage times
        List<int> imageIndices = new List<int>();
        for (int i = 0; i < totalImages; i++) {
            for (int j = 0; j < pairsPerImage; j++) {
                imageIndices.Add(i);
            }
        }

        // If there are remaining cards, distribute them evenly among the images
        int remainingCards = totalCards % totalImages;
        for (int i = 0; i < remainingCards; i++) {
            imageIndices.Add(i);
        }

        // Shuffle the list of image indices
        for (int i = 0; i < imageIndices.Count; i++) {
            int temp = imageIndices[i];
            int randomIndex = Random.Range(i, imageIndices.Count);
            imageIndices[i] = imageIndices[randomIndex];
            imageIndices[randomIndex] = temp;
        }

        // Assign the shuffled indices to the cards
        for (int i = 0; i < allCards.Length; i++) {
            int imageIndex = imageIndices[i];
            allCards[i].SetCardIndex(imageIndex);
            allCards[i].SetCardData(cardData);
            allCards[i].InitializeCard();
        }
    }

    void CheckForGameOver() {
        foreach (var card in allCards) {
            if (card.cardButton.interactable) {
                return;  // If any card is still interactable, the game is not over
            }
        }
        Debug.Log("Game Over! All cards matched.");
        // Add your game over logic here (e.g., display a message, restart the game, etc.)
    }
}
