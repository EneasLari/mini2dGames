using System;
using UnityEngine;

public class LetterGridGameAudioEvents : MonoBehaviour
{
    public static event Action OnClick;
    public static event Action OnTileAdded;
    // Add more events as needed

    public static void RaiseClick() => OnClick?.Invoke();
    public static void RaiseTileAdded() => OnTileAdded?.Invoke();
}
