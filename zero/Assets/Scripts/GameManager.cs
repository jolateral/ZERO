using UnityEngine;
using TMPro;

/// <summary>
/// Owns the center-floor countdown number (starts at 5) and the unlock chain
/// A -> B -> C -> D -> (E, final puzzle). Wire each PuzzleX component in the
/// Inspector in solve order.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Center Floor Number Display (placeholder TMP text)")]
    [SerializeField] private TMP_Text centerCountText;

    [Header("Puzzles, in solve order: A, B, C, D")]
    [SerializeField] private PuzzleBase[] puzzlesInOrder;

    [Header("Final Puzzle (Puzzle E) root object, enabled once count hits 1")]
    [SerializeField] private GameObject finalPuzzleRoot;

    [Header("Optional: room-view <-> close-up focus transitions")]
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

        // Only the first puzzle starts active; the rest start Off until unlocked.
        for (int i = 0; i < puzzlesInOrder.Length; i++)
        {
            int index = i; // capture
            puzzlesInOrder[i].OnSolved += HandlePuzzleSolved;
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

        // NOTE: we deliberately do NOT auto-focus Puzzle A here. The room view
        // should be what's visible after the title fades out. TitleScreenController
        // calls BeginPuzzleAFocus() below once its fade sequence finishes.
    }

    /// <summary>Call this from TitleScreenController once the title fade sequence completes.</summary>
    public void BeginPuzzleAFocus()
    {
        if (focusController != null)
        {
            focusController.FocusOnPuzzle(0);
        }
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

            // Fade out solved puzzle's close-up, briefly show room, fade into next puzzle's close-up.
            if (focusController != null)
            {
                focusController.TransitionToNextPuzzle(nextIndex);
            }
        }
        else if (remainingCount <= 1 && finalPuzzleRoot != null)
        {
            // All four wall puzzles solved, center shows 1 -> reveal final puzzle
            finalPuzzleRoot.SetActive(true);

            if (focusController != null)
            {
                focusController.ReturnToRoomView();
            }
        }
    }

    private void UpdateCenterDisplay()
    {
        if (centerCountText != null)
        {
            centerCountText.text = remainingCount.ToString();
        }
    }

    /// <summary>Call this from your Puzzle E script when the final puzzle is solved.</summary>
    public void CompleteFinalPuzzle()
    {
        remainingCount = 0;
        UpdateCenterDisplay();
        Debug.Log("Game complete! Hook your win screen / scene transition here.");
    }
}