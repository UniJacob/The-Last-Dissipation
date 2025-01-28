using TMPro;
using UnityEngine;

/// <summary>
/// A script responsible for animating text in a "typing" fashion.
/// </summary>
public class TextAnimator : MonoBehaviour
{
    [SerializeField] TMP_Text DialogTextBox;
    [SerializeField] GameObject TextPanel;
    [SerializeField] float timeBetweenChars = 0.03f;
    [SerializeField] WaitCursorBehavior SentenceEndCursor;
    [SerializeField] bool DialogOnStart = false;
    [SerializeField] string DialogFileName = "start";

    string[] sentences;
    int currentIndex = 0;

    // Animation state variables
    bool isTyping = false;
    float stopper = 0f;
    int currentVisibleCount = 0;
    int totalVisibleCharacters = 0;

    void Start()
    {
        if (DialogOnStart && !GameState.ExhaustedDialogs.Contains(DialogFileName))
        {
            gameObject.SetActive(true);
            StartDialog(OverworldState.DialogsMap[DialogFileName]);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isTyping)
        {
            stopper += Time.deltaTime;

            // Check if it's time to show the next character
            if (stopper >= timeBetweenChars)
            {
                stopper -= timeBetweenChars;
                currentVisibleCount++;
                DialogTextBox.maxVisibleCharacters = currentVisibleCount;

                // Check if animation is complete
                if (currentVisibleCount >= totalVisibleCharacters)
                {
                    isTyping = false;
                    SentenceEndCursor.SetActive2(true);
                }
            }
        }
    }

    /// <summary>
    /// Starts the current dialog.
    /// </summary>
    /// <param name="Dialog">A TextAsset containing all of the sentences of a dialog</param>
    public void StartDialog(TextAsset Dialog)
    {
        OverworldState.IsInDialog = true;
        sentences = Auxiliary.TextAssetToSentences(ref Dialog);
        transform.gameObject.SetActive(true);
        stopper = 0;
        GameState.ExhaustedDialogs.Add(DialogFileName);

        AnimateSentence();
    }

    /// <summary>
    /// Animates the next sentence (if available).
    /// </summary>
    void AnimateSentence()
    {
        if (currentIndex <= sentences.Length - 1)
        {
            DialogTextBox.text = sentences[currentIndex++];
            StartTextAnimation();
            return;
        }
        OverworldState.IsInDialog = false;
        if (GetComponent<CanvasFader>() != null)
        {
            GetComponent<CanvasFader>().TriggerFadeOut();
        }
        else
        {
            gameObject.SetActive(false);
        }
        GameState.SaveGameState();
    }

    /// <summary>
    /// Starts the text animation for the current sentence.
    /// </summary>
    void StartTextAnimation()
    {
        DialogTextBox.ForceMeshUpdate();
        totalVisibleCharacters = DialogTextBox.textInfo.characterCount;
        currentVisibleCount = 0;
        DialogTextBox.maxVisibleCharacters = 0;
        stopper = 0f;

        SentenceEndCursor.SetActive2(false);
        isTyping = true;
    }

    /// <summary>
    /// Instantly completes the current text animation.
    /// </summary>
    public void CompleteCurrentAnimation()
    {
        if (isTyping)
        {
            isTyping = false;
            DialogTextBox.maxVisibleCharacters = totalVisibleCharacters;
            SentenceEndCursor.SetActive2(true);
        }
    }

    void OnDestroy()
    {
        isTyping = false;
    }
}