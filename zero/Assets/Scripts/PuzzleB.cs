using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Puzzle B: four digits auto-count down independently at different speeds
/// (leftmost slowest, rightmost fastest), wrapping 9 after hitting 0.
/// Player must press the buttons LEFT TO RIGHT, each press only "catching"
/// its digit if that digit currently reads 0. On a correct catch, that digit
/// freezes at 0 and the next button becomes the active one.
/// On an incorrect press (wrong button, or right button but digit isn't 0),
/// everything resets: all digits resume auto-counting and the active index
/// resets to 0 (first button).
/// Solved when all four digits are frozen at 0 (display reads 0000).
/// </summary>
public class PuzzleB : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;

    [Header("Countdown speeds per digit (seconds per tick), index 0 = leftmost/slowest")]
    [SerializeField] private float[] tickIntervals = { 1.2f, 0.9f, 0.6f, 0.4f };

    [Header("Starting values (matches design doc: 2 8 5 3)")]
    [SerializeField] private int[] startingDigits = { 2, 8, 5, 3 };

    [Header("Buttons (visual only placeholders — swap sprites for red/gray states)")]
    [SerializeField] private Image[] buttonImages = new Image[4];
    [SerializeField] private Color activeButtonColor = Color.red;
    [SerializeField] private Color inactiveButtonColor = Color.gray;

    private bool[] digitFrozen = new bool[4];
    private int activeIndex = 0;
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
            if (!digitFrozen[i])
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
            display.DecrementDigit(index, wrapAtZero: true); // wraps 0 -> 9 per design doc
        }
    }

    // Hook these to your four UI Buttons' OnClick events (index 0..3, left to right).
    public void PressButton(int index)
    {
        if (IsSolved) return;

        bool correctButton = (index == activeIndex);
        bool digitIsZero = (display.GetDigit(index) == 0);

        if (correctButton && digitIsZero)
        {
            FreezeDigit(index);
            activeIndex++;
            RefreshButtonVisuals();

            if (activeIndex >= 4)
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
        activeIndex = 0;
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
            buttonImages[i].color = (i == activeIndex) ? activeButtonColor : inactiveButtonColor;
        }
    }
}
