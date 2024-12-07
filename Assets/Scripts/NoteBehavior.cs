using System.Collections.Generic;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private static NoteProperties NP;

    [HideInInspector] public bool WasTapped = false, WasMissed = false;
    private float timer = 0;

    private bool IsFadingOut = true, IsScalingOut = false, ToDestroy = false;
    private bool IsFadingIn = true, IsScalingIn = false;
    private float ScaleWhenTapped = 0;
    private List<Transform> MainNoteGO = new List<Transform>(), TappedGO = new List<Transform>();
    private Dictionary<string, List<Transform>> TransformsMap = new Dictionary<string, List<Transform>>();
    private SpriteRenderer[] spriteRenderers;

    [SerializeField] GameObject Bar;
    private BarBehavior BB;

    private float TimeLeftToFadeIn, TimeLeftToScaleIn;
    [HideInInspector] public float TimeAtActivation;
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

        TimeLeftToFadeIn = NP.FadeInTime;
        TimeLeftToScaleIn = NP.ScaleInTime + TimeLeftToFadeIn;

        BB = Bar.GetComponent<BarBehavior>();
    }

    private void Start()
    {
    }

    void Update()
    {
        if (TimeAtActivation > 0)
        {
            float tmp = (Time.time - TimeAtActivation);
            TimeLeftToFadeIn -= tmp;
            TimeLeftToScaleIn -= tmp;
            TimeAtActivation = 0;
        }
        TimeLeftToFadeIn -= Time.deltaTime;
        TimeLeftToScaleIn -= Time.deltaTime;

        if (!WasMissed && !WasTapped)
        {
            if (timer >= NP.LifeTime)
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
            if (ScaleWhenTapped == 0)
            {
                ScaleWhenTapped = transform.localScale.x;
                IsFadingIn = false;
                IsScalingIn = false;
                IsScalingOut = true;
                IsFadingOut = true;
                SetAlpha(1, NP.TappedNoteTag);
            }
            GradualDestroy();
        }
        else if (WasMissed)
        {
            if (!GradualFade(0, NP.FadeOutRate, NP.MainNoteTag))
            {
                ToDestroy = true;
            }
        }
        else if (IsFadingIn)
        {

            IsFadingIn = GradualFade2(1, TimeLeftToFadeIn, NP.MainNoteTag);
            IsScalingIn = !IsFadingIn;
        }

        if (IsScalingIn)
        {
            IsScalingIn = GradualGrow2(NP.DefaultSize, TimeLeftToScaleIn, transform);
            if (!IsScalingIn)
            {
                spriteRenderers[0].color = Color.yellow;

                //float tmp = Time.time;
                //Debug.LogError(tmp - LastFullGrownTime);
                //LastFullGrownTime = tmp;

                //float barY = Bar.transform.position.y;
                //float barUPS = BB.barUPS;
                //float yDiff = transform.position.y - barY;
                //if (BB.up)
                //{
                //    yDiff = -yDiff;
                //}
                //float timeDelay = yDiff / barUPS;
                //Debug.LogError("yDiff " + yDiff + ", timeDelay " + timeDelay + ", transform.position.y " + transform.position.y);
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
        SpriteRenderer tmp = spriteRenderers[0];
        int i = 1;
        while (!tmp.CompareTag(tag))
        {
            tmp = spriteRenderers[i++];
        }
        Color tmpCol = tmp.color;
        float fadeRate = timeLeft > 0 ? ((targetAlpha - tmp.color.a)) / timeLeft : targetAlpha;
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
    private void GradualDestroy()
    {
        float maxScale = ScaleWhenTapped * NP.ScaleMultiplierWhenTapped;
        if (IsScalingOut)
        {
            //float tmpScaleRate = NP.ScaleInRate * NP.ScaleRateMultiplierWhenTapped;
            IsScalingOut = GradualGrow(maxScale, NP.TappedScaleRate, transform) ||
                GradualGrow(0.76f, NP.TappedAnimationScaleRate, TransformsMap[NP.TappedNoteTag][0]); //TODO remove magic numbers and maybe remove map
        }
        else if (IsFadingOut)
        {
            //float tmpFadeRate = NP.FadeInRate * NP.FadeRateMultiplierWhenTapped;
            IsFadingOut = GradualFade(0, NP.FadeOutRate, NP.MainNoteTag) ||
                GradualFade(0, NP.FadeOutRate, NP.TappedNoteTag);
        }
        else
        {
            ToDestroy = true;
        }
    }
}
