using UnityEngine;

public class NoteProperties : MonoBehaviour
{
    [SerializeField] public float DefaultSize = 2000, SpawnScaleMultiplier = 0.5f;
    [SerializeField] public float ScaleMultiplierWhenTapped = 1.2f;
    [SerializeField] public string MainNoteTag = "Main Note", TappedNoteTag = "Tapped Note";

    [HideInInspector] public float FadeInRate, FadeOutRate;
    [HideInInspector] public float ScaleInRate, TappedScaleRate, TappedAnimationScaleRate = 10;
    [HideInInspector] public float FadeInTime, ScaleInTime, LifeTime;

    void Start()
    {
        //FadeInTime = 1 / FadeInRate;
        //float scaleRate = DefaultSize * ScaleInRate;
        //ScaleInTime = SpawnScaleMultiplier * DefaultSize / scaleRate;
    }
}
