using System.Collections;
using UnityEngine;

/// <summary>
/// Manages crossfading between the wide "room view" (background room image +
/// small unreadable puzzle thumbnails) and a full-screen "focus view" close-up
/// of whichever puzzle is currently active. All puzzle UI you already built
/// (the big readable panels) become the focus views; the room view is a new,
/// separate wide shot.
///
/// Wire this up alongside GameManager: GameManager calls FocusOnPuzzle(index)
/// when it activates a puzzle, and ReturnToRoomView() briefly after a puzzle
/// is solved before focusing the next one.
/// </summary>
public class PuzzleFocusController : MonoBehaviour
{
    [Header("Room (wide shot) view")]
    [SerializeField] private CanvasGroup roomView;

    [Header("Focus (close-up) views, same order as GameManager's Puzzles In Order")]
    [SerializeField] private CanvasGroup[] focusViews;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdOnRoomView = 1.0f; // pause on room view between puzzles

    private int currentFocusIndex = -1;
    private Coroutine activeTransition;

    private void Awake()
    {
        // Start on room view; all focus views hidden/non-interactive.
        SetGroupVisible(roomView, true);
        foreach (var fv in focusViews)
        {
            SetGroupVisible(fv, false);
        }
    }

    /// <summary>Crossfade from whatever's showing to the given puzzle's close-up.</summary>
    public void FocusOnPuzzle(int index)
    {
        if (index < 0 || index >= focusViews.Length) return;
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(CrossfadeToFocus(index));
    }

    /// <summary>Crossfade from the current focus view back to the room view.</summary>
    public void ReturnToRoomView()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(CrossfadeToRoom());
    }

    /// <summary>Convenience: fade out current focus, show room briefly, fade into next puzzle.</summary>
    public void TransitionToNextPuzzle(int nextIndex)
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(RoomThenFocusSequence(nextIndex));
    }

    private IEnumerator CrossfadeToFocus(int index)
    {
        yield return FadeOut(roomView);
        yield return FadeIn(focusViews[index]);
        currentFocusIndex = index;
    }

    private IEnumerator CrossfadeToRoom()
    {
        if (currentFocusIndex >= 0)
        {
            yield return FadeOut(focusViews[currentFocusIndex]);
        }
        yield return FadeIn(roomView);
        currentFocusIndex = -1;
    }

    private IEnumerator RoomThenFocusSequence(int nextIndex)
    {
        if (currentFocusIndex >= 0)
        {
            yield return FadeOut(focusViews[currentFocusIndex]);
        }
        yield return FadeIn(roomView);
        currentFocusIndex = -1;

        yield return new WaitForSeconds(holdOnRoomView);

        yield return FadeOut(roomView);
        yield return FadeIn(focusViews[nextIndex]);
        currentFocusIndex = nextIndex;
    }

    private IEnumerator FadeOut(CanvasGroup group)
    {
        yield return Fade(group, group.alpha, 0f);
        SetGroupVisible(group, false);
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        SetGroupVisible(group, true);
        yield return Fade(group, group.alpha, 1f);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }

    private void SetGroupVisible(CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.blocksRaycasts = visible;
        group.interactable = visible;
        // Keep GameObject active (not SetActive false) so coroutines/timers on
        // puzzle scripts underneath can still run if you want them ticking
        // even while not focused. If you'd rather fully pause off-screen
        // puzzles, call group.gameObject.SetActive(visible) instead.
    }
}
