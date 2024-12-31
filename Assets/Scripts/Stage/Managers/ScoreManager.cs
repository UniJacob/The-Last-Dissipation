using UnityEngine;

/// <summary>
/// A singleton script responsible for handling the score of the stage.
/// Function are self-explanatory.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    /// <summary>
    /// The maximum time difference between the moment the player tapped a note to the moment it's supposed to be "perfect"
    /// so that the score is considered "good".
    /// </summary>
    public float GoodThreshold = 0.15f;

    /// <summary>
    /// The maximum time difference between the moment the player tapped a note to the moment it's supposed to be "perfect"
    /// so that the score is still considered "perfect".
    /// </summary>
    public float PerfectThreshold = 0.05f;

    public int MissCount { get; private set; }
    public int BadCount { get; private set; }
    public int GoodCount { get; private set; }
    public int PerfectCount { get; private set; }

    static ScoreManager instance;

    /* Debug */
    [Tooltip("This score-array is for view-only")]
    [SerializeField] int[] InspectorScoreArray;

    void Awake()
    {
        Auxiliary.AssureSingleton(ref instance, gameObject);
    }

    void Start()
    {
        MissCount = 0;
        BadCount = 0;
        GoodCount = 0;
        PerfectCount = 0;
        InspectorScoreArray = new int[4];
    }

    public void AddMiss()
    {
        ++MissCount;
        InspectorScoreArray[0] = MissCount;
    }

    public void AddBad()
    {
        ++BadCount;
        InspectorScoreArray[1] = BadCount;
    }

    public void AddGood()
    {
        ++GoodCount;
        InspectorScoreArray[2] = GoodCount;
    }

    public void AddPerfect()
    {
        ++PerfectCount;
        InspectorScoreArray[3] = PerfectCount;
    }
    public void Restart()
    {
        Start();
    }
}
