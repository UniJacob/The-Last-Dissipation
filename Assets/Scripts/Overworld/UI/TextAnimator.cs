using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// A singleton script responsible for animating text in a "typing" fashion.
/// </summary>
public class TextAnimator : MonoBehaviour
{
    [SerializeField] TMP_Text DialogTextBox;
    [SerializeField] GameObject TextPanel;
    [SerializeField] float timeBetweenChars = 0.03f;
    [SerializeField] WaitCursorBehavior SentenceEndCursor;
    [SerializeField] bool DialogOnStart = false;
    [SerializeField] string StartDialogName = "start";

    static TextAnimator instance;
    string[] sentences;
    int currentIndex = 0;
    CancellationTokenSource currentAnimationCTS;

    private void Awake()
    {
        Auxiliary.AssureSingleton(ref instance, gameObject);
    }

    void Start()
    {
        if (DialogOnStart)
        {
            StartDialog(OverworldState.DialogsMap[StartDialogName]);
        }
    }

    /// <summary>
    /// Starts the current dialog.
    /// </summary>
    /// <param name="Dialog">A TextAsset containing all of the sentences of a dialog</param>
    public void StartDialog(TextAsset Dialog)
    {
        OverworldState.InDialog = true;
        sentences = Auxiliary.TextAssetToSentences(ref Dialog);
        transform.gameObject.SetActive(true);
        AnimateSentence();
    }

    /// <summary>
    /// Animates the next sentence (if available), with animation-cancelation handling.
    /// </summary>
    public async void AnimateSentence()
    {
        currentAnimationCTS?.Cancel();
        currentAnimationCTS?.Dispose();
        if (currentIndex <= sentences.Length - 1)
        {
            DialogTextBox.text = sentences[currentIndex++];
            if (!OverworldState.WebGLCompatibility)
            {
                currentAnimationCTS = new CancellationTokenSource();
                SentenceEndCursor.SetActive2(false);
                try
                {
                    await AnimateTextAsync(currentAnimationCTS.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
            SentenceEndCursor.SetActive2(true);
        }
        else
        {
            OverworldState.InDialog = false;
            TextPanel.SetActive(false);
        }

    }

    /// <summary>
    /// Helper async function to animate the next sentence.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken that would be called if the animation stopped midway</param>
    /// <returns>Task to be awaited</returns>
    async Task AnimateTextAsync(CancellationToken cancellationToken)
    {
        DialogTextBox.ForceMeshUpdate();
        int totalVisibleCharacters = DialogTextBox.textInfo.characterCount;
        DialogTextBox.maxVisibleCharacters = 0;

        for (int visibleCount = 0; visibleCount <= totalVisibleCharacters; visibleCount++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DialogTextBox.maxVisibleCharacters = visibleCount;
            if (visibleCount < totalVisibleCharacters)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(timeBetweenChars), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    DialogTextBox.maxVisibleCharacters = totalVisibleCharacters;
                    throw;
                }
            }
        }
    }

    void OnDestroy()
    {
        try
        {
            currentAnimationCTS?.Cancel();
            currentAnimationCTS?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            return;
        }
    }
}