using UnityEngine;

/// <summary>
/// Puzzle D: three unlabeled buttons, each wired (visually, misleadingly, per
/// the crossed-wire art) to subtract a fixed 4-digit amount from the WHOLE
/// display value: -0111, -0101, -1001. There's also a labeled "reset" button
/// that restores the display to its starting value. Solved when display
/// reads 0000. The crossed wires in the art are purely a visual red herring --
/// functionally each button always subtracts the same fixed amount from the
/// whole number regardless of wire path.
///
/// NOTE: because subtraction happens on the whole 4-digit number, digits
/// other than the ones a wire visually points to WILL change whenever the
/// subtraction crosses a place-value boundary (e.g. 3007 - 111 = 2896 --
/// the thousands digit has to change too). That's expected arithmetic, not
/// a bug. This version adds a small per-button debounce so that rapid
/// double-fires from a single click (e.g. duplicate OnClick bindings, or a
/// UI Button re-triggering on pointer-up+pointer-click) can't sneak in an
/// extra subtraction you didn't intend.
/// </summary>
public class PuzzleD : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;
    [SerializeField] private int startingValue = 3518; // matches design doc art (3518)

    [Header("Subtract amounts for the three wired buttons")]
    [SerializeField] private int subtractButton1 = 111;  // -0111
    [SerializeField] private int subtractButton2 = 101;   // -0101
    [SerializeField] private int subtractButton3 = 1001;  // -1001

    [Header("Debounce (seconds) - ignores a second press on the same button within this window")]
    [SerializeField] private float pressDebounce = 0.1f;

    private float lastPress1Time = -999f;
    private float lastPress2Time = -999f;
    private float lastPress3Time = -999f;

    protected override void Awake()
    {
        base.Awake();
        display.SetValue(startingValue);
        display.OnReachedZero += HandleReachedZero;
    }

    // Hook these to the three wired UI Buttons' OnClick events.
    public void PressWiredButton1()
    {
        if (Time.unscaledTime - lastPress1Time < pressDebounce) return;
        lastPress1Time = Time.unscaledTime;
        Subtract(subtractButton1);
    }

    public void PressWiredButton2()
    {
        if (Time.unscaledTime - lastPress2Time < pressDebounce) return;
        lastPress2Time = Time.unscaledTime;
        Subtract(subtractButton2);
    }

    public void PressWiredButton3()
    {
        if (Time.unscaledTime - lastPress3Time < pressDebounce) return;
        lastPress3Time = Time.unscaledTime;
        Subtract(subtractButton3);
    }

    // Hook this to the labeled "reset" UI Button's OnClick event.
    public void PressReset()
    {
        if (IsSolved) return;
        display.SetValue(startingValue);
    }

    private void Subtract(int amount)
    {
        if (IsSolved) return;
        // Per-digit subtraction with independent wraparound -- NOT whole-number
        // subtraction. Each of the 4 digits has the matching digit of `amount`
        // subtracted from it on its own, wrapping 0 -> 9 if it goes negative.
        // There is no borrowing/carrying between digits (e.g. 3518 - 0111 = 3407,
        // but 3007 - 0111 = 3996, since the hundreds/tens/ones each wrap on
        // their own instead of the thousands digit absorbing a carry).
        int[] amountDigits =
        {
            (amount / 1000) % 10,
            (amount / 100) % 10,
            (amount / 10) % 10,
            amount % 10
        };

        for (int i = 0; i < 4; i++)
        {
            int newDigit = ((display.GetDigit(i) - amountDigits[i]) % 10 + 10) % 10;
            display.SetDigit(i, newDigit);
        }
    }

    private void HandleReachedZero()
    {
        MarkSolved();
    }

    /// <summary>Called by GameManager when the player backs out without solving this puzzle.</summary>
    public override void ResetPuzzle()
    {
        if (IsSolved) return;
        display.SetValue(startingValue);
    }
}