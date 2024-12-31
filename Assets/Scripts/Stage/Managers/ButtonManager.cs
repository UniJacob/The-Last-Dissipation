using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A singleton script responsible for handling all of the UI buttons in stage menus. 
/// Function are self-explanatory.
/// </summary>
public class ButtonManager : MonoBehaviour
{
    [SerializeField] StageManager StageManager;
    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject StageSummary;

    [SerializeField] GameObject PauseButton;
    [SerializeField] Button ResumeButton;
    [SerializeField] Button RestartButton;
    [SerializeField] GameObject RestartButton2;
    [SerializeField] Button QuitButton;

    static ButtonManager instance;

    void Awake()
    {
        if (!Auxiliary.AssureSingleton(ref instance, gameObject))
        {
            return;
        }

        PauseButton.GetComponent<Button>().onClick.AddListener(TogglePause);
        ResumeButton.onClick.AddListener(TogglePause);
        RestartButton.onClick.AddListener(RestartStage);
        RestartButton2.GetComponent<Button>().onClick.AddListener(RestartStage);
        QuitButton.onClick.AddListener(EndStage);
    }
    public void Start()
    {
        PauseButton.SetActive(true);
        RestartButton2.SetActive(false);
        StageSummary.SetActive(false);
        Resume(false);
    }

    void TogglePause()
    {
        Debug.Log("Pause Toggled");
        if (StageState.IsPaused)
        {
            Resume(true);
        }
        else
        {
            Pause(true);
        }
    }

    void Pause(bool showMenu)
    {
        StageState.IsPaused = true;
        if (showMenu)
        {
            PauseMenu.SetActive(true);
        }
        Time.timeScale = 0;
        if (MusicPlayer.enabled)
        {
            MusicPlayer.Pause();
        }
    }

    void Resume(bool PlayMusic)
    {
        StageState.IsPaused = false;
        PauseMenu.SetActive(false);
        Time.timeScale = 1;
        if (PlayMusic && MusicPlayer.enabled)
        {
            MusicPlayer.Play();
        }
    }

    void RestartStage()
    {
        StageManager.Restart();
    }

    void EndStage()
    {
        StageManager.EndStage();
    }

    public void PartialEndStage()
    {
        PauseMenu.SetActive(false);
        PauseButton.SetActive(false);
        RestartButton2.SetActive(true);
        StageSummary.SetActive(true);
    }
}
