using System.Collections.Generic;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    [SerializeField] float NoteFadeRate, NoteRelativeScaleRate;
    [SerializeField] float DefaultNoteSize, SpawnScaleMultiplier;
    [SerializeField] float ScaleMultiplierWhenTapped, ScaleRateMultiplierWhenTapped, FadeRateMultiplierWhenTapped;
    [SerializeField] private string MainNoteTag, TappedNoteTag;

    [HideInInspector] public bool WasTapped = false, WasMissed = false;

    private bool IsFading = true, IsGrowing = false, ToDestroy = false;
    private float ScaleWhenTapped = 0;
    private List<Transform> MainNoteGO = new List<Transform>(), TappedGO = new List<Transform>();
    private Dictionary<string, List<Transform>> TransformsMap = new Dictionary<string, List<Transform>>();
    private SpriteRenderer[] spriteRenderers;

    private int debug = 1;

    void Start()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        NoteFadeRate /= 100;
        NoteRelativeScaleRate /= 100;

        //MainNoteGO.Add(transform);
        foreach (Transform child in transform)
        {
            if (child.CompareTag(TappedNoteTag))
            {
                TappedGO.Add(child);
            }
            //    else if (child.CompareTag(MainNoteTag))
            //    {
            //        MainNoteGO.Add(child);
            //    }
        }
        //TransformsMap.Add(MainNoteTag, MainNoteGO);
        TransformsMap.Add(TappedNoteTag, TappedGO);

        SetAlpha(0, MainNoteTag);
        SetAlpha(0, TappedNoteTag);
        float firstScale = DefaultNoteSize * SpawnScaleMultiplier;
        transform.localScale = new(firstScale, firstScale, 1);
    }

    void Update()
    {
        if (ToDestroy)
        {
            Destroy(gameObject);
            return;
        }
        else if (WasTapped)
        {
            if (ScaleWhenTapped == 0)
            {
                ScaleWhenTapped = transform.localScale.x;
                IsGrowing = true;
                IsFading = true;
                SetAlpha(1, TappedNoteTag);
            }
            GradualDestroy();
        }
        else if (WasMissed)
        {
            if (!GradualFade(0, NoteFadeRate * FadeRateMultiplierWhenTapped, MainNoteTag))
            {
                ToDestroy = true;
            }
        }
        else if (IsFading)
        {
            IsFading = GradualFade(1, NoteFadeRate, MainNoteTag);
            IsGrowing = !IsFading;
        }
        else if (IsGrowing)
        {
            IsGrowing = GradualGrow(DefaultNoteSize, NoteRelativeScaleRate, transform);
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

    //private void SetScale(float s)
    //{
    //    transform.localScale = new Vector3(s, s, 1);
    //}
    //private void SetScale(float s, string tag)
    //{
    //    //if (tag.Equals(""))
    //    //{
    //    //    SetScale(s);
    //    //    return;
    //    //}
    //    foreach (Transform taggedTransform in TransformsMap[tag])
    //    {
    //        taggedTransform.localScale = new Vector3(s, s, 1);
    //    }
    //}


    //private bool GradualGrow(float maxScale, float scaleRateMultiplier, string tag)
    //{
    //    bool ans = true;
    //    float scaleToAdd = DefaultScaleRate * scaleRateMultiplier * Time.deltaTime;
    //    foreach (Transform taggedTransform in TransformsMap[tag])
    //    {
    //        float newScale = taggedTransform.localScale.x + scaleToAdd;
    //        if (newScale >= maxScale)
    //        {
    //            newScale = maxScale;
    //            //IsGrowing = false;
    //            ans = false;
    //        }
    //        taggedTransform.localScale = new Vector3(newScale, newScale, 1);
    //    }
    //    return ans;
    //}
    private bool GradualGrow(float maxScale, float scaleRate, Transform tr)
    {
        bool ans = true;
        float scaleToAdd = maxScale * scaleRate * Time.deltaTime;
        float newScale = tr.localScale.x + scaleToAdd;
        if (newScale >= maxScale)
        {
            newScale = maxScale;
            //IsGrowing = false;
            ans = false;
        }
        tr.localScale = new Vector3(newScale, newScale, 1);

        return ans;
    }

    private void GradualDestroy()
    {
        float maxScale = ScaleWhenTapped * ScaleMultiplierWhenTapped;
        //if (IsGrowing)
        //{
        //    IsGrowing = GradualGrow(maxScale, ScaleRateMultiplierWhenTapped);
        //}
        //bool allDoneGrowing = GradualGrow(ScaleWhenTapped * 0.77f, ScaleRateMultiplierWhenTapped * 2, TappedNoteTag);
        //if (allDoneGrowing && !IsGrowing)
        //{
        //    ToDestroy = true;
        //}
        if (IsGrowing)
        {
            float tmpScaleRate = NoteRelativeScaleRate * ScaleRateMultiplierWhenTapped;
            IsGrowing = GradualGrow(maxScale, tmpScaleRate, transform) ||
                GradualGrow(0.76f, tmpScaleRate * 5, TransformsMap[TappedNoteTag][0]); //TODO change 0.76f, *5 and maybe remove map
        }
        else if (IsFading)
        {
            float tmpFadeRate = NoteFadeRate * FadeRateMultiplierWhenTapped;
            IsFading = GradualFade(0, tmpFadeRate, MainNoteTag) ||
                GradualFade(0, tmpFadeRate, TappedNoteTag);
        }
        else
        {
            ToDestroy = true;
        }
    }

    public void SetMissed()
    {
        WasMissed = true;
    }
}
