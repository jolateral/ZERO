using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Center Floor Number Display (placeholder TMP text)")]
    [SerializeField] private TMP_Text centerCountText;

    [Header("Puzzles, in solve order: A, B, C, D")]
    [SerializeField] private PuzzleBase[] puzzlesInOrder;

    [Header("Hallway thumbnails, SAME ORDER as Puzzles In Order")]
    [SerializeField] private PuzzleHallwayIcon[] hallwayIcons;

    [Header("Final Puzzle (Puzzle E) root object, enabled once count hits 1")]
    [SerializeField] private GameObject finalPuzzleRoot;
    [SerializeField] private PuzzleE finalPuzzle; // the PuzzleE component living under finalPuzzleRoot

    [Header("Room-view <-> close-up focus transitions")]
    [SerializeField] private PuzzleFocusController focusController;

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
    }

    private bool IsPuzzleUnlocked(int index)
    {
        if (index == 0) return true;
        return puzzlesInOrder[index - 1].IsSolved;
    }

    private void HandlePuzzleSolved(PuzzleBase solved)
    {
        remainingCount = Mathf.Max(0, remainingCount - 1);
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
            // All four wall puzzles solved, center shows 1 -> reveal final puzzle
            finalPuzzleRoot.SetActive(true);
            if (finalPuzzle != null)
            {
                finalPuzzle.Activate(); // flips the 3 target decorations to "1" and starts the glow
            }
        }

        // Point-and-click flow: always drop back to the hallway after a solve so the
        // player chooses when to walk into the next puzzle, rather than auto-zooming in.
        if (focusController != null)
        {
            focusController.ReturnToHallway();
        }
    }

    private void UpdateCenterDisplay()
    {
        if (centerCountText != null)
        {
            centerCountText.text = remainingCount.ToString();
        }
    }

    /// <summary>Called by PuzzleE once all 3 target decorations are fixed back to zero.</summary>
    public void CompleteFinalPuzzle()
    {
        remainingCount = 0;
        UpdateCenterDisplay(); // the giant center number flips from "1" to "0"
        Debug.Log("Game complete! Hook your win screen / scene transition here.");
    }
}
