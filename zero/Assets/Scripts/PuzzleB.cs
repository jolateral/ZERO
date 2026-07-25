using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Puzzle B: four digits auto-count down independently and SIMULTANEOUSLY
/// (leftmost slowest, rightmost fastest), wrapping 9 after hitting 0.
/// The player can press the four buttons in ANY order, "catching" a digit
/// only if that digit currently reads 0. On a correct catch, that digit
/// freezes at 0 permanently. On an incorrect press (any digit that isn't
/// currently 0), every un-frozen digit's ticking flashes/resets and the
/// player has to start catching over -- but there is no fixed left-to-right
/// order requirement anymore.
/// Solved when all four digits are frozen at 0 (display reads 0000).
/// </summary>
public class PuzzleB : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;

    [Header("Countdown speeds per digit (seconds per tick), index 0 = leftmost/slowest")]
    [Tooltip("Doubled speed from the original pass (intervals halved).")]
    [SerializeField] private float[] tickIntervals = { 0.6f, 0.45f, 0.3f, 0.2f };

    [Header("Starting values (updated: 3 2 7 9)")]
    [SerializeField] private int[] startingDigits = { 3, 2, 7, 9 };

    [Header("Buttons (visual only placeholders -- swap sprites for red/green states)")]
    [SerializeField] private Image[] buttonImages = new Image[4];
    [SerializeField] private Color runningColor = Color.red;
    [SerializeField] private Color frozenColor = Color.green;

    private bool[] digitFrozen = new bool[4];
    private Coroutine[] tickRoutines = new Coroutine[4];
    private bool running = false;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < 4; i++)
        {
            display.SetDigit(i, startingDigits[i]);
        }
    }

    public override void Activate()
    {
        base.Activate();
        if (!running && !IsSolved)
        {
            running = true;
            StartAllTicking();
            RefreshButtonVisuals();
        }
    }

    private void StartAllTicking()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!digitFrozen[i] && tickRoutines[i] == null)
            {
                tickRoutines[i] = StartCoroutine(TickDigit(i));
            }
        }
    }

    private void StopAllTicking()
    {
        for (int i = 0; i < 4; i++)
        {
            if (tickRoutines[i] != null)
            {
                StopCoroutine(tickRoutines[i]);
                tickRoutines[i] = null;
            }
        }
    }

    private IEnumerator TickDigit(int index)
    {
        while (true)
        {
            yield return new WaitForSeconds(tickIntervals[index]);
            if (digitFrozen[index]) yield break;
            display.DecrementDigit(index, wrapAtZero: true); // wraps 0 -> 9
        }
    }

    // Hook all four UI Buttons' OnClick events to this, each passing its own index (0..3).
    // Order doesn't matter anymore -- any button can be pressed at any time.
    public void PressButton(int index)
    {
        if (IsSolved) return;
        if (digitFrozen[index]) return; // already caught, ignore further presses on it

        bool digitIsZero = (display.GetDigit(index) == 0);

        if (digitIsZero)
        {
            FreezeDigit(index);
            RefreshButtonVisuals();

            if (AllFrozen())
            {
                StopAllTicking();
                MarkSolved();
            }
        }
        else
        {
            ResetProgress();
        }
    }

    private bool AllFrozen()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!digitFrozen[i]) return false;
        }
        return true;
    }

    private void FreezeDigit(int index)
    {
        digitFrozen[index] = true;
        if (tickRoutines[index] != null)
        {
            StopCoroutine(tickRoutines[index]);
            tickRoutines[index] = null;
        }
    }

    private void ResetProgress()
    {
        // Un-freeze everything and resume ticking from wherever each digit
        // currently sits (matches the "everything starts running again" feedback --
        // it's the caught-progress that resets, not the raw digit values).
        for (int i = 0; i < 4; i++)
        {
            digitFrozen[i] = false;
        }
        StopAllTicking();
        StartAllTicking();
        RefreshButtonVisuals();
        // TODO: trigger a "flash" animation/SFX here per design doc feedback
    }

    private void RefreshButtonVisuals()
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (buttonImages[i] == null) continue;
            buttonImages[i].color = digitFrozen[i] ? frozenColor : runningColor;
        }
    }

    /// <summary>Called by GameManager when the player backs out without solving this puzzle.</summary>
    public override void ResetPuzzle()
    {
        if (IsSolved) return;
        StopAllTicking();
        running = false;
        for (int i = 0; i < 4; i++)
        {
            digitFrozen[i] = false;
            display.SetDigit(i, startingDigits[i]);
        }
        RefreshButtonVisuals();
        // If the puzzle is currently reachable (light isn't Off), resume ticking immediately.
        if (CurrentLightState != LightState.Off)
        {
            Activate();
        }
    }
}
