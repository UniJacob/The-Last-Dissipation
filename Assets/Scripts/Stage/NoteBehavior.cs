using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script responsible for the behavior of the Notes, from functionalityto animation .
/// </summary>
public class NoteBehavior : MonoBehaviour
{
    [SerializeField] NoteProperties NoteProperties;
    [SerializeField] ScoreManager ScoreManager;
    [SerializeField] Collider2D NoteCollider;
    [SerializeField] SpriteRenderer MainSpriteRenderer;
    [SerializeField] SpriteRenderer TappedSpriteRenderer;

    /* Note Animation */
    bool WasTapped = false;
    bool WasMissed = false;
    bool IsFadingOut = true, IsScalingOut = false;
    bool IsFadingIn = true, IsScalingIn = false;
    bool ToDestroy = false;
    float ScaleWhenTapped = 0;
    readonly Dictionary<string, Transform> TransformsMap = new Dictionary<string, Transform>();
    SpriteRenderer[] spriteRenderers;

    /* Note Lifetime Timers */
    float TimeAtActivation = -1;
    float MissedStopper = 0, TimeBeforeMissed;
    float FadeInTimer;
    float ScaleInTimer;
    float TappedScaleTimer;
    float FadeOutTimer;

    /* Timing/Score Related */
    float TimeAtPerfect;
    bool scoreCalculated = false;

    /* Debug */
    public static float LastFullGrownTime = 0;

