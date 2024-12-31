using TMPro;
using UnityEngine;

/// <summary>
/// A script responsible for dynamically changing text elements in the HUD.
/// </summary>
public class HUDTextSetter : MonoBehaviour
{
    [SerializeField] ScoreManager ScoreManager;

    [SerializeField] TMP_Text StageName;
    [SerializeField] TMP_Text DiffLevel;

    [SerializeField] TMP_Text Miss;
    [SerializeField] TMP_Text Bad;
    [SerializeField] TMP_Text Good;
    [SerializeField] TMP_Text Perfect;

    void Start()
    {
        StageName.text = StageState.StageFileName.Substring(StageState.StageFileName.IndexOf('/') + 1);
        DiffLevel.text = "DiffLevel";
    }

    /// <summary>
    /// Updates the score values displayed in the stage summary menu.
    /// </summary>
    public void UpdateScoreTrackers()
    {
        Miss.text = ScoreManager.MissCount.ToString();
        Bad.text = ScoreManager.BadCount.ToString();
        Good.text = ScoreManager.GoodCount.ToString();
        Perfect.text = ScoreManager.PerfectCount.ToString();
    }
}
