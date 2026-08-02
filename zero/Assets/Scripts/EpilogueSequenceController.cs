using System.Collections;
using UnityEngine;

/// <summary>
/// Plays after Puzzle E is solved and the open-hallway view is showing.
/// Fades in epilogue paragraph lines one at a time (with a hold between
/// each), then hard-cuts straight to the credits display (no fade -- an
/// intentional tonal snap after the slow reveal).
///
/// Call PlayEpilogue() once, from GameManager, after the door-open /
/// hallway-swap transition has had a moment to breathe.
/// </summary>
public class EpilogueSequenceController : MonoBehaviour
{
    [Header("Epilogue paragraph, one CanvasGroup per line, in reading order")]
    [SerializeField] private CanvasGroup[] epilogueLines;

    [Header("Timing")]
    [SerializeField] private float lineFadeDuration = 1.2f;
    [SerializeField] private float holdBetweenLines = 1.5f;
    [SerializeField] private float holdAfterLastLine = 2.5f;

    [Header("Credits (hard cut, no fade)")]
    [SerializeField] private GameObject creditsDisplay;

    [Header("Root object for this whole sequence")]
    [Tooltip("Hidden until PlayEpilogue() is called. Should sit on top of / adjacent to the open-hallway view.")]
    [SerializeField] private GameObject sequenceRoot;

    private bool hasPlayed = false;

    private void Awake()
    {
        if (sequenceRoot != null) sequenceRoot.SetActive(false);
        if (creditsDisplay != null) creditsDisplay.SetActive(false);

        foreach (var line in epilogueLines)
        {
            if (line != null) SetAlpha(line, 0f);
        }
    }

    /// <summary>Called once by GameManager once Puzzle E's door-open transition completes.</summary>
    public void PlayEpilogue()
    {
        if (hasPlayed) return; // guard against being triggered twice
        hasPlayed = true;

        if (sequenceRoot != null) sequenceRoot.SetActive(true);
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        foreach (var line in epilogueLines)
        {
            if (line == null) continue;

            yield return FadeIn(line);
            yield return new WaitForSeconds(holdBetweenLines);
        }

        yield return new WaitForSeconds(holdAfterLastLine);

        // Hard cut: paragraph vanishes, credits appear instantly. No crossfade here on purpose.
        foreach (var line in epilogueLines)
        {
            if (line != null) SetAlpha(line, 0f);
        }

        if (creditsDisplay != null)
        {
            creditsDisplay.SetActive(true);
        }
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        float elapsed = 0f;
        while (elapsed < lineFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(group, Mathf.Lerp(0f, 1f, elapsed / lineFadeDuration));
            yield return null;
        }
        SetAlpha(group, 1f);
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        group.alpha = alpha;
    }
}
