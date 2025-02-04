using UnityEngine;

/// <summary>
/// A singleton script responsible for various properties of Note objects, for example timing and animation speed.
/// </summary>
public class NoteProperties : MonoBehaviour
{
    static NoteProperties instance;
    public static NoteProperties Instance { get { return instance; } }

    /// <summary>
    /// The biggest size/scale of Notes before they are tapped.
    /// </summary>
    public float NoteSize { get { return GameState.StageNoteSizeSettings; } }
    //public float DefaultSize = 0.54f;
    public float SpawnScaleMultiplier = 0.5f;
    public float ScaleMultiplierWhenTapped = 1.2f;
    public string MainNoteTag = "Main Note", TappedNoteTag = "Tapped Note";

    /* Portions of the life-time of a note (their sum can be more than 1) */
    [SerializeField] float FadeInPortion = 0.2f;
    [SerializeField] float ScaleInPortion = 0.2f;
    [SerializeField] float MainLifePortion = 0.7f;
    [SerializeField] float TappedScalePortion = 0.1f;
    [SerializeField] float FadeOutPortion = 0.1f;

    [HideInInspector] public float FadeInTime;
    [HideInInspector] public float ScaleInTime;
    [HideInInspector] public float MainLifeTime;
    [HideInInspector] public float TappedScaleTime;
    [HideInInspector] public float FadeOutTime;

    public float timeTillDestruction { get; private set; }

    public float TappedAnimationScaleRate = 10;
    public float TappedInnerCircleGrowth = 0.66f;
    public float TappedInnerCircleAlpha = 0.5f;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject)) return;
        //DefaultSize = GameState.StageNoteSizeSettings;
    }

    /// <summary>
    /// Adjust various note animation and timing properties by a given "seconds per beat" value of a track.
    /// </summary>
    /// <param name="SPB"> Seconds Per Beat </param> 
    public void SetPropertiesFromSPB(float SPB)
    {
        timeTillDestruction = SPB * 2; // Maximum time from Start() to Destroy() of notes

        ScaleInTime = ScaleInPortion * timeTillDestruction;
        FadeInTime = FadeInPortion * timeTillDestruction;
        MainLifeTime = MainLifePortion * timeTillDestruction;
        TappedScaleTime = TappedScalePortion * timeTillDestruction;
        FadeOutTime = FadeOutPortion * timeTillDestruction;
    }
}
