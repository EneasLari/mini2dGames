using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PictureEntry {
    public string name;
    public Sprite image;
    public AudioClip audio; // Optional audio clip
}

[CreateAssetMenu(fileName = "PictureDatabase", menuName = "Game/PictureDatabase")]
public class PictureData : ScriptableObject {
    public List<PictureEntry> pictures;
}
