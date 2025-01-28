using UnityEngine;

/// <summary>
/// A singleton script responsible for handling the score of the stage.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    static ScoreManager instance;
    public static ScoreManager Instance {  get { return instance; } }

    [Tooltip("The maximum time difference between the moment the player tapped a note to the moment it's " +
        "supposed to be \"perfect\" so that the score is still considered \"good\".")]
    [Range(0f, 1f)]
    public float GoodThreshold = 0.15f;

    [Tooltip("The maximum time difference between the moment the player tapped a note to the moment it's " +
        "supposed to be \"perfect\" so that the score is still considered \"perfect\".")]
    [Range(0f, 1f)]
    public float PerfectThreshold = 0.05f;

    public int MissCount { get; private set; }
    public int BadCount { get; private set; }
    public int GoodCount { get; private set; }
    public int PerfectCount { get; private set; }

    public float MaxScore = 1000;

    [Tooltip("If all Note tapping will be 'bad', the final score will be MaxScore * AllBadScoreRatio.")]
    [Range(0f, 1f)]
    [SerializeField] float AllBadScoreRatio = 0.3f;

    [Tooltip("If all Note tapping will be 'good', the final score will be MaxScore * AllGoodScoreRatio.")]
    [Range(0f, 1f)]
    [SerializeField] float AllGoodScoreRatio = 0.8f;
    float finalScore;

    /* Debug */
    [Tooltip("This score-array is for view-only")]
    [SerializeField] int[] InspectorGradesArray;

    public void Awake()
    {
        Auxiliary.EnsureSingleton(ref instance, gameObject);
    }

    void Start()
    {
        MissCount = 0;
        BadCount = 0;
        GoodCount = 0;
        PerfectCount = 0;
        InspectorGradesArray = new int[4];
        finalScore = -1;
    }

    public void AddMiss()
    {
        ++MissCount;
        InspectorGradesArray[0] = MissCount;
    }

    public void AddBad()
    {
        ++BadCount;
        InspectorGradesArray[1] = BadCount;
    }

    public void AddGood()
    {
        ++GoodCount;
        InspectorGradesArray[2] = GoodCount;
    }

    public void AddPerfect()
    {
        ++PerfectCount;
        InspectorGradesArray[3] = PerfectCount;
    }

    public void Restart()
    {
        Start();
    }

    public float GetFinalScore()
    {
        if (finalScore < 0)
        {
            int totalNotes = MissCount + BadCount + GoodCount + PerfectCount;

            float scorePerfect = PerfectCount;
            float scoreGood = AllGoodScoreRatio * GoodCount;
            float scoreBad = AllBadScoreRatio * BadCount;

            finalScore = MaxScore * (scorePerfect + scoreGood + scoreBad) / totalNotes;
        }
        return finalScore;
    }
}
