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
    [SerializeField] string DialogFileName = "start";

    static TextAnimator instance;
    string[] sentences;
    int currentIndex = 0;

    // Animation state variables
    private bool isAnimating = false;
    private float stopper = 0f;
    private int currentVisibleCount = 0;
    private int totalVisibleCharacters = 0;

    void Awake()
    {
        Auxiliary.AssureSingleton(ref instance, gameObject);
    }

    void Start()
    {
        if (DialogOnStart)
        {
            StartDialog(OverworldState.DialogsMap[DialogFileName]);
        }
    }

    void Update()
    {
        if (isAnimating)
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
                    isAnimating = false;
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
        OverworldState.InDialog = true;
        sentences = Auxiliary.TextAssetToSentences(ref Dialog);
        transform.gameObject.SetActive(true);
        stopper = 0;
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
        }
        else
        {
            OverworldState.InDialog = false;
            TextPanel.SetActive(false);
        }
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
        isAnimating = true;
    }

    /// <summary>
    /// Instantly completes the current text animation.
    /// </summary>
    public void CompleteCurrentAnimation()
    {
        if (isAnimating)
        {
            isAnimating = false;
            DialogTextBox.maxVisibleCharacters = totalVisibleCharacters;
            SentenceEndCursor.SetActive2(true);
        }
    }

    void OnDestroy()
    {
        isAnimating = false;
    }
}