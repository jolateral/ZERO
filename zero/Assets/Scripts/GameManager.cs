using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Center Floor Number Display (stylized PNG sprites)")]
    [SerializeField] private Image centerCountImage;
    [Tooltip("Sprites indexed by number to display: index 0 = '0', index 1 = '1', ... index 5 = '5'. Assign as many as you have; if the count exceeds the array, the last sprite is used.")]
    [SerializeField] private Sprite[] numberSprites;

    [Header("Puzzles, in solve order: A, B, C, D")]
    [SerializeField] private PuzzleBase[] puzzlesInOrder;

    [Header("Hallway thumbnails, SAME ORDER as Puzzles In Order")]
    [SerializeField] private PuzzleHallwayIcon[] hallwayIcons;

    [Header("Final Puzzle (Puzzle E) root object, enabled once count hits 1")]
    [SerializeField] private GameObject finalPuzzleRoot;
    [SerializeField] private PuzzleE finalPuzzle; // the PuzzleE component living under finalPuzzleRoot

    [Header("Room-view <-> close-up focus transitions")]
    [SerializeField] private PuzzleFocusController focusController;

    [Header("Ending: open-hallway view + epilogue/credits")]
    [Tooltip("Enabled once Puzzle E is solved -- e.g. a hallway background variant with the door open, or a swapped end-cap decoration. Sits alongside/within hallwayView.")]
    [SerializeField] private GameObject openHallwayBackground;
    [Tooltip("The 4 puzzle thumbnail GameObjects (or their shared parents), same order as hallwayIcons isn't required here -- just drag each one in. Hidden once the door opens.")]
    [SerializeField] private GameObject[] hallwayThumbnails;
    [Tooltip("Parent object of the center floor puzzle-count display. Hidden once the door opens.")]
    [SerializeField] private GameObject puzzleCounterContainer;
    [SerializeField] private EpilogueSequenceController epilogueController;
    [Tooltip("How long the player gets to sit in the open-hallway view before the epilogue text starts fading in.")]
    [SerializeField] private float epilogueStartDelay = 2f;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    private int remainingCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        remainingCount = puzzlesInOrder.Length + 1; // 4 wall puzzles + final = 5, matches design doc
        UpdateCenterDisplay();

        // Only the first puzzle starts active (ticking/reachable); the rest start Off until unlocked.
        // All four are always VISIBLE in the hallway, but only unlocked ones are clickable.
        for (int i = 0; i < puzzlesInOrder.Length; i++)
        {
            puzzlesInOrder[i].OnSolved += HandlePuzzleSolved;

            if (hallwayIcons != null && i < hallwayIcons.Length)
            {
                hallwayIcons[i].SetUnlocked(i == 0);
            }

            if (i == 0)
            {
                puzzlesInOrder[i].Activate();
            }
            else
            {
                puzzlesInOrder[i].SetLight(LightState.Off);
            }
        }

        if (finalPuzzleRoot != null)
        {
            finalPuzzleRoot.SetActive(false);
        }

        // Game starts on the hallway view (all four thumbnails visible, only A clickable).
        // PuzzleFocusController.Awake() already defaults to showing the hallway/room view.
    }

    /// <summary>Called by a PuzzleHallwayIcon when the player clicks an unlocked puzzle thumbnail.</summary>
    public void SelectPuzzle(int index)
    {
        if (index < 0 || index >= puzzlesInOrder.Length) return;
        if (!IsPuzzleUnlocked(index)) return;

        // Puzzle E has no focus view of its own -- it plays in the hallway. Stop listening
        // for ZERO keystrokes the moment the player leaves the hallway for any other puzzle
        // (including re-visiting an already-solved A-D puzzle).
        if (finalPuzzle != null)
        {
            finalPuzzle.SetListening(false);
        }

        if (focusController != null)
        {
            focusController.FocusOnPuzzle(index);
        }
    }

    /// <summary>Called by a puzzle's "back" button. Resets that puzzle if unsolved, then returns to the hallway.</summary>
    public void BackToHallwayFromPuzzle(int index)
    {
        if (index >= 0 && index < puzzlesInOrder.Length)
        {
            puzzlesInOrder[index].ResetPuzzle();
        }

        if (focusController != null)
        {
            focusController.ReturnToHallway();
        }

        RefreshFinalPuzzleListening();
    }

    /// <summary>Puzzle E should only be listening for keystrokes when D is solved, E itself
    /// isn't solved yet, and the player is actually looking at the hallway (not some other
    /// puzzle's focus view).</summary>
    private void RefreshFinalPuzzleListening()
    {
        if (finalPuzzle == null) return;

        bool eUnlocked = finalPuzzleRoot != null && finalPuzzleRoot.activeSelf;
        finalPuzzle.SetListening(eUnlocked && !finalPuzzle.IsSolved);
    }

    private bool IsPuzzleUnlocked(int index)
    {
        if (index == 0) return true;
        return puzzlesInOrder[index - 1].IsSolved;
    }

    private void HandlePuzzleSolved(PuzzleBase solved)
    {
        remainingCount = Mathf.Max(0, remainingCount - 1);
        audioManager.PuzzleCompleted(remainingCount);
        UpdateCenterDisplay();

        int solvedIndex = System.Array.IndexOf(puzzlesInOrder, solved);
        int nextIndex = solvedIndex + 1;

        if (nextIndex < puzzlesInOrder.Length)
        {
            puzzlesInOrder[nextIndex].Activate();

            if (hallwayIcons != null && nextIndex < hallwayIcons.Length)
            {
                hallwayIcons[nextIndex].SetUnlocked(true);
            }
        }
        else if (remainingCount <= 1 && finalPuzzleRoot != null)
        {
            // All four wall puzzles solved, center shows 1 -> Puzzle E becomes active.
            // It has no focus view of its own; it plays directly in the hallway.
            finalPuzzleRoot.SetActive(true);
            if (finalPuzzle != null)
            {
                finalPuzzle.Activate();
            }
        }

        // Point-and-click flow: always drop back to the hallway after a solve so the
        // player chooses when to walk into the next puzzle, rather than auto-zooming in.
        if (focusController != null)
        {
            focusController.ReturnToHallway();
        }

        RefreshFinalPuzzleListening();
    }

    /// <summary>
    /// Swaps the center display's sprite to match remainingCount.
    /// If numberSprites doesn't have an entry for the exact count (e.g. missing a "0" sprite),
    /// it clamps to the last available sprite in the array instead of throwing/erroring.
    /// </summary>
    private void UpdateCenterDisplay()
    {
        if (centerCountImage == null || numberSprites == null || numberSprites.Length == 0)
        {
            return;
        }

        int spriteIndex = Mathf.Clamp(remainingCount, 0, numberSprites.Length - 1);
        Sprite sprite = numberSprites[spriteIndex];

        if (sprite != null)
        {
            centerCountImage.sprite = sprite;
            // NOTE: intentionally NOT calling SetNativeSize() here — that would override
            // the RectTransform size/position you set up manually in the editor.
            // The Image component will scale the sprite to fit whatever RectTransform
            // size you've configured, as long as its Image Type is set appropriately
            // (see notes below).
        }
    }

    /// <summary>Called by PuzzleE once the player has spelled ZERO and the wall has opened.</summary>
    public void CompleteFinalPuzzle()
    {
        remainingCount = 0;
        UpdateCenterDisplay(); // the giant center number flips from "1" to "0"

        if (openHallwayBackground != null)
        {
            openHallwayBackground.SetActive(true);
        }

        if (finalPuzzleRoot != null)
        {
            finalPuzzleRoot.SetActive(false);
        }

        // The puzzle is fully over -- clear the hallway of everything that referred to it.
        if (hallwayThumbnails != null)
        {
            foreach (var thumbnail in hallwayThumbnails)
            {
                if (thumbnail != null) thumbnail.SetActive(false);
            }
        }

        if (puzzleCounterContainer != null)
        {
            puzzleCounterContainer.SetActive(false);
        }

        // Drop back to the (now-open) hallway view rather than lingering on the puzzle room.
        if (focusController != null)
        {
            focusController.ReturnToHallway();
        }

        StartCoroutine(DelayedEpilogue());
    }

    /// <summary>Gives the player a beat to take in the open hallway before the epilogue text starts.</summary>
    private IEnumerator DelayedEpilogue()
    {
        yield return new WaitForSeconds(epilogueStartDelay);

        if (epilogueController != null)
        {
            epilogueController.PlayEpilogue();
        }
    }
}