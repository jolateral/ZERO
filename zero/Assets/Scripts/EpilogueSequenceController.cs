using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Plays the ending sequence once Puzzle E is solved and the wall opens:
///
/// PHASE 1 -- Epilogue paragraph: each line fades in one after another, but
/// (unlike the credits) they all STAY on screen, so by the end of the phase
/// every line of the paragraph is visible together.
///
/// PHASE 2 -- Title + credits: hard cuts. Only one line is ever visible at a
/// time -- "ZERO", then "A Game By...", "Coded by...", etc. Each new line
/// replaces the previous one abruptly (no crossfade between lines), though
/// each line can still have its own quick fade-in on arrival if you want a
/// little polish (set Credit Fade In Duration to 0 for a truly instant, no-
/// fade hard cut).
///
/// Call PlayEpilogue() once (e.g. from GameManager after Puzzle E solves).
/// </summary>
public class EpilogueSequenceController : MonoBehaviour
{
    [Header("Phase 1: Epilogue paragraph (accumulates, stays fully visible)")]
    [Tooltip("One CanvasGroup per line of the epilogue paragraph, in the order they should appear. Each should already contain its own TMP_Text with the line's text and be positioned/stacked in the scene, alpha starting at 0.")]
    [SerializeField] private CanvasGroup[] epilogueLineGroups;
    [SerializeField] private float epilogueLineFadeDuration = 1f;
    [SerializeField] private float delayBetweenEpilogueLines = 1.2f;
    [SerializeField] private float holdAfterEpilogueParagraph = 2f;
    [SerializeField] private float epilogueParagraphFadeOutDuration = 1f;

    [Header("Phase 1 container (optional, fades the whole paragraph out together after the hold)")]
    [SerializeField] private CanvasGroup epilogueParagraphRoot;

    [Header("Phase 2: Title + credits (hard cut, one line visible at a time)")]
    [SerializeField] private CanvasGroup creditsGroup;
    [SerializeField] private TMP_Text creditsText;
    [TextArea]
    [SerializeField] private string[] creditLines = new string[]
    {
        "ZERO",
        "A Game By Joshua Manrique",
        "Coded by Jo Nguyen",
        "Art by Laura",
        "Audio Design by Dominik Kosciolek"
    };
    [SerializeField] private float creditFadeInDuration = 0.6f; // set to 0 for a truly instant hard cut with no fade-in
    [SerializeField] private float creditHoldDuration = 2f;

    private bool played = false;

    private void Awake()
    {
        // Make sure everything starts invisible/non-blocking until PlayEpilogue() runs.
        foreach (var group in epilogueLineGroups)
        {
            if (group != null) SetGroupVisible(group, false);
        }
        if (epilogueParagraphRoot != null) SetGroupVisible(epilogueParagraphRoot, true); // container itself visible; individual lines control their own reveal
        if (creditsGroup != null) SetGroupVisible(creditsGroup, false);
    }

    /// <summary>Call this once, e.g. from GameManager right after Puzzle E is solved / the wall-open animation plays.</summary>
    public void PlayEpilogue()
    {
        Debug.Log($"[Epilogue] PlayEpilogue() called. played={played}, gameObject.activeInHierarchy={gameObject.activeInHierarchy}");
        if (played) return;
        played = true;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        Debug.Log($"[Epilogue] PlaySequence() started. epilogueLineGroups count={epilogueLineGroups.Length}");
        // --- Phase 1: epilogue paragraph, lines accumulate ---
        foreach (var group in epilogueLineGroups)
        {
            if (group == null)
            {
                Debug.LogWarning("[Epilogue] A slot in Epilogue Line Groups is NULL -- skipping it.");
                continue;
            }
            yield return FadeIn(group, epilogueLineFadeDuration);
            yield return new WaitForSeconds(delayBetweenEpilogueLines);
        }

        yield return new WaitForSeconds(holdAfterEpilogueParagraph);

        // Fade the whole paragraph out together before starting the hard-cut credits.
        if (epilogueParagraphRoot != null)
        {
            yield return Fade(epilogueParagraphRoot, epilogueParagraphRoot.alpha, 0f, epilogueParagraphFadeOutDuration);
            SetGroupVisible(epilogueParagraphRoot, false);
        }
        else
        {
            // No shared container assigned -- fade each line out individually instead.
            foreach (var group in epilogueLineGroups)
            {
                if (group == null) continue;
                yield return Fade(group, group.alpha, 0f, epilogueParagraphFadeOutDuration);
                SetGroupVisible(group, false);
            }
        }

        // --- Phase 2: title + credits, hard cut, one line at a time ---
        foreach (var line in creditLines)
        {
            yield return ShowHardCutLine(line);
        }

        // Sequence complete. Hook whatever comes next here (e.g. return to main menu,
        // reload the scene, show a "play again" button) once you know what that is.
    }

    private IEnumerator ShowHardCutLine(string line)
    {
        if (creditsText != null) creditsText.text = line;

        if (creditFadeInDuration > 0f)
        {
            SetGroupVisible(creditsGroup, true);
            creditsGroup.alpha = 0f;
            yield return Fade(creditsGroup, 0f, 1f, creditFadeInDuration);
        }
        else
        {
            // Truly instant hard cut -- no fade at all.
            SetGroupVisible(creditsGroup, true);
            creditsGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(creditHoldDuration);

        // Hard cut out -- instantly hidden, no fade, before the next line starts.
        SetGroupVisible(creditsGroup, false);
    }

    private IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        SetGroupVisible(group, true);
        yield return Fade(group, 0f, 1f, duration);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private void SetGroupVisible(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? group.alpha : 0f;
        group.blocksRaycasts = visible;
        group.interactable = visible;
    }
}
