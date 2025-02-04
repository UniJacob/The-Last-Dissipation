using TMPro;
using UnityEngine;

/// <summary>
/// A singleton script responsible for handling stage menus and their buttons. 
/// Function are self-explanatory.
/// </summary>
public class MenuManagerStage : MonoBehaviour
{
    static MenuManagerStage instance;

    [SerializeField] StageManager StageManager;
    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject ConfirmationMenu;
    [SerializeField] TMP_Text ConfirmationMenuText;
    [SerializeField] GameObject StageSummary;
    [SerializeField] GameObject PauseButton;
    [SerializeField] GameObject RestartButtonSummary;
    [SerializeField] GameObject ExitButtonSummary;
    [SerializeField] CanvasFader AdjustDelayMenu;
    [SerializeField] TMP_InputField AdjustDelayInputField;
    [SerializeField] TMP_Text AdjustDelayStageFileNameText;
    [SerializeField] GameObject AdjustDelayModeButton;


    string confirmationMode = "";
    public const string restartMode = "restart";
    const string quitMode = "quit";
    bool ChangesMadeInAdjustDelayMenu = false;
    enum Menu { None, Pause, AdjustDelay, Summary };
    Menu currentMenu;
    Color tmpColor;
    string tmpString;
    float MusicDelayEpsilon = 0.005f;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
        {
            return;
        }
    }
    void Start()
    {
        PauseButton.SetActive(true);
        RestartButtonSummary.SetActive(false);
        ExitButtonSummary.SetActive(false);
        StageSummary.SetActive(false);
        ConfirmationMenu.SetActive(false);
        AdjustDelayMenu.gameObject.SetActive(false);
        if (StageState.InAdjustDelayMode)
        {
            AdjustDelayModeButton.SetActive(true);
            tmpColor = AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().color;
            tmpString = AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().text;
            AdjustDelayInputField.text = GameState.StageMusicDelay.ToString("0.000");
        }
        else
        {
            AdjustDelayModeButton.SetActive(false);
        }
        currentMenu = Menu.None;

        Resume(false);
    }

    public void Restart()
    {
        Start();
    }

    public void TogglePause()
    {
        if (currentMenu == Menu.AdjustDelay) return;
        if (StageState.IsPaused)
        {
            Resume(true);
        }
        else
        {
            Pause(true);
        }
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    /// <param name="showMenu">Whether to show the pause menu</param>
    public void Pause(bool showMenu)
    {
        if (StageState.IsPaused) return;

        StageState.IsPaused = true;
        if (showMenu && currentMenu == Menu.None)
        {
            PauseMenu.SetActive(true);
            currentMenu = Menu.Pause;
        }
        Time.timeScale = 0;
        if (MusicPlayer.enabled)
        {
            MusicPlayer.Pause();
        }
    }

    /// <summary>
    /// Resumes the stage.
    /// </summary>
    /// <param name="PlayMusic">Whether to resume music playback after resuming.</param>
    public void Resume(bool PlayMusic)
    {
        StageState.IsPaused = false;
        PauseMenu.SetActive(false);
        Time.timeScale = 1;
        if (PlayMusic && MusicPlayer.enabled)
        {
            MusicPlayer.Play();
        }
        currentMenu = Menu.None;
    }

    /// <summary>
    /// Confirms a menu action. A menu action could be restarting the stage or quitting it.
    /// </summary>
    public void Confirm()
    {
        if (confirmationMode == restartMode)
        {
            StageManager.Restart();
        }
        else if (confirmationMode == quitMode)
        {
            HideConfirmationMenu();
            Resume(false);
            StageManager.Quit();
        }
    }

    public void AskRestartStage()
    {
        ConfirmationMenu.SetActive(true);
        ConfirmationMenuText.text = "Are you sure you want to restart?";
        confirmationMode = restartMode;
        if (StageState.InAdjustDelayMode)
        {
            Confirm();
        }
    }

    public void AskQuitStage()
    {
        ConfirmationMenu.SetActive(true);
        ConfirmationMenuText.text = "Are you sure you want to quit? Progress won't be saved.";
        confirmationMode = quitMode;
        if (StageState.InAdjustDelayMode)
        {
            Confirm();
        }
    }

    public void ExitStage()
    {
        confirmationMode = quitMode;
        Confirm();
    }

    public void HideConfirmationMenu()
    {
        ConfirmationMenu.SetActive(false);
    }

    public void PartialEndStage()
    {
        PauseMenu.SetActive(false);
        PauseButton.SetActive(false);
        RestartButtonSummary.SetActive(true);
        ExitButtonSummary.SetActive(true);
        StageSummary.SetActive(true);
        currentMenu = Menu.Summary;
    }

    public void EndStage()
    {
        StageManager.EndStage();
    }

    public void ToggleAdjustDelayMenu()
    {
        if (!StageState.InAdjustDelayMode) return;
        if (currentMenu == Menu.Pause) return;

        if (AdjustDelayMenu.gameObject.activeSelf)
        {
            Resume(true);
            AdjustDelayMenu.TriggerFadeOut();
            currentMenu = Menu.None;
            if (ChangesMadeInAdjustDelayMenu)
            {
                ChangesMadeInAdjustDelayMenu = false;
                AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().color = tmpColor;
                AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().text = tmpString;
                StageManager.Restart(true);
            }
            return;
        }
        Pause(false);
        AdjustDelayMenu.gameObject.SetActive(true);
        currentMenu = Menu.AdjustDelay;
    }

    public void OnAdjustDelayInputValueChanged()
    {
        if (float.TryParse(AdjustDelayInputField.text, out float newDelay))
        {
            GameState.StageMusicDelay = newDelay;
            MusicDelayRelatedChangesMade();
        }
        else
        {
            AdjustDelayInputField.text = GameState.StageMusicDelay.ToString("0.000");
            Debug.LogWarning("Invalid input field value");
        }
    }

    public void OnAdjustDelayMenuStageFileChange()
    {
        StageState.StageFileName = AdjustDelayStageFileNameText.text;
        MusicDelayRelatedChangesMade();
    }

    void MusicDelayRelatedChangesMade()
    {
        ChangesMadeInAdjustDelayMenu = true;
        AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().color = Color.green;
        AdjustDelayModeButton.GetComponentInChildren<TMP_Text>().text = "Apply Changes & Restart";
    }

    public void ChangeMusicDelay(bool increase)
    {
        if (float.TryParse(AdjustDelayInputField.text, out float currentDelay))
        {
            if (increase)
            {
                currentDelay += MusicDelayEpsilon;
            }
            else
            {
                currentDelay -= MusicDelayEpsilon;
            }
            AdjustDelayInputField.text = currentDelay.ToString("0.000");
            OnAdjustDelayInputValueChanged();
        }
        else
        {
            Debug.LogError("Couldn't increase/decrease delay");
        }
    }
}
