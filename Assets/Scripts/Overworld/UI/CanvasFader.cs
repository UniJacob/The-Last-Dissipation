using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script for a UI canvas with Image and TMP_Text coomponents that causes them to fade in when
/// gameObject.SetActive(true) is called and fade out when CanvasFader.TriggerFadeOut() is called.
/// </summary>
public class CanvasFader : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.1f;

    Dictionary<Image, float> imageOriginalAlphas = new();
    Dictionary<TMP_Text, float> textOriginalAlphas = new();
    float fadeStartTime;
    float startAlpha;
    float currentDuration;
    bool isFading = false;
    bool isFadingIn = false;

    void Awake()
    {
        // Cache and store original alphas
        foreach (var image in GetComponentsInChildren<Image>(true))
        {
            imageOriginalAlphas[image] = image.color.a;
        }

        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            textOriginalAlphas[text] = text.color.a;
        }
    }

    void OnEnable()
    {
        StartFade(true);
    }

    void OnDisable()
    {
        if (isFading)
        {
            SetElementsAlpha(0f);
            isFading = false;
        }
    }

    void Update()
    {
        if (!isFading) return;

        float elapsedTime = Time.unscaledTime- fadeStartTime;

        if (elapsedTime >= currentDuration)
        {
            // Fade is complete
            if (isFadingIn)
            {
                // Restore original alpha values
                foreach (var pair in imageOriginalAlphas)
                {
                    Color color = pair.Key.color;
                    color.a = pair.Value;
                    pair.Key.color = color;
                }
                foreach (var pair in textOriginalAlphas)
                {
                    Color color = pair.Key.color;
                    color.a = pair.Value;
                    pair.Key.color = color;
                }
            }
            else
            {
                SetElementsAlpha(0f);
            }

            isFading = false;

            if (!isFadingIn)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            float normalizedTime = elapsedTime / currentDuration;
            float targetAlpha = isFadingIn ? 1f : 0f;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            SetElementsAlpha(currentAlpha);
        }
    }

    public void TriggerFadeOut()
    {
        StartFade(false);
    }
    public async void TriggerFadeOut(float delayInSeconds)
    {
        await Awaitable.WaitForSecondsAsync(delayInSeconds);
        TriggerFadeOut();
    }


    void StartFade(bool fadeIn)
    {
        isFading = true;
        isFadingIn = fadeIn;
        fadeStartTime = Time.unscaledTime;
        currentDuration = fadeIn ? fadeInDuration : fadeOutDuration;
        startAlpha = fadeIn ? 0f : 1f;

        SetElementsAlpha(startAlpha);
    }

    void SetElementsAlpha(float fadeProgress)
    {
        foreach (var pair in imageOriginalAlphas)
        {
            if (pair.Key != null)
            {
                Color color = pair.Key.color;
                color.a = fadeProgress * pair.Value;
                pair.Key.color = color;
            }
        }

        foreach (var pair in textOriginalAlphas)
        {
            if (pair.Key != null)
            {
                Color color = pair.Key.color;
                color.a = fadeProgress * pair.Value;
                pair.Key.color = color;
            }
        }
    }

    public void SetFadeDurations(float fadeIn, float fadeOut)
    {
        fadeInDuration = fadeIn;
        fadeOutDuration = fadeOut;
    }
}