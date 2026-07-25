using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the title-screen -> puzzle-room reveal sequence, all within one scene.
/// Attach to an empty GameObject. Wire the Start button's OnClick() to
/// BeginTransition().
///
/// Sequence: fade out title overlay -> fade to black -> hide title overlay ->
/// fade black back out, revealing the puzzle room underneath (which should
/// already exist in the scene, just visually covered by the title until now).
///
/// Note: this no longer calls into GameManager at the end. PuzzleFocusController
/// already defaults to showing the hallway view on Awake(), and GameManager.Start()
/// already activates Puzzle A -- so once the black overlay clears, the player just
/// sees the hallway with Puzzle A's thumbnail unlocked and ready to click.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup titleOverlay;   // title text + start button
    [SerializeField] private CanvasGroup blackFadeOverlay; // full-screen black image, alpha 0 at start
    [SerializeField] private GameObject puzzleRoomRoot;   // optional: enable/activate puzzles only after reveal

    [Header("Timing")]
    [SerializeField] private float titleFadeOutDuration = 0.6f;
    [SerializeField] private float blackFadeInDuration = 0.6f;
    [SerializeField] private float holdOnBlack = 0.3f;
    [SerializeField] private float blackFadeOutDuration = 0.8f;

    private bool transitioning = false;

    private void Awake()
    {
        // Make sure the black overlay starts fully transparent and non-blocking.
        if (blackFadeOverlay != null)
        {
            blackFadeOverlay.alpha = 0f;
            blackFadeOverlay.blocksRaycasts = false;
            blackFadeOverlay.interactable = false;
        }
    }

    // Hook this to your Start button's OnClick().
    public void BeginTransition()
    {
        if (transitioning) return;
        StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
    {
        transitioning = true;

        // 1. Fade out title text/button
        yield return Fade(titleOverlay, 1f, 0f, titleFadeOutDuration);
        titleOverlay.blocksRaycasts = false;
        titleOverlay.interactable = false;

        // 2. Fade to black
        blackFadeOverlay.blocksRaycasts = true;
        yield return Fade(blackFadeOverlay, 0f, 1f, blackFadeInDuration);

        // 3. Hide title overlay entirely now that screen is black
        titleOverlay.gameObject.SetActive(false);

        // Optional: only "start" the puzzle room logic once we're behind the black screen
        if (puzzleRoomRoot != null && !puzzleRoomRoot.activeSelf)
        {
            puzzleRoomRoot.SetActive(true);
        }

        yield return new WaitForSeconds(holdOnBlack);

        // 4. Fade black back out, revealing the puzzle room (hallway view underneath)
        yield return Fade(blackFadeOverlay, 1f, 0f, blackFadeOutDuration);
        blackFadeOverlay.blocksRaycasts = false;

        transitioning = false;
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }
}