using System;
using UnityEngine;
using UnityEngine.UI;

public enum LightState { Off, Red, Green }

/// <summary>
/// Base class for every puzzle panel. Handles the lightbulb state machine
/// (Off -> Red -> Green) and firing the OnSolved event that the GameManager
/// listens to in order to unlock the next puzzle and decrement the center counter.
/// </summary>
public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Identity")]
    [SerializeField] protected string puzzleId = "A";

    [Header("Light (placeholder: just an Image tinted by color)")]
    [SerializeField] protected Image lightBulbImage;
    [SerializeField] protected Color offColor = Color.gray;
    [SerializeField] protected Color redColor = Color.red;
    [SerializeField] protected Color greenColor = Color.green;

    public bool IsSolved { get; protected set; }
    public LightState CurrentLightState { get; protected set; } = LightState.Off;

    public event Action<PuzzleBase> OnSolved;

    protected virtual void Awake()
    {
        SetLight(LightState.Off);
    }

    /// <summary>Called by GameManager when this puzzle becomes reachable (previous puzzle solved).</summary>
    public virtual void Activate()
    {
        if (!IsSolved)
        {
            SetLight(LightState.Red);
        }
    }

    public void SetLight(LightState state)
    {
        CurrentLightState = state;
        if (lightBulbImage == null) return;

        switch (state)
        {
            case LightState.Off: lightBulbImage.color = offColor; break;
            case LightState.Red: lightBulbImage.color = redColor; break;
            case LightState.Green: lightBulbImage.color = greenColor; break;
        }
    }

    protected void MarkSolved()
    {
        if (IsSolved) return;
        IsSolved = true;
        SetLight(LightState.Green);
        OnSolved?.Invoke(this);
    }

    public string PuzzleId => puzzleId;
}
