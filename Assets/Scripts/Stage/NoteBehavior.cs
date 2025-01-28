using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script responsible for the behavior of the Notes, from functionalityto animation .
/// </summary>
public class NoteBehavior : MonoBehaviour
{
    [SerializeField] Collider2D NoteCollider;
    [SerializeField] SpriteRenderer MainSpriteRenderer;
    [SerializeField] SpriteRenderer TappedSpriteRenderer;
    [SerializeField] ParticleSystem Ring;
    [SerializeField] Color[] OuterRingColors;
    float[] durations;

    NoteProperties NoteP;
    ScoreManager ScoreM;

    /* Note Animation */
    bool WasTapped = false;
    bool WasMissed = false;
    bool IsFadingOut = true, IsScalingOut = false;
    bool IsFadingIn = true, IsScalingIn = false;
    bool ToDestroy = false;
    float ScaleWhenTapped = 0;
    readonly Dictionary<string, Transform> TransformsMap = new Dictionary<string, Transform>();
    SpriteRenderer[] spriteRenderers;
    float outerRingColorStopwatch = 0;
    int currentIndex = 0;

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
    enum Grade { None, Miss, Bad, Good, Perfect };
    static Dictionary<Grade, int> GradeColorDict = new() {
    { Grade.None, -1 },{ Grade.Miss, 0 },{ Grade.Bad, 1 },{ Grade.Good, 2 },{ Grade.Perfect, 3 }};

    /* Debug */
    [SerializeField] bool AutoTapNotesInEditor = false;
    public static float LastFullGrownTime = 0;

