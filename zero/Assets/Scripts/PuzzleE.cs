using System.Collections;
using UnityEngine;

/// <summary>
/// Puzzle E, the final puzzle. Unlike A-D, this one has no separate focus/
/// room view -- it plays directly in the hallway, which is already covered
/// in ambient "zero"/"0" wall decoration (pure flavor text up to this point).
/// It only starts accepting input once Puzzle D is solved. To solve it, the
/// player types Z-E-R-O on the keyboard while standing in the hallway. Each
/// correct letter cumulatively lights up that letter's glow overlay on the
/// wall (Z, then Z+E, then Z+E+R, then Z+E+R+O). Any wrong letter resets
/// all glow and progress back to zero -- the player has to start spelling
/// from Z again.
///
/// Once all 4 letters are correct: a brief "full wall glow" flourish plays,
/// then the wall art swaps from its closed state (with the keyhole/silhouette
/// art) to its open state, the puzzle is marked solved, and GameManager is
/// notified so it can drop the giant center number to 0 and kick off the
/// open-hallway + epilogue sequence.
/// </summary>
public class PuzzleE : PuzzleBase
{
    private static readonly KeyCode[] TargetSequence = { KeyCode.Z, KeyCode.E, KeyCode.R, KeyCode.O };

    [Header("Cumulative letter glow overlays, in order Z, E, R, O")]
    [Tooltip("Size 4. Each GameObject holds one letter's glow overlay graphic (e.g. glowZPattern, glowEPattern, glowRPattern, glowO-0Pattern). Enabled one at a time, cumulatively, as the player spells correctly.")]
    [SerializeField] private GameObject[] letterGlowObjects;

    [Header("Full-wall glow flourish, shown briefly once all 4 letters are correct")]
    [Tooltip("The glowPatternFull overlay -- the big 'everything lights up' moment right before the wall opens.")]
    [SerializeField] private GameObject fullGlowObject;
    [SerializeField] private float fullGlowHoldSeconds = 1.0f;

    [Header("Wall art swap: closed (keyhole/silhouette art) -> open (reveals hallway)")]
    [Tooltip("The hallway wall's Image component itself -- this puzzle plays directly in the hallway view, not a separate focus/room view.")]
    [SerializeField] private UnityEngine.UI.Image wallImage;
    [SerializeField] private Sprite wallClosedSprite;
    [SerializeField] private Sprite wallOpenSprite;

    [Header("Audio (optional -- leave unassigned if you haven't added the matching AudioManager methods yet)")]
    [SerializeField] private AudioManager audioManager;

    [Tooltip("Only listens for keystrokes while this puzzle's room view is the one actually on screen. Call SetListening(true/false) when entering/leaving this room.")]
    [SerializeField] private bool isListening = false;

    private int progress = 0;
    private bool solving = false; // true once the win coroutine has started; blocks further input mid-sequence

    protected override void Awake()
    {
        base.Awake();

        if (wallImage != null && wallClosedSprite != null)
        {
            wallImage.sprite = wallClosedSprite;
        }

        ClearGlow();
    }

    /// <summary>Called by GameManager once all 4 wall puzzles are solved and this room becomes reachable.</summary>
    public override void Activate()
    {
        base.Activate();
        if (IsSolved) return;

        progress = 0;
        solving = false;
        ClearGlow();
    }

    /// <summary>Called by GameManager if the player backs out of this room without finishing it.</summary>
    public override void ResetPuzzle()
    {
        if (IsSolved) return;

        progress = 0;
        solving = false;
        ClearGlow();
    }

    /// <summary>
    /// Wire this to whatever shows/hides this room view (e.g. your focus/room
    /// controller) so keystrokes only count while the player is actually
    /// looking at this room, not while they're elsewhere in the hallway.
    /// </summary>
    public void SetListening(bool listening)
    {
        isListening = listening;
    }

    private void Update()
    {
        if (!isListening || IsSolved || solving) return;

        for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
        {
            if (!Input.GetKeyDown(key)) continue;

            if (key == TargetSequence[progress])
            {
                if (letterGlowObjects != null && progress < letterGlowObjects.Length && letterGlowObjects[progress] != null)
                {
                    letterGlowObjects[progress].SetActive(true);
                }

                progress++;

                if (progress >= TargetSequence.Length)
                {
                    StartCoroutine(WinSequence());
                }
            }
            else
            {
                HandleMistake();
            }

            return; // only ever process one keypress per frame
        }
    }

    private void HandleMistake()
    {
        progress = 0;
        ClearGlow();

        if (audioManager != null)
        {
            // Hook a wrong-letter buzz/reset sound here once you add one, e.g.:
            // audioManager.PuzzleEMistake();
        }
    }

    private void ClearGlow()
    {
        if (letterGlowObjects != null)
        {
            foreach (var glow in letterGlowObjects)
            {
                if (glow != null) glow.SetActive(false);
            }
        }

        if (fullGlowObject != null)
        {
            fullGlowObject.SetActive(false);
        }
    }

    private IEnumerator WinSequence()
    {
        solving = true;
        isListening = false;

        if (fullGlowObject != null)
        {
            fullGlowObject.SetActive(true);
        }

        if (audioManager != null)
        {
            // Hook a "solved" stinger here once you add one, e.g.:
            // audioManager.PuzzleESolved();
        }

        yield return new WaitForSeconds(fullGlowHoldSeconds);

        // Wall swap and door-open happen in the same beat -- no gap where the open wall
        // art is visible with the glow still up (or vice versa). GameManager.CompleteFinalPuzzle()
        // is what actually deactivates finalPuzzleRoot (and with it, the glow overlays), so we
        // swap the wall sprite right here immediately before calling it.
        if (wallImage != null && wallOpenSprite != null)
        {
            wallImage.sprite = wallOpenSprite;
        }

        MarkSolved();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteFinalPuzzle();
        }
    }
}