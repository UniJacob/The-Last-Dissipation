using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUnlockAnimation : MonoBehaviour
{
    [SerializeField] GameObject ScaledParticlesContainer;
    [SerializeField] ParticleSystem[] ParticleSystems;
    [SerializeField] float[] targetScales = { 0.5f, 3, 20 }; // The scales to pulse to
    [SerializeField] float[] TimesBetweenPulses = { 1, 1 }; // The scales to pulse to
    [SerializeField] float[] pulseDurations = { 0.5f, 0.5f, 0.5f }; // Duration of each pulse in seconds
    [SerializeField] AnimationCurve pulseShape = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Shape of the pulse
    [SerializeField] Canvas Canvas;
    [SerializeField] Image ChapterUnlockedImage;
    [SerializeField] float imageFadeDuration = 0.5f;
    [SerializeField] float imageDisplayDuration = 2f;

    public float AnimationTotalDuration { get; private set; }

    int currentPulseIndex;
    float pulseTimer;
    float startScale;
    bool isPulsing = false;
    float waitTimer;
    bool isAnimatingImage = false;
    float imageTimer;
    enum ImagePhase { FadeIn, Display, FadeOut, Done }
    ImagePhase currentImagePhase = ImagePhase.FadeIn;
    float extraTimeBeforeDestroy = 3;

    void Awake()
    {
        AnimationTotalDuration = pulseDurations.Sum() + TimesBetweenPulses.Sum() +
            imageFadeDuration + imageDisplayDuration;
    }
    void Start()
    {
        startScale = ScaledParticlesContainer.transform.localScale.x;
        Color c = ChapterUnlockedImage.color;
        c.a = 0;
        ChapterUnlockedImage.color = c;
        Canvas.worldCamera = GameState.ScreenAnimationsCamera;

        BeginAnimation();
    }

    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        if (isAnimatingImage)
        {
            AnimatingImage();
            return;
        }

        if (isPulsing)
        {
            pulseTimer += Time.deltaTime;
            float normalizedTime = pulseTimer / pulseDurations[currentPulseIndex];
            if (normalizedTime >= 1f)
            {
                PrepareForNextPulse();
                return;
            }

            // Calculate new scale based on animation curve
            float evaluatedTime = pulseShape.Evaluate(normalizedTime);
            float newScale = Mathf.Lerp(startScale, targetScales[currentPulseIndex], evaluatedTime);
            ScaledParticlesContainer.transform.localScale = new Vector3(newScale, newScale, newScale);
            return;
        }

        EndAnimation();
    }

    void AnimatingImage()
    {
        imageTimer += Time.deltaTime;
        Color c = ChapterUnlockedImage.color;

        switch (currentImagePhase)
        {
            case ImagePhase.FadeIn:
                c.a = imageTimer / imageFadeDuration;
                if (imageTimer >= imageFadeDuration)
                {
                    c.a = 1;
                    currentImagePhase = ImagePhase.Display;
                    imageTimer = 0;
                }
                break;

            case ImagePhase.Display:
                if (imageTimer >= imageDisplayDuration)
                {
                    currentImagePhase = ImagePhase.FadeOut;
                    imageTimer = 0;
                }
                break;

            case ImagePhase.FadeOut:
                c.a = 1 - (imageTimer / imageFadeDuration);
                if (imageTimer >= imageFadeDuration)
                {
                    c.a = 0;
                    currentImagePhase = ImagePhase.Done;
                    isAnimatingImage = false;
                }
                break;
        }

        ChapterUnlockedImage.color = c;
    }

    void PrepareForNextPulse()
    {
        // Reset for next pulse
        pulseTimer = 0f;
        ScaledParticlesContainer.transform.localScale = new Vector3(
            targetScales[currentPulseIndex],
            targetScales[currentPulseIndex],
            targetScales[currentPulseIndex]);
        startScale = ScaledParticlesContainer.transform.localScale.x;
        currentPulseIndex++;

        if (currentPulseIndex == targetScales.Length)
        {
            isPulsing = false;
            isAnimatingImage = true;
            imageTimer = 0;
            currentImagePhase = ImagePhase.FadeIn;
            return;
        }

        if (currentPulseIndex == targetScales.Length - 1)
        {
            foreach (ParticleSystem ps in ParticleSystems)
            {
                var main = ps.main;
                main.loop = false;
            }
        }
        waitTimer = TimesBetweenPulses[currentPulseIndex - 1];
    }

    void BeginAnimation()
    {
        currentPulseIndex = 0;
        pulseTimer = 0;
        waitTimer = 0;
        isPulsing = true;
        isAnimatingImage = false;
        currentImagePhase = ImagePhase.FadeIn;
        if (ChapterUnlockedImage != null)
        {
            Color c = ChapterUnlockedImage.color;
            c.a = 0;
            ChapterUnlockedImage.color = c;
        }
        OverworldState.IsInAnimation = true;
    }

    void EndAnimation()
    {
        OverworldState.IsInAnimation = false;
        if (extraTimeBeforeDestroy > 0)
        {
            extraTimeBeforeDestroy -= Time.deltaTime;
            return;
        }
        Destroy(gameObject);
    }
}