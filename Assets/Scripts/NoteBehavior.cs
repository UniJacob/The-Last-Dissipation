using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    [SerializeField] private float FadeRate, ScaleRate;
    [SerializeField] private float DefaultScale;
    [SerializeField] private float ScaleMultiplierWhenTapped, ScaleRateMultiplierWhenTapped;
    [SerializeField] private string MainNoteTag, TappedNoteTag;

    private bool IsFading = true, IsGrowing = false, ToDestroy = false;
    public bool WasTapped = false;
    private float ScaleWhenTapped = 0;
    private List<Transform> MainNoteGO = new List<Transform>(), TappedGO = new List<Transform>();
    private Dictionary<string, List<Transform>> TransformsMap = new Dictionary<string, List<Transform>>();

    void Start()
    {
        SetAlpha(0, MainNoteTag);
        SetAlpha(0, TappedNoteTag);
        SetScale(DefaultScale);

        ScaleRate /= 100;
        FadeRate /= 100;

        MainNoteGO.Add(transform);
        foreach (Transform child in transform)
        {
            if (child.CompareTag(TappedNoteTag))
            {
                TappedGO.Add(child);
            }
            else if (child.CompareTag(MainNoteTag))
            {
                MainNoteGO.Add(child);
            }
        }
        TransformsMap.Add(MainNoteTag, MainNoteGO);
        TransformsMap.Add(TappedNoteTag, TappedGO);
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
                SetAlpha(1, TappedNoteTag);
            }
            GradualDestroy();
        }
        else if (IsFading)
        {
            GradualFade(1, MainNoteTag);
        }
        else if (IsGrowing)
        {
            IsGrowing = GradualGrow(1, 1);
        }
    }

    private void SetAlpha(float a, string tag)
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>()
            .Where(sr => sr.gameObject.CompareTag(tag)).ToArray();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Color color = sr.color;
            color.a = a;
            sr.color = color;
        }
    }

    private void GradualFade(float fadeMultiplier, string tag)
    {
        Color newCol = GetComponent<SpriteRenderer>().color;
        float alphaToAdd = FadeRate * fadeMultiplier * Time.deltaTime;
        float newAlpha = newCol.a + alphaToAdd;
        if (newAlpha >= 1)
        {
            newAlpha = 1;
            IsFading = false;
            IsGrowing = true;
        }
        SetAlpha(newAlpha, tag);
    }

    private void SetScale(float s)
    {
        transform.localScale = new Vector3(s, s, 1);
    }
    private void SetScale(float s, string tag)
    {
        if (tag.Equals(""))
        {
            SetScale(s);
            return;
        }
        foreach (Transform taggedTransform in TransformsMap[tag])
        {
            Debug.Log(taggedTransform.name);
            taggedTransform.localScale = new Vector3(s, s, 1);
        }
    }

    private bool GradualGrow(float maxScale, float scaleRateMultiplier)
    {
        bool ans = true;
        float scaleToAdd = ScaleRate * scaleRateMultiplier * Time.deltaTime;
        float newScale = transform.localScale.x + scaleToAdd;

        if (newScale >= maxScale)
        {
            newScale = maxScale;
            //IsGrowing = false;
            ans = false;
        }
        SetScale(newScale);
        return ans;
    }

    private bool GradualGrow(float maxScale, float scaleRateMultiplier, string tag)
    {
        bool ans = true;
        float scaleToAdd = ScaleRate * scaleRateMultiplier * Time.deltaTime;
        foreach (Transform taggedTransform in TransformsMap[tag])
        {
            float newScale = taggedTransform.localScale.x + scaleToAdd;
            if (newScale >= maxScale)
            {
                newScale = maxScale;
                //IsGrowing = false;
                ans = false;
            }
            taggedTransform.localScale = new Vector3(newScale, newScale, 1);
        }
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

        IsGrowing = GradualGrow(maxScale, ScaleRateMultiplierWhenTapped) ||
            GradualGrow(ScaleWhenTapped * 0.85f, ScaleRateMultiplierWhenTapped * 3, TappedNoteTag);
        if (!IsGrowing)
        {
            ToDestroy = true;
        }
    }
}