    void Start()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag(NoteProperties.TappedNoteTag))
            {
                TransformsMap.Add(NoteProperties.TappedNoteTag, child);
            }
        }
        SetInitialScale();
        SetAlpha(0, NoteProperties.MainNoteTag);
        SetTimers();
    }

    /// <summary>
    /// Sets the Note initial scale (usually smaller than normal).
    /// </summary>
    void SetInitialScale()
    {
        float initialScale = NoteProperties.DefaultSize * NoteProperties.SpawnScaleMultiplier;
        transform.localScale = new(initialScale, initialScale, 1);
    }

    /// <summary>
    /// Sets the alpha value of the color of every Note sprite with a given tag.
    /// </summary>
    /// <param name="alphaValue">Normalaized new alpha value</param>
    /// <param name="tag">Tag to change alpha for</param>
    private void SetAlpha(float alphaValue, string tag)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr.CompareTag(tag))
            {
                Color col = sr.color;
                col.a = alphaValue;
                sr.color = col;
            }
        }
    }

    /// <summary>
    /// Sets various process-tracking timers.
    /// </summary>
    void SetTimers()
    {
        FadeInTimer = NoteProperties.FadeInTime;
        ScaleInTimer = NoteProperties.ScaleInTime + FadeInTimer;
        //MainLifeTimer = NP.MainLifeTime;
        TappedScaleTimer = NoteProperties.TappedScaleTime;
        FadeOutTimer = NoteProperties.FadeOutTime;

        TimeBeforeMissed = NoteProperties.MainLifeTime + NoteProperties.FadeInTime + NoteProperties.ScaleInTime;
        TimeAtPerfect = TimeAtActivation + ScaleInTimer;

        if (TimeAtActivation > 0)
        {
            float tmp = (Time.time - TimeAtActivation);
            FadeInTimer -= tmp;
            ScaleInTimer -= tmp;
            TimeAtActivation = 0;
        }
    }

    void Update()
    {
        FadeInTimer -= Time.deltaTime;
        ScaleInTimer -= Time.deltaTime;

        if (!WasMissed && !WasTapped)
        {
            if (MissedStopper >= TimeBeforeMissed)
            {
                WasMissed = true;
            }
            else
            {
                MissedStopper += Time.deltaTime;
            }
        }

        if (ToDestroy)
        {
            CalcNoteScore(-1);
            Destroy(gameObject);
            return;
        }

        if (WasTapped)
        {
            TappedRoutine();
        }
        else if (WasMissed)
        {
            FadeOutTimer -= Time.deltaTime;
            if (!GradualFade3(0, FadeOutTimer, GetComponent<SpriteRenderer>()))
            {
                ToDestroy = true;
            }
        }
        else if (IsFadingIn)
        {
            IsFadingIn = GradualFade3(1, FadeInTimer, GetComponent<SpriteRenderer>());
            IsScalingIn = !IsFadingIn;
        }
        if (IsScalingIn)
        {
            IsScalingIn = GradualGrow2(NoteProperties.DefaultSize, ScaleInTimer, transform);
            if (!IsScalingIn)
            {
                MainSpriteRenderer.color = Color.green;
            }
        }
    }

    //private bool GradualFade(float targetAlpha, float fadeRate, string tag)
    //{
    //    bool ans = true;
    //    SpriteRenderer tmp = spriteRenderers[0];
    //    int i = 1;
    //    while (!tmp.CompareTag(tag))
    //    {
    //        tmp = spriteRenderers[i++];
    //    }
    //    Color tmpCol = tmp.color;
    //    float alphaToAdd = fadeRate * Time.deltaTime;
    //    float newAlpha;
    //    if (tmpCol.a < targetAlpha)
    //    {
    //        newAlpha = tmpCol.a + alphaToAdd;
    //        if (newAlpha >= targetAlpha)
    //        {
    //            newAlpha = targetAlpha;
    //            ans = false;
    //        }
    //    }
    //    else
    //    {
    //        newAlpha = tmpCol.a - alphaToAdd;
    //        if (newAlpha <= targetAlpha)
    //        {
    //            newAlpha = targetAlpha;
    //            ans = false;
    //        }
    //    }
    //    SetAlpha(newAlpha, tag);
    //    return ans;
    //}


    //private bool GradualFade2(float targetAlpha, float timeLeft, string tag)
    //{
    //    bool ans = true;
    //    SpriteRenderer CurrSprite = spriteRenderers[0];
    //    int i = 1;
    //    while (!CurrSprite.CompareTag(tag))
    //    {
    //        CurrSprite = spriteRenderers[i++];
    //    }
    //    Color CurrCol = CurrSprite.color;
    //    bool FadeIn = CurrCol.a < targetAlpha;
    //    float fadeRate;
    //    if (timeLeft <= 0)
    //    {
    //        fadeRate = FadeIn ? 1 : -1;
    //    }
    //    else
    //    {
    //        fadeRate = (targetAlpha - CurrSprite.color.a) / timeLeft;
    //    }
    //    float alphaToAdd = fadeRate * Time.deltaTime;
    //    float newAlpha = CurrCol.a + alphaToAdd;
    //    if (FadeIn)
    //    {
    //        if (newAlpha >= targetAlpha)
    //        {
    //            newAlpha = targetAlpha;
    //            ans = false;
    //        }
    //    }
    //    else
    //    {
    //        if (newAlpha <= targetAlpha)
    //        {
    //            newAlpha = targetAlpha;
    //            ans = false;
    //        }
    //    }
    //    SetAlpha(newAlpha, tag);
    //    return ans;
    //}

    /// <summary>
    /// Slightly reduces or raise the alpha value of a given SpriteRenderer, depending on a given target alpha value.
    /// </summary>
    /// <param name="targetAlpha">Alpha value to aim for</param>
    /// <param name="timeLeft">Time left to change the alpha such that when it reaches 0 the alpha value will be targetAlpha</param>
    /// <param name="sr">SpriteRenderer to change alpha for</param>
    /// <returns>True if sr's alpha value reached targetAlpha, false otherwise.</returns>
    private bool GradualFade3(float targetAlpha, float timeLeft, SpriteRenderer sr)
    {
        bool ans = true;
        Color CurrCol = sr.color;
        bool FadeIn = CurrCol.a < targetAlpha;
        float fadeRate;
        if (timeLeft <= 0)
        {
            fadeRate = FadeIn ? 1 : -1;
        }
        else
        {
            fadeRate = (targetAlpha - CurrCol.a) / timeLeft;
        }
        float alphaToAdd = fadeRate * Time.deltaTime;
        float newAlpha = CurrCol.a + alphaToAdd;
        if (FadeIn)
        {
            if (newAlpha >= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = false;
            }
        }
        else
        {
            if (newAlpha <= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = false;
            }
        }
        SetAlpha(newAlpha, tag);
        return ans;
    }

    /// <summary>
    /// Slightly reduces or raise the scale of a given Transform, depending on a given scale rate.
    /// </summary>
    /// <param name="targetScale">The target value of the scale</param>
    /// <param name="scaleRate">Scale rate (value per second)</param>
    /// <param name="tr">Given Transform</param>
    /// <returns>>True if tr's scale value reached targetScale, false otherwise.</returns>
    private bool GradualGrow(float targetScale, float scaleRate, Transform tr)
    {
        bool ans = true;
        float scaleToAdd = targetScale * scaleRate * Time.deltaTime;
        float newScale = tr.localScale.x + scaleToAdd;
        if (newScale >= targetScale)
        {
            newScale = targetScale;
            ans = false;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }

    /// <summary>
    /// Slightly reduces or raise the scale value of a given Transform, depending on a given target scale value.
    /// </summary>
    /// <param name="targetScale">Scale value to aim for</param>
    /// <param name="timeLeft">Time left to change the scale such that when it reaches 0 the scale value will be targetScale</param>
    /// <param name="tr">Transform to change alpha for</param>
    /// <returns>True if sr's scale value reached targetScale, false otherwise.</returns>
    private bool GradualGrow2(float targetScale, float timeLeft, Transform tr)
    {
        bool ans = true;
        float currScale = tr.localScale.x;
        float scaleRate = timeLeft > 0 ? ((targetScale - currScale) / timeLeft) : targetScale;
        float scaleToAdd = scaleRate * Time.deltaTime;
        float newScale = currScale + scaleToAdd;
        if (newScale >= targetScale)
        {
            newScale = targetScale;
            ans = false;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }

    //private void GradualTappedDestroy()
    //{
    //    float maxScale = ScaleWhenTapped * NoteProperties.ScaleMultiplierWhenTapped;
    //    if (IsScalingOut)
    //    {
    //        IsScalingOut = GradualGrow2(maxScale, TappedScaleTimer, transform) ||
    //            GradualGrow2(NoteProperties.TappedInnerCircleGrowth,
    //                        TappedScaleTimer,
    //                        TransformsMap[NoteProperties.TappedNoteTag]); //TODO maybe remove map
    //    }
    //    else if (IsFadingOut)
    //    {
    //        IsFadingOut = GradualFade3(0, FadeOutTimer, MainSpriteRenderer) ||
    //            GradualFade3(0, FadeOutTimer, TappedSpriteRenderer);
    //    }
    //    else
    //    {
    //        ToDestroy = true;
    //    }
    //}

    /// <summary>
    /// Controls what happens when the note is considered "tapped", up until right before its destruction.
    /// </summary>
    void TappedRoutine()
    {
        if (ScaleWhenTapped == 0)
        {
            CalcNoteScore(Time.time);
            IsFadingIn = false;
            IsScalingIn = false;
            IsScalingOut = true;
            IsFadingOut = true;
            SetAlpha(NoteProperties.TappedInnerCircleAlpha, NoteProperties.TappedNoteTag);
            FadeOutTimer = NoteProperties.FadeOutTime;
            ScaleWhenTapped = transform.localScale.x;
        }

        float maxScale = ScaleWhenTapped * NoteProperties.ScaleMultiplierWhenTapped;
        if (IsScalingOut)
        {
            TappedScaleTimer -= Time.deltaTime;
            bool sc1 = GradualGrow2(maxScale, TappedScaleTimer, transform);
            var tmp = TransformsMap[NoteProperties.TappedNoteTag];
            bool sc2 = GradualGrow(NoteProperties.TappedInnerCircleGrowth,
                           NoteProperties.TappedAnimationScaleRate,
                           tmp);
            IsScalingOut = sc1 || sc2;
        }
        if (IsFadingOut)
        {
            FadeOutTimer -= Time.deltaTime;
            IsFadingOut = GradualFade3(0, FadeOutTimer, GetComponent<SpriteRenderer>());
        }
        else if (!IsScalingOut && !IsFadingOut)
        {
            ToDestroy = true;
        }
    }

    /// <summary>
    /// Calculates the score the player got from tapping on this note at a given time, and updates ScoreManager accordingly.
    /// </summary>
    /// <param name="TappedTime">Given time at which the player has tapped the note</param>
    void CalcNoteScore(float TappedTime)
    {
        if (scoreCalculated) return;
        scoreCalculated = true;
        if (TappedTime < 0)
        {
            ScoreManager.AddMiss();
            return;
        }
        float timeDiff = Mathf.Abs(TappedTime - TimeAtPerfect);
        if (timeDiff <= ScoreManager.PerfectThreshold)
        {
            ScoreManager.AddPerfect();
        }
        else if (timeDiff <= ScoreManager.GoodThreshold)
        {
            ScoreManager.AddGood();
        }
        else
        {
            ScoreManager.AddBad();
        }
    }

    /// <summary>
    /// Taps on this note
    /// </summary>
    public void Tap()
    {
        WasTapped = true;
        NoteCollider.enabled = false;
    }

    /// <summary>
    /// Activates this note
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);
        TimeAtActivation = Time.time;
    }
}
