using System.Collections.Generic;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private static NoteProperties NP;

    /* Note Animation */
    [HideInInspector] public bool WasTapped = false, WasMissed = false;
    private bool IsFadingOut = true, IsScalingOut = false;
    private bool IsFadingIn = true, IsScalingIn = false;
    private bool ToDestroy = false;
    private float ScaleWhenTapped = 0;
    private readonly List<Transform> MainNoteGO = new List<Transform>();
    private readonly List<Transform> TappedGO = new List<Transform>();
    private readonly Dictionary<string, List<Transform>> TransformsMap = new Dictionary<string, List<Transform>>();
    private SpriteRenderer[] spriteRenderers;

    /* Note Lifetime Timers */
    [HideInInspector] public float TimeAtActivation;
    private float timer = 0;
    private float FadeInTimer;
    private float ScaleInTimer;
    //private float MainLifeTimer;
    private float TappedScaleTimer;
    private float FadeOutTimer;

    /* Debug */
    public static float LastFullGrownTime = 0;

    void Awake()
    {
        if (NP == null)
        {
            NP = StageManager.GetComponent<NoteProperties>();
        }
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (Transform child in transform)
        {
            if (child.CompareTag(NP.TappedNoteTag))
            {
                TappedGO.Add(child);
            }
        }
        TransformsMap.Add(NP.TappedNoteTag, TappedGO);

        SetAlpha(0, NP.MainNoteTag);
        SetAlpha(0, NP.TappedNoteTag);
        float initialScale = NP.DefaultSize * NP.SpawnScaleMultiplier;
        transform.localScale = new(initialScale, initialScale, 1);

        SetTimers();
    }

    private void SetTimers()
    {
        FadeInTimer = NP.FadeInTime;
        ScaleInTimer = NP.ScaleInTime + FadeInTimer;
        //MainLifeTimer = NP.MainLifeTime;
        TappedScaleTimer = NP.TappedScaleTime;
        FadeOutTimer = NP.FadeOutTime;
    }

    void Update()
    {
        if (TimeAtActivation > 0)
        {
            float tmp = (Time.time - TimeAtActivation);
            FadeInTimer -= tmp;
            ScaleInTimer -= tmp;
            TimeAtActivation = 0;
        }
        FadeInTimer -= Time.deltaTime;
        ScaleInTimer -= Time.deltaTime;

        if (!WasMissed && !WasTapped)
        {
            if (timer >= NP.MainLifeTime)
            {
                WasMissed = true;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        if (ToDestroy)
        {
            Destroy(gameObject);
            return;
        }

        if (WasTapped)
        {
            //GradualTappedDestroy();
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
            IsScalingIn = GradualGrow2(NP.DefaultSize, ScaleInTimer, transform);
            if (!IsScalingIn)
            {
                spriteRenderers[0].color = Color.green;
            }
        }
    }
    private void SetAlpha(float a, string tag)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr.CompareTag(tag))
            {
                Color col = sr.color;
                col.a = a;
                sr.color = col;
            }
        }
    }
    private bool GradualFade(float targetAlpha, float fadeRate, string tag)
    {
        bool ans = true;
        SpriteRenderer tmp = spriteRenderers[0];
        int i = 1;
        while (!tmp.CompareTag(tag))
        {
            tmp = spriteRenderers[i++];
        }
        Color tmpCol = tmp.color;
        float alphaToAdd = fadeRate * Time.deltaTime;
        float newAlpha;
        if (tmpCol.a < targetAlpha)
        {
            newAlpha = tmpCol.a + alphaToAdd;
            if (newAlpha >= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = false;
            }
        }
        else
        {
            newAlpha = tmpCol.a - alphaToAdd;
            if (newAlpha <= targetAlpha)
            {
                newAlpha = targetAlpha;
                ans = false;
            }
        }
        SetAlpha(newAlpha, tag);
        return ans;
    }
    private bool GradualFade2(float targetAlpha, float timeLeft, string tag)
    {
        bool ans = true;
        SpriteRenderer CurrSprite = spriteRenderers[0];
        int i = 1;
        while (!CurrSprite.CompareTag(tag))
        {
            CurrSprite = spriteRenderers[i++];
        }
        Color CurrCol = CurrSprite.color;
        bool FadeIn = CurrCol.a < targetAlpha;
        float fadeRate;
        if (timeLeft <= 0)
        {
            fadeRate = FadeIn ? 1 : -1;
        }
        else
        {
            fadeRate = (targetAlpha - CurrSprite.color.a) / timeLeft;
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
    private bool GradualGrow(float maxScale, float scaleRate, Transform tr)
    {
        bool ans = true;
        float scaleToAdd = maxScale * scaleRate * Time.deltaTime;
        float newScale = tr.localScale.x + scaleToAdd;
        if (newScale >= maxScale)
        {
            newScale = maxScale;
            ans = false;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }
    /// <summary>
    /// Gradual scales a game object given its Transform.
    /// </summary>
    /// <param name="maxScale"></param>
    /// <param name="timeLeft"></param>
    /// <param name="tr"></param>
    /// <returns> True if the object is still scaling, false if the object 
    /// reached its desired scale. </returns>
    private bool GradualGrow2(float maxScale, float timeLeft, Transform tr)
    {
        bool ans = true;
        float currScale = tr.localScale.x;
        float scaleRate = timeLeft > 0 ? ((maxScale - currScale) / timeLeft) : maxScale;
        float scaleToAdd = scaleRate * Time.deltaTime;
        float newScale = currScale + scaleToAdd;
        if (newScale >= maxScale)
        {
            newScale = maxScale;
            ans = false;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }
    private void GradualTappedDestroy()
    {
        float maxScale = ScaleWhenTapped * NP.ScaleMultiplierWhenTapped;
        if (IsScalingOut)
        {
            IsScalingOut = GradualGrow2(maxScale, TappedScaleTimer, transform) ||
                GradualGrow2(NP.TappedInnerCircleGrowth,
                            TappedScaleTimer,
                            TransformsMap[NP.TappedNoteTag][0]); //TODO maybe remove map
        }
        else if (IsFadingOut)
        {
            IsFadingOut = GradualFade2(0, FadeOutTimer, NP.MainNoteTag) ||
                GradualFade2(0, FadeOutTimer, NP.TappedNoteTag);
        }
        else
        {
            ToDestroy = true;
        }
    }

    void TappedRoutine()
    {
        if (ScaleWhenTapped == 0)
        {
            IsFadingIn = false;
            IsScalingIn = false;
            IsScalingOut = true;
            IsFadingOut = true;
            SetAlpha(1, NP.TappedNoteTag);
            FadeOutTimer = NP.FadeOutTime;
            ScaleWhenTapped = transform.localScale.x;
        }

        float maxScale = ScaleWhenTapped * NP.ScaleMultiplierWhenTapped;
        if (IsScalingOut)
        {
            TappedScaleTimer -= Time.deltaTime;
            bool sc1 = GradualGrow2(maxScale, TappedScaleTimer, transform);
            //bool sc1 = GradualGrow(maxScale, NP.TappedAnimationScaleRate, transform);
            var tmp = TransformsMap[NP.TappedNoteTag][0];
            bool sc2 = GradualGrow(NP.TappedInnerCircleGrowth,
                           NP.TappedAnimationScaleRate,
                           tmp); //TODO maybe remove map
            //bool sc2 = GradualGrow2(NP.TappedInnerCircleGrowth,
            //               TappedScaleTimer,
            //               tmp); //TODO maybe remove map
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
}
