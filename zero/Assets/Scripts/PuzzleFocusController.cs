using System.Collections;
using UnityEngine;

/// <summary>
/// Manages crossfading between the wide "hallway view" (all four puzzle
/// thumbnails, locked ones greyed out) and a full-screen "focus view"
/// close-up of whichever puzzle the player clicked into. All puzzle UI you
/// already built (the big readable panels) are the focus views; the hallway
/// view is the separate wide shot with the clickable thumbnails.
///
/// Wire this up alongside GameManager:
/// - GameManager.SelectPuzzle(index) -> FocusOnPuzzle(index) when the player
///   clicks an unlocked hallway thumbnail.
/// - GameManager.HandlePuzzleSolved(...) -> ReturnToHallway() once a puzzle
///   is solved.
/// - GameManager.BackToHallwayFromPuzzle(index) -> ReturnToHallway() when the
///   player clicks a puzzle's own "back" button.
/// </summary>
public class PuzzleFocusController : MonoBehaviour
{
    [Header("Hallway (wide shot, all thumbnails) view")]
    [SerializeField] private CanvasGroup hallwayView;

    [Header("Focus (close-up) views, same order as GameManager's Puzzles In Order")]
    [SerializeField] private CanvasGroup[] focusViews;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    private int currentFocusIndex = -1;
    private Coroutine activeTransition;

    private void Awake()
    {
        // Start on the hallway view; all focus views hidden/non-interactive.
        SetGroupVisible(hallwayView, true);
        foreach (var fv in focusViews)
        {
            SetGroupVisible(fv, false);
        }
    }

    /// <summary>Crossfade from the hallway to the given puzzle's close-up.</summary>
    public void FocusOnPuzzle(int index)
    {
        if (index < 0 || index >= focusViews.Length) return;
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(CrossfadeToFocus(index));

        audioManager.PuzzleView();
    }

    /// <summary>Crossfade from the current focus view back to the hallway view.</summary>
    public void ReturnToHallway()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(CrossfadeToHallway());

        audioManager.HallwayView();
    }

    private IEnumerator CrossfadeToFocus(int index)
    {
        yield return FadeOut(hallwayView);
        yield return FadeIn(focusViews[index]);
        currentFocusIndex = index;
    }

    private IEnumerator CrossfadeToHallway()
    {
        if (currentFocusIndex >= 0)
        {
            yield return FadeOut(focusViews[currentFocusIndex]);
        }
        yield return FadeIn(hallwayView);
        currentFocusIndex = -1;
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
        // puzzle scripts underneath can still run while not focused (e.g. Puzzle
        // C's countdown keeps ticking even if you're looking at the hallway).
    }
}
