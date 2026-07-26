using UnityEngine;

/// <summary>
/// Puzzle E, the final puzzle. The wall has been decorated with plain "zero"
/// text and "0" digits the whole time as ambient decoration. Once puzzles
/// A-D are solved and this puzzle activates, exactly the three decorations
/// assigned below flip to "1"/"one" and start glowing. The player clicks
/// each of the three to flip it back to zero. Once all three are fixed,
/// this puzzle is marked solved, which (via GameManager) turns the giant
/// center number from "1" to "0" and opens the wall back into the hallway.
/// </summary>
public class PuzzleE : PuzzleBase
{
    [Header("Exactly the 3 decorations that flip to '1' for this puzzle")]
    [SerializeField] private PuzzleEDecoration[] targetDecorations;

    [Header("Optional: wall-opening animation/object, enabled once solved")]
    [SerializeField] private GameObject wallOpenAnimationRoot;

    private int fixedCount;
    private bool started = false;

    protected override void Awake()
    {
        base.Awake();
        foreach (var deco in targetDecorations)
        {
            if (deco != null) deco.Init(this);
        }
    }

    public override void Activate()
    {
        base.Activate();
        if (started || IsSolved) return;
        started = true;

        fixedCount = 0;
        foreach (var deco in targetDecorations)
        {
            if (deco != null) deco.FlipToOne();
        }
    }

    /// <summary>Called by a PuzzleEDecoration when the player clicks it back to zero.</summary>
    public void NotifyDecorationFixed(PuzzleEDecoration deco)
    {
        if (IsSolved) return;

        fixedCount++;
        Debug.Log($"[PuzzleE] Decoration fixed. fixedCount={fixedCount}/{targetDecorations.Length}");

        if (fixedCount >= targetDecorations.Length)
        {
            Debug.Log("[PuzzleE] All decorations fixed -- calling MarkSolved()");
            MarkSolved();

            if (wallOpenAnimationRoot != null)
            {
                wallOpenAnimationRoot.SetActive(true);
            }

            if (GameManager.Instance != null)
            {
                Debug.Log("[PuzzleE] Calling GameManager.CompleteFinalPuzzle()");
                GameManager.Instance.CompleteFinalPuzzle();
            }
            else
            {
                Debug.LogWarning("[PuzzleE] GameManager.Instance was NULL -- CompleteFinalPuzzle() never called!");
            }
        }
    }
}