    void Start()
    {
        NoteP = NoteProperties.Instance;
        ScoreM = ScoreManager.Instance;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag(NoteP.TappedNoteTag))
            {
                TransformsMap.Add(NoteP.TappedNoteTag, child);
            }
        }
        SetInitialScale();
        SetAlpha(0, NoteP.MainNoteTag);
        SetTimers();
        SetOuterRingDurations();
    }

    void SetOuterRingDurations()
    {
        var timeTillPerfect1 = NoteP.FadeInTime + NoteP.ScaleInTime - ScoreM.PerfectThreshold;
        var timeTillPerfect2 = NoteP.FadeInTime + NoteP.ScaleInTime + ScoreM.PerfectThreshold;
        var timeTillGood1 = timeTillPerfect1 - ScoreM.GoodThreshold;
        var timeTillGood2 = timeTillPerfect2 + ScoreM.GoodThreshold;

        durations = new float[] {
            timeTillGood1/2,
            timeTillGood1,
            timeTillPerfect1,
            timeTillGood2,
            (timeTillGood2 + NoteP.timeTillDestruction)/2,
            NoteP.timeTillDestruction };
    }

    void Update()
    {
        if (ToDestroy)
        {
            CalcNoteGrade(-1);
            Destroy(gameObject);
            return;
        }

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

        if (WasTapped)
        {
            TappedRoutine();
            return;
        }
        if (WasMissed)
        {
            FadeOutTimer -= Time.deltaTime;
            bool FinishedFadingOut = GradualFade3(0, FadeOutTimer, GetComponent<SpriteRenderer>());
            if (FinishedFadingOut)
            {
                ToDestroy = true;
            }
            return;
        }
        if (IsFadingIn)
        {
            bool FinishedFadingIn = GradualFade3(1, FadeInTimer, GetComponent<SpriteRenderer>());
            IsFadingIn = !FinishedFadingIn;
            IsScalingIn = FinishedFadingIn;
        }
        if (IsScalingIn)
        {
            bool FinishedScalingIn = GradualGrow2(NoteP.DefaultSize, ScaleInTimer, transform);
            IsScalingIn = !FinishedScalingIn;
            if (FinishedScalingIn)
            {
                MainSpriteRenderer.color = Color.green;
                //if (StageState.AutoTapNotesInEditor && Application.isEditor)
                if (AutoTapNotesInEditor && Application.isEditor)
                {
                    WasTapped = true;
                }
            }
        }
        ColorOuterRing();
    }

    void ColorOuterRing()
    {
        while (outerRingColorStopwatch > durations[currentIndex])
        {
            if (currentIndex >= durations.Length)
            {
                return;
            }
            ++currentIndex;
        }
        float normalizedTime =
            (1 - (durations[currentIndex] - outerRingColorStopwatch)) / durations[currentIndex];
        GetComponent<SpriteRenderer>().color =
            Color.Lerp(
                OuterRingColors[currentIndex],
                OuterRingColors[Mathf.Min(currentIndex + 1, durations.Length - 1)],
                normalizedTime);

        outerRingColorStopwatch += Time.deltaTime;
    }


    /// <summary>
    /// Sets the Note initial scale (usually smaller than normal).
    /// </summary>
    void SetInitialScale()
    {
        float initialScale = NoteP.DefaultSize * NoteP.SpawnScaleMultiplier;
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
        FadeInTimer = NoteP.FadeInTime;
        ScaleInTimer = NoteP.ScaleInTime + NoteP.FadeInTime;
        //MainLifeTimer = NP.MainLifeTime;
        TappedScaleTimer = NoteP.TappedScaleTime;
        FadeOutTimer = NoteP.FadeOutTime;

        TimeBeforeMissed = NoteP.MainLifeTime + NoteP.FadeInTime + NoteP.ScaleInTime;
        TimeAtPerfect = TimeAtActivation + NoteP.ScaleInTime + NoteP.FadeInTime;

        if (TimeAtActivation > 0)
        {
            float tmp = (Time.time - TimeAtActivation);
            FadeInTimer -= tmp;
            ScaleInTimer -= tmp;
            TimeAtActivation = 0;
        }
    }

    /// <summary>
    /// Slightly reduces or raises the alpha value of a given SpriteRenderer, depending on a given target alpha value.
    /// </summary>
    /// <param name="targetAlpha">Alpha value to aim for</param>
    /// <param name="timeLeft">When this reaches 0 the alpha value will be targetAlpha</param>
    /// <param name="sr">SpriteRenderer to change alpha for</param>
    /// <returns>True if sr's alpha value reached targetAlpha, false otherwise.</returns>
    private bool GradualFade3(float targetAlpha, float timeLeft, SpriteRenderer sr)
    {
        bool ans = false;
        Color CurrCol = sr.color;
        bool FadeIn = CurrCol.a < targetAlpha;
        float alphaToAdd;
        if (timeLeft <= 0)
        {
            alphaToAdd = FadeIn ? 1 : -1;
        }
        else
        {
            float fadeRate = (targetAlpha - CurrCol.a) / timeLeft;
            alphaToAdd = fadeRate * Time.deltaTime;
        }
        //float alphaToAdd = fadeRate * Time.deltaTime;
        float newAlpha = CurrCol.a + alphaToAdd;
        if (FadeIn)
        {
            if (newAlpha >= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = true;
            }
        }
        else
        {
            if (newAlpha <= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = true;
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
        bool ans = false;
        float scaleToAdd = targetScale * scaleRate * Time.deltaTime;
        float newScale = tr.localScale.x + scaleToAdd;
        if (newScale >= targetScale)
        {
            newScale = targetScale;
            ans = true;
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
        bool ans = false;
        float currScale = tr.localScale.x;
        float scaleRate = timeLeft > 0 ? ((targetScale - currScale) / timeLeft) : targetScale;
        float scaleToAdd = scaleRate * Time.deltaTime;
        float newScale = currScale + scaleToAdd;
        if (newScale >= targetScale)
        {
            newScale = targetScale;
            ans = true;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }

    /// <summary>
    /// Controls what happens when the note is considered "tapped", up until right before its destruction.
    /// </summary>
    void TappedRoutine()
    {
        if (ScaleWhenTapped == 0)
        {
            var grade = CalcNoteGrade(Time.time);
            IsFadingIn = false;
            IsScalingIn = false;
            IsScalingOut = true;
            IsFadingOut = true;
            SetAlpha(NoteP.TappedInnerCircleAlpha, NoteP.TappedNoteTag);
            FadeOutTimer = NoteP.FadeOutTime;
            ScaleWhenTapped = transform.localScale.x;

            if (GradeColorDict[grade] > 0)
            {
                var tmp = Ring.main;
                //int nextIndex = Mathf.Min(currentIndex + 1, durations.Length - 1);
                //var v1 = Mathf.Abs(durations[currentIndex] - outerRingColorStopwatch);
                //var v2 = Mathf.Abs(durations[nextIndex] - outerRingColorStopwatch);
                //if (v1 < v2)
                //{
                //    tmp.startColor = OuterRingColors[currentIndex];
                //}
                //else
                //{
                //    tmp.startColor = OuterRingColors[nextIndex];
                //}
                tmp.startColor = OuterRingColors[GradeColorDict[grade]];
                Ring.Play();
            }
        }

        float maxScale = ScaleWhenTapped * NoteP.ScaleMultiplierWhenTapped;
        if (IsScalingOut)
        {
            TappedScaleTimer -= Time.deltaTime;
            bool FinishedScalingOut1 = GradualGrow2(maxScale, TappedScaleTimer, transform);
            var tmp = TransformsMap[NoteP.TappedNoteTag];
            bool FinishedScalingOut2 = GradualGrow(NoteP.TappedInnerCircleGrowth,
                           NoteP.TappedAnimationScaleRate,
                           tmp);
            IsScalingOut = !FinishedScalingOut1 || !FinishedScalingOut2;
        }
        if (IsFadingOut)
        {
            FadeOutTimer -= Time.deltaTime;
            bool FinishedFadingOut = GradualFade3(0, FadeOutTimer, GetComponent<SpriteRenderer>());
            IsFadingOut = !FinishedFadingOut;
        }
        if (!IsScalingOut && !IsFadingOut)
        {
            ToDestroy = true;
        }
    }

    /// <summary>
    /// Calculates the grade the player got from tapping on this note at a given time, 
    /// and updates ScoreManager accordingly.
    /// </summary>
    /// <param name="TappedTime">Given time at which the player has tapped the note</param>
    Grade CalcNoteGrade(float TappedTime)
    {
        if (scoreCalculated) return Grade.None;
        scoreCalculated = true;
        if (TappedTime < 0)
        {
            ScoreM.AddMiss();
            return Grade.Miss;
        }
        float timeDiff = Mathf.Abs(TappedTime - TimeAtPerfect);
        if (timeDiff <= ScoreM.PerfectThreshold)
        {
            ScoreM.AddPerfect();
            return Grade.Perfect;
        }
        if (timeDiff <= ScoreM.GoodThreshold)
        {
            ScoreM.AddGood();
            return Grade.Good;
        }
        ScoreM.AddBad();
        return Grade.Bad;
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
