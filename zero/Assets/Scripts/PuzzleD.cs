using UnityEngine;

/// <summary>
/// Puzzle D: three unlabeled buttons, each wired (visually, misleadingly, per
/// the crossed-wire art) to subtract a fixed 4-digit amount from the display:
/// -0111, -0101, -1001. There's also a labeled "reset" button that restores
/// the display to its starting value. Solved when display reads 0000.
/// The crossed wires in the art are purely a visual red herring — functionally
/// each button always subtracts the same fixed amount regardless of wire path.
/// </summary>
public class PuzzleD : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;
    [SerializeField] private int startingValue = 3518; // matches design doc art (3518)

    [Header("Subtract amounts for the three wired buttons")]
    [SerializeField] private int subtractButton1 = 111;  // -0111
    [SerializeField] private int subtractButton2 = 101;   // -0101
    [SerializeField] private int subtractButton3 = 1001;  // -1001

    protected override void Awake()
    {
        base.Awake();
        display.SetValue(startingValue);
        display.OnReachedZero += HandleReachedZero;
    }

    // Hook these to the three wired UI Buttons' OnClick events.
    public void PressWiredButton1() => Subtract(subtractButton1);
    public void PressWiredButton2() => Subtract(subtractButton2);
    public void PressWiredButton3() => Subtract(subtractButton3);

    // Hook this to the labeled "reset" UI Button's OnClick event.
    public void PressReset()
    {
        if (IsSolved) return;
        display.SetValue(startingValue);
    }

    private void Subtract(int amount)
    {
        if (IsSolved) return;
        // Wraps around: e.g. 0003 - 5 = 9998, not clamped to 0. This stops players
        // from just spamming every button repeatedly to "safely" bottom out at zero.
        int wrapped = ((display.GetValue() - amount) % 10000 + 10000) % 10000;
        display.SetValue(wrapped);
    }

    private void HandleReachedZero()
    {
        MarkSolved();
    }
}