using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script responsible for general management when the player is in the overworld,
/// including screen-animation timings and menu management.
/// </summary>
public class OverworldManager : MonoBehaviour
{
    static OverworldManager _instance;
    /// <summary>
    /// The (sole) existing instance of this class.
    /// </summary>
    public static OverworldManager instance { get; private set; }

    [SerializeField] Camera ScreenAnimationsCamera;
    [SerializeField] GameObject ChapterUnlockedAnimationPrefab;
    [SerializeField] StageSelector StageSelectionMenu;
    [SerializeField] CanvasFader SettingsMenu;
    [SerializeField] Sprite TutorialMusicDelayImage;


    [SerializeField] bool WebGLCompatibility = false;

    //bool waitingForAnimationEnd = false;
    enum Menu { None, StageSelection, Settings };
    Menu currentMenu = Menu.None;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref _instance, gameObject)) return;
        instance = _instance;

        Application.targetFrameRate = OverworldState.FrameCap;
        OverworldState.WebGLCompatibility = WebGLCompatibility;

        StageSelectionMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);

        GameState.ScreenAnimationsCamera = ScreenAnimationsCamera;
        GameState.LoadGameState();
    }

    void Update()
    {
        //if (waitingForAnimationEnd && OverworldState.IsInAnimation) return;
        if (OverworldState.IsInAnimation) return;

        if (currentMenu == Menu.None) return;
        OverworldState.IsInMenu = true;
        if (currentMenu == Menu.StageSelection)
        {
            StageSelectionMenu.gameObject.SetActive(true);
        }
        else if (currentMenu == Menu.Settings)
        {
            SettingsMenu.gameObject.SetActive(true);
        }
        currentMenu = Menu.None;

        //OverworldState.IsInMenu = true;
        //StageSelectionMenu.gameObject.SetActive(true);
        //waitingForAnimationEnd = false;
    }
    /// <summary>
    /// If relevant, unlocks a new chapter while displaying an animation.
    /// </summary>
    /// <param name="chapterNumber"></param>
    public void UnlockChapter(int chapterNumber)
    {
        if (chapterNumber <= GameState.UnlockedChapters) return;
        GameState.UnlockedChapters = chapterNumber;
        StageSelectionMenu.UpdateForNewChapterUnlock();
        Instantiate(ChapterUnlockedAnimationPrefab);
        GameState.SaveGameState();
    }

    public void OpenStageSelection()
    {
        if (OverworldState.IsInMenu == true || currentMenu != Menu.None) return;
        //waitingForAnimationEnd = true;
        currentMenu = Menu.StageSelection;
    }

    public void ToggleSettingsMenu()
    {
        if (SettingsMenu.gameObject.activeSelf)
        {
            SettingsMenu.TriggerFadeOut();
            OverworldState.IsInMenu = false;
            return;
        }
        if (OverworldState.IsInMenu == true || currentMenu != Menu.None) return;
        currentMenu = Menu.Settings;
    }

    public void BeginStage(string stageName, string stageDifficulty, bool setBackgroundImage = true)
    {
        GameState.LastOverworldSceneName = SceneManager.GetActiveScene().name;
        StageState.StageFileName = stageName;
        StageState.StageDifficultyLevel = stageDifficulty;
        if (setBackgroundImage)
        {
            StageState.BackgroundImage = Resources.Load<Sprite>(StageState.StageImagesPath + stageName);
        }
        OverworldState.IsInMenu = false;
        SceneChanger.instance.StartSceneChange(StageState.StageSceneName);
    }

    public void BeginMusicDelayAdjustment()
    {
        StageState.BackgroundImage = TutorialMusicDelayImage;
        StageState.InAdjustDelayMode = true;
        BeginStage("60 BPM", "", false);
    }
}
