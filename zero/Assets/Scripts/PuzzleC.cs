using System.Collections;
using UnityEngine;

/// <summary>
/// Puzzle C: display auto-counts down by 1 every tick. There is a single
/// unlabeled button that ADDS 11 to the display (a trap — the solution is
/// to simply wait and not touch it). Solved when the display reaches 0000.
/// </summary>
public class PuzzleC : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;
    [SerializeField] private int startingValue = 32; // matches design doc art (0032)
    [SerializeField] private float tickInterval = 1f; // seconds per auto-decrement

    private Coroutine countdownRoutine;
    private bool running = false;

    protected override void Awake()
    {
        base.Awake();
        display.SetValue(startingValue);
        display.OnReachedZero += HandleReachedZero;
    }

    public override void Activate()
    {
        base.Activate();
        if (!running && !IsSolved)
        {
            running = true;
            countdownRoutine = StartCoroutine(CountDown());
        }
    }

    private IEnumerator CountDown()
    {
        while (display.GetValue() > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            if (IsSolved) yield break;
            display.AddToValue(-1);
        }
    }

    // Hook this to the single UI Button's OnClick event.
    public void PressTrapButton()
    {
        if (IsSolved) return;
        display.AddToValue(11);
    }

    private void HandleReachedZero()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }
        MarkSolved();
    }
}
