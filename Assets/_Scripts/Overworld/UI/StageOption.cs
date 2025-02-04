using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script responsible for Stage Option (UI) behavior. 
/// A Stage Option GameObject should have an Image component and a Button component.
/// It should also have a Text component in one of its children.
/// </summary>
public class StageOption : MonoBehaviour
{
    [SerializeField] StageSelector StageSelector;
    [SerializeField] TMP_Text StageName;
    [SerializeField] TMP_Text StageDifficulty;
    [SerializeField] TMP_Text Highscore;
    [SerializeField] int StageSpeedCoefficient = 1;

    string stageDifficulty = "";

    void Start()
    {
        //stageDifficulty = Auxiliary.GetValueForKey(StageName.text, GameState.DifficultiesTextAsset);
        stageDifficulty = GameState.DifficultiesMap[StageName.text];
        StageDifficulty.text = "Difficulty " + stageDifficulty;
        Highscore.text = "";
        if (GameState.HighscoresMap.ContainsKey(StageName.text))
        {
            Highscore.text = "Highscore - " +
                GameState.HighscoresMap[StageName.text].ToString("0.0");
        }
    }

    public void MoveToConfirmation()
    {
        var img = GetComponent<RawImage>();
        StageSelector.SetConfirmationMenu(StageName.text, stageDifficulty, img, StageSpeedCoefficient);
    }

    void OnValidate()
    {
        if (StageSpeedCoefficient <=0)
        {
            Debug.LogError("Stage Speed Coefficient must be a positive number!");
        }
    }
}
