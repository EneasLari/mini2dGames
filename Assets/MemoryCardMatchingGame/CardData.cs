using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "MemoryGame/CardData", order = 1)]
public class CardData : ScriptableObject {
    public Sprite[] frontImages;  // Array to store all the front images of cards
    public Sprite backImage;      // The back image (shared across all cards)
}
