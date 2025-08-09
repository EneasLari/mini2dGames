using System;
using UnityEngine;

/// <summary>
/// A utility component that detects when a RectTransform's size changes
/// and raises an event with the new (and optionally old) size.
/// 
/// Useful for UI elements that need to react to resizes caused by:
/// - Screen resolution changes
/// - Device orientation changes
/// - Layout adjustments by parent elements
/// - Dynamic content size changes
/// 
/// Attach this to any GameObject with a RectTransform.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RectResizeForwarder : MonoBehaviour {
    /// <summary>
    /// Fired when the RectTransform's size changes.
    /// Provides the old size and the new size.
    /// </summary>
    public event Action<Vector2, Vector2> Resized;

    private RectTransform rectTransform;
    private Vector2 lastSize;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        lastSize = rectTransform.rect.size;
    }

    private void OnEnable() {
        // Optional: Immediately notify subscribers of the current size
        Resized?.Invoke(lastSize, lastSize);
    }

    /// <summary>
    /// Unity's built-in callback whenever this RectTransform's dimensions change.
    /// This includes width/height changes due to parent layout changes or anchors.
    /// </summary>
    private void OnRectTransformDimensionsChange() {
        Vector2 newSize = rectTransform.rect.size;

        // If the size hasn't changed, do nothing.
        if (newSize == lastSize)
            return;

        // Store the old size, update the last size, and fire the event.
        Vector2 oldSize = lastSize;
        lastSize = newSize;

        Resized?.Invoke(oldSize, newSize);
    }
}
