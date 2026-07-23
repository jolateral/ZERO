using System;
using UnityEngine;
using TMPro;

/// <summary>
/// A reusable 4-digit numeric display. Uses TMP_Text placeholders for each digit
/// so you can wire this up immediately without art, then later swap the digit
/// rendering (e.g. in RenderDigit) for real seven-segment sprites/atlases.
/// Value range is clamped to 0-9999 by default (4 digits, wraps per-digit if enabled).
/// </summary>
public class SevenSegmentDisplay : MonoBehaviour
{
    [Header("Digit Text Slots (index 0 = leftmost/thousands)")]
    [SerializeField] private TMP_Text[] digitSlots = new TMP_Text[4];

    [Header("Behavior")]
    [Tooltip("If true, individual digits wrap 9->0 or 0->9 instead of clamping the whole value.")]
    [SerializeField] private bool perDigitWrap = false;

    [Tooltip("Colors are placeholder-only; swap for sprite swaps once art arrives.")]
    [SerializeField] private Color activeColor = new Color(0.6f, 1f, 0.2f); // lime green placeholder

    private int[] digits = new int[4];

    public event Action OnReachedZero;
    public event Action<int[]> OnValueChanged;

    private void Awake()
    {
        RefreshVisuals();
    }

    /// <summary>Set the whole 4-digit value at once (0-9999).</summary>
    public void SetValue(int value)
    {
        value = Mathf.Clamp(value, 0, 9999);
        digits[0] = (value / 1000) % 10;
        digits[1] = (value / 100) % 10;
        digits[2] = (value / 10) % 10;
        digits[3] = value % 10;
        RefreshVisuals();
        CheckZero();
    }

    /// <summary>Get the whole value as an int.</summary>
    public int GetValue()
    {
        return digits[0] * 1000 + digits[1] * 100 + digits[2] * 10 + digits[3];
    }

    /// <summary>Get a single digit (0-3 index, 0 = leftmost).</summary>
    public int GetDigit(int index) => digits[index];

    /// <summary>Set a single digit directly (0-9). Useful for Puzzle B's independent countdowns.</summary>
    public void SetDigit(int index, int value)
    {
        if (perDigitWrap)
        {
            value = ((value % 10) + 10) % 10; // wrap
        }
        else
        {
            value = Mathf.Clamp(value, 0, 9);
        }
        digits[index] = value;
        RefreshVisuals();
        OnValueChanged?.Invoke(digits);
        CheckZero();
    }

    /// <summary>Decrement a single digit by 1, with optional wrap 0->9 (per Puzzle B design doc).</summary>
    public void DecrementDigit(int index, bool wrapAtZero)
    {
        int v = digits[index] - 1;
        if (v < 0)
        {
            v = wrapAtZero ? 9 : 0;
        }
        SetDigit(index, v);
    }

    /// <summary>Add a delta to the whole 4-digit number (used by Puzzle C's +11 and Puzzle D's -0111 etc).</summary>
    public void AddToValue(int delta)
    {
        SetValue(GetValue() + delta);
    }

    private void CheckZero()
    {
        if (digits[0] == 0 && digits[1] == 0 && digits[2] == 0 && digits[3] == 0)
        {
            OnReachedZero?.Invoke();
        }
    }

    private void RefreshVisuals()
    {
        for (int i = 0; i < digitSlots.Length; i++)
        {
            if (digitSlots[i] == null) continue;
            digitSlots[i].text = digits[i].ToString();
            digitSlots[i].color = activeColor;
        }
        OnValueChanged?.Invoke(digits);
    }
}
