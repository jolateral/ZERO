using UnityEngine;

/// <summary>
/// Puzzle A: four buttons (W, X, Y, Z), each hooked to one digit of the display.
/// Pressing a button decrements ONLY its own digit by 1 (clamped at 0, no wrap).
/// Solved when the display reads 0000.
/// Wire the four buttons' OnClick() in the Inspector to DecrementDigit0..3,
/// or hook them programmatically if you're instantiating buttons at runtime.
/// </summary>
public class PuzzleA : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;
    [SerializeField] private int startingValue = 3778; // matches design doc art (W X Y Z = 3 7 7 8)

    protected override void Awake()
    {
        base.Awake();
        display.SetValue(startingValue);
        display.OnReachedZero += HandleReachedZero;
    }

    // Hook these to your four UI Buttons' OnClick events.
    public void DecrementDigit0() => TryDecrement(0);
    public void DecrementDigit1() => TryDecrement(1);
    public void DecrementDigit2() => TryDecrement(2);
    public void DecrementDigit3() => TryDecrement(3);

    private void TryDecrement(int index)
    {
        if (IsSolved) return;
        display.DecrementDigit(index, wrapAtZero: false);
    }

    private void HandleReachedZero()
    {
        MarkSolved();
    }
}
