using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Puzzle B: only the CURRENTLY ACTIVE digit auto-counts down (leftmost first),
/// wrapping 9 after hitting 0, so the UI isn't cluttered with all four digits
/// moving at once. Player presses the active button to "catch" it at 0 —
/// on a correct catch, that digit freezes and the next digit starts ticking.
/// On an incorrect press (wrong button, or right button but digit isn't 0),
/// everything resets: the active index goes back to 0 and only the first
/// digit resumes ticking.
/// Solved when all four digits are frozen at 0 (display reads 0000).
/// </summary>
public class PuzzleB : PuzzleBase
{
    [SerializeField] private SevenSegmentDisplay display;

    [Header("Countdown speed per digit (seconds per tick), index 0 = leftmost")]
    [SerializeField] private float[] tickIntervals = { 0.9f, 0.7f, 0.45f, 0.3f };

    [Header("Starting values (matches design doc: 2 8 5 3)")]
    [SerializeField] private int[] startingDigits = { 2, 8, 5, 3 };

    [Header("Buttons (visual only placeholders — swap sprites for red/gray states)")]
    [SerializeField] private Image[] buttonImages = new Image[4];
    [SerializeField] private Color activeButtonColor = Color.red;
    [SerializeField] private Color inactiveButtonColor = Color.gray;

    private bool[] digitFrozen = new bool[4];
    private int activeIndex = 0;
    private Coroutine activeTickRoutine;
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
            StartTickingActiveDigit();
            RefreshButtonVisuals();
        }
    }

    private void StartTickingActiveDigit()
    {
        StopTicking();
        if (activeIndex < 4 && !digitFrozen[activeIndex])
        {
            activeTickRoutine = StartCoroutine(TickDigit(activeIndex));
        }
    }

    private void StopTicking()
    {
        if (activeTickRoutine != null)
        {
            StopCoroutine(activeTickRoutine);
            activeTickRoutine = null;
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

            if (activeIndex >= 4)
            {
                StopTicking();
                MarkSolved();
            }
            else
            {
                StartTickingActiveDigit();
            }
            RefreshButtonVisuals();
        }
        else
        {
            ResetProgress();
        }
    }

    private void FreezeDigit(int index)
    {
        digitFrozen[index] = true;
        StopTicking();
    }

    private void ResetProgress()
    {
        activeIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            digitFrozen[i] = false;
        }
        StartTickingActiveDigit();
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