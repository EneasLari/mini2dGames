using System;
using UnityEngine;

/// <summary>
/// Utility that detects when a <see cref="RectTransform"/> changes size
/// and notifies listeners via the <see cref="Resized"/> event.
/// 
/// Supports two usage patterns:
/// 
/// 1️ **Instance-Based:**  
///     - Add this script to a GameObject manually in the editor or via code.  
///     - Subscribe/unsubscribe to the <see cref="Resized"/> event yourself.
/// 
/// 2️ **Static Helper (`Watch`) — "Fire & Forget":**  
///     - Call <see cref="Watch(RectTransform, Action{Vector2, Vector2})"/> with any RectTransform.  
///     - This will add the watcher automatically (if missing), subscribe your callback,  
///       and auto-unsubscribe it when the watched GameObject is destroyed.
/// 
/// Typical use cases:
/// - Reacting to device orientation changes.
/// - Adjusting layouts when a parent panel resizes.
/// - Triggering animations or scaling logic when UI elements grow/shrink.
/// - Measuring delta changes for responsive UI.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RectTransformResizeWatcher : MonoBehaviour {
    /// <summary>
    /// Event fired whenever the attached RectTransform's size changes.
    /// Parameters: 
    /// <para>• oldSize — The previous width/height in local space units.</para>
    /// <para>• newSize — The current width/height in local space units.</para>
    /// </summary>
    public event Action<Vector2, Vector2> Resized;

    // Internal reference to the RectTransform we are observing.
    private RectTransform rectTransform;

    // Tracks the last known size so we can detect actual changes.
    private Vector2 lastSize;

    // Used only for static Watch() mode so we can auto-unsubscribe in OnDestroy.
    private Action<Vector2, Vector2> autoUnsubscribeAction;

    private void Awake() {
        // Cache the RectTransform (avoids repeated GetComponent calls)
        rectTransform = GetComponent<RectTransform>();
        lastSize = rectTransform.rect.size;
    }

    private void OnEnable() {
        // Optionally notify subscribers immediately with the current size
        // (oldSize = newSize at this point)
        Resized?.Invoke(lastSize, lastSize);
    }

    /// <summary>
    /// Unity built-in callback that runs whenever the RectTransform's dimensions change.
    /// This can happen for many reasons:
    /// - Device orientation changes
    /// - Screen resolution changes
    /// - Parent layout group resizing
    /// - Anchors, pivots, or sizeDelta being modified in code
    /// </summary>
    private void OnRectTransformDimensionsChange() {
        Vector2 newSize = rectTransform.rect.size;

        // Skip if size hasn't actually changed
        if (newSize == lastSize)
            return;

        Vector2 oldSize = lastSize;
        lastSize = newSize;

        // Notify listeners
        Resized?.Invoke(oldSize, newSize);
    }

    private void OnDestroy() {
        // In static Watch() mode, we automatically unsubscribe the provided callback
        if (autoUnsubscribeAction != null) {
            Resized -= autoUnsubscribeAction;
            autoUnsubscribeAction = null;
        }
    }

    /// <summary>
    /// Static helper to watch a RectTransform for size changes.
    /// Automatically:
    /// - Adds a RectTransformResizeWatcher if not present
    /// - Subscribes the given callback
    /// - Unsubscribes it automatically when the target is destroyed
    /// 
    /// <para>This is the "fire & forget" mode — no manual cleanup needed.</para>
    /// </summary>
    /// <param name="target">The RectTransform to observe.</param>
    /// <param name="onResize">Callback invoked when size changes (oldSize, newSize).</param>
    /// <returns>The RectTransformResizeWatcher component (for optional further use).</returns>
    public static RectTransformResizeWatcher Watch(RectTransform target, Action<Vector2, Vector2> onResize) {
        if (target == null) {
            Debug.LogError("RectTransformResizeWatcher.Watch called with null target");
            return null;
        }

        // Ensure we have a watcher component on the target
        var watcher = target.GetComponent<RectTransformResizeWatcher>() ??
                      target.gameObject.AddComponent<RectTransformResizeWatcher>();

        if (onResize != null) {
            watcher.Resized += onResize;
            watcher.autoUnsubscribeAction = onResize; // Track so we can remove automatically on destroy
        }

        return watcher;
    }
}
