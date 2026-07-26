using System;
using UnityEngine;

/// <summary>
/// A reusable 4-digit display driving UI Animators.
/// Set Value updates the 'DigitValue' parameter in each slot's Animator.
/// </summary>
public class SevenSegmentDisplay : MonoBehaviour
{
    [Header("Digit Animator Slots (index 0 = leftmost)")]
    [SerializeField] private Animator[] digitAnimators = new Animator[4];

    [Header("Behavior")]
    [Tooltip("If true, individual digits wrap 9->0 or 0->9 instead of clamping.")]
    [SerializeField] private bool perDigitWrap = false;

    private static readonly int DigitValueHash = Animator.StringToHash("DigitValue");
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

    public int GetValue() => digits[0] * 1000 + digits[1] * 100 + digits[2] * 10 + digits[3];
    public int GetDigit(int index) => digits[index];

    public void SetDigit(int index, int value)
    {
        if (perDigitWrap)
        {
            value = ((value % 10) + 10) % 10;
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

    public void DecrementDigit(int index, bool wrapAtZero)
    {
        int v = digits[index] - 1;
        if (v < 0)
        {
            v = wrapAtZero ? 9 : 0;
        }
        SetDigit(index, v);
    }

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
        for (int i = 0; i < digitAnimators.Length; i++)
        {
            if (digitAnimators[i] == null) continue;

            // Sets the 'DigitValue' int parameter in the Animator controller
            digitAnimators[i].SetInteger(DigitValueHash, digits[i]);
        }
        OnValueChanged?.Invoke(digits);
    }
}