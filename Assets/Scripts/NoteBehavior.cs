using System.Collections.Generic;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private NoteProperties NP;

    [HideInInspector] public bool WasTapped = false, WasMissed = false;
    private float timer = 0;

    private bool IsFading = true, IsGrowing = false, ToDestroy = false;
    private float ScaleWhenTapped = 0;
    private List<Transform> MainNoteGO = new List<Transform>(), TappedGO = new List<Transform>();
    private Dictionary<string, List<Transform>> TransformsMap = new Dictionary<string, List<Transform>>();
    private SpriteRenderer[] spriteRenderers;
    void Start()
    {
        NP = StageManager.GetComponent<NoteProperties>();
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
        float firstScale = NP.DefaultSize * NP.SpawnScaleMultiplier;
        transform.localScale = new(firstScale, firstScale, 1);
    }

    void Update()
    {
        if (!(WasMissed || WasTapped))
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
        else if (WasTapped)
        {
            if (ScaleWhenTapped == 0)
            {
                ScaleWhenTapped = transform.localScale.x;
                IsGrowing = true;
                IsFading = true;
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
        else if (IsFading)
        {
            IsFading = GradualFade(1, NP.FadeInRate, NP.MainNoteTag);
            IsGrowing = !IsFading;
        }
        else if (IsGrowing)
        {
            IsGrowing = GradualGrow(NP.DefaultSize, NP.ScaleInRate, transform);
            if (!IsGrowing)
            {
                spriteRenderers[0].color = Color.yellow;
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

    private void GradualDestroy()
    {
        float maxScale = ScaleWhenTapped * NP.ScaleMultiplierWhenTapped;
        if (IsGrowing)
        {
            //float tmpScaleRate = NP.ScaleInRate * NP.ScaleRateMultiplierWhenTapped;
            IsGrowing = GradualGrow(maxScale, NP.TappedScaleRate, transform) ||
                GradualGrow(0.76f, NP.TappedAnimationScaleRate, TransformsMap[NP.TappedNoteTag][0]); //TODO remove magic numbers and maybe remove map
        }
        else if (IsFading)
        {
            //float tmpFadeRate = NP.FadeInRate * NP.FadeRateMultiplierWhenTapped;
            IsFading = GradualFade(0, NP.FadeOutRate, NP.MainNoteTag) ||
                GradualFade(0, NP.FadeOutRate, NP.TappedNoteTag);
        }
        else
        {
            ToDestroy = true;
        }
    }
}
