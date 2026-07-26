using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// One "zero" decoration on the wall (word "zero" or digit "0"). Most of your
/// wall's zeros/0's can just stay static background art with no script at
/// all -- only assign this component to the specific decorations you want to
/// be interactive for Puzzle E (the 3 that will flip to "1"/"one").
///
/// Placeholder-friendly: uses a single TMP_Text whose string is swapped
/// between "0"/"zero" and "1"/"one", with a simple color pulse standing in
/// for a glow effect until real art exists.
/// </summary>
public class PuzzleEDecoration : MonoBehaviour
{
    [Header("Text to swap between")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private bool wordMode = false; // false = "0"/"1", true = "zero"/"one"

    [Header("Placeholder glow (color pulse) while this decoration reads '1'")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color glowColorA = new Color(1f, 0.9f, 0.2f); // warm yellow
    [SerializeField] private Color glowColorB = new Color(1f, 0.4f, 0.1f); // warm orange
    [SerializeField] private float glowPulseSpeed = 2f;

    private bool isFlippedToOne = false;
    private Coroutine glowRoutine;
    private PuzzleE owner;

    private string ZeroText => wordMode ? "zero" : "0";
    private string OneText => wordMode ? "one" : "1";

    private void Awake()
    {
        SetZeroVisual();
    }

    /// <summary>Called once by PuzzleE when it registers this as one of its target decorations.</summary>
    public void Init(PuzzleE puzzleE)
    {
        owner = puzzleE;
    }

    /// <summary>Flip this decoration from "0"/"zero" to "1"/"one" and start the glow pulse.</summary>
    public void FlipToOne()
    {
        isFlippedToOne = true;
        if (label != null) label.text = OneText;
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(GlowPulse());
    }

    // Hook this decoration's Button OnClick() (or a raycast-target Image + EventTrigger) to this.
    public void OnClicked()
    {
        if (!isFlippedToOne) return; // ambient decoration, or already fixed -- ignore clicks
        SetZeroVisual();
        owner?.NotifyDecorationFixed(this);
    }

    private void SetZeroVisual()
    {
        isFlippedToOne = false;
        if (label != null)
        {
            label.text = ZeroText;
            label.color = normalColor;
        }
        if (glowRoutine != null)
        {
            StopCoroutine(glowRoutine);
            glowRoutine = null;
        }
    }

    private IEnumerator GlowPulse()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * glowPulseSpeed;
            if (label != null)
            {
                label.color = Color.Lerp(glowColorA, glowColorB, (Mathf.Sin(t) + 1f) / 2f);
            }
            yield return null;
        }
    }
}
