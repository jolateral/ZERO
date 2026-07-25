using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One clickable thumbnail in the hallway view representing a single puzzle.
/// Shows a lock overlay when the puzzle isn't unlocked yet (previous puzzle
/// not solved), and tells GameManager to focus that puzzle when clicked.
///
/// Scene setup: put this on each puzzle's small thumbnail button in the
/// hallway. Assign the Button component and an optional "lock" GameObject
/// (a padlock icon / greyscale panel) that's shown while locked. Set
/// Puzzle Index to 0/1/2/3 matching GameManager's Puzzles In Order array.
/// Hook the Button's OnClick() to this script's OnClicked().
/// </summary>
public class PuzzleHallwayIcon : MonoBehaviour
{
    [SerializeField] private int puzzleIndex;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockOverlay; // shown when locked, hidden when unlocked

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    public void SetUnlocked(bool unlocked)
    {
        if (button != null) button.interactable = unlocked;
        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
    }

    // Hook to this icon's Button OnClick().
    public void OnClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectPuzzle(puzzleIndex);
        }
    }
}
