using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public static OverworldManager instance { get { return _instance; } }

    [SerializeField] Vector3[] corners;

    [SerializeField] Camera ScreenAnimationsCamera;
    [SerializeField] GameObject ChapterUnlockedAnimationPrefab;
    [SerializeField] StageSelector StageSelectionMenu;
    [SerializeField] CanvasFader SettingsMenu;
    [SerializeField] Sprite TutorialMusicDelayImage;
    [SerializeField] MapManager MapManager;
    [SerializeField] AudioSource BackgroundMusicPlayer;
    [SerializeField] Slider SettingsNoteSizeSlider;
    [SerializeField] TMP_Text SettingsNoteSizeValue;
    public PlayerMovement Player;

    [SerializeField] bool WebGLCompatibility = false;

    enum Menu { None, StageSelection, Settings };
    Menu MenuToToggle = Menu.None;
    bool settingsValuesChanged = false;

    float maxAspectRatio = 2.164104f;
    float backgroundMusicFadeSpeed = 0.5f;


    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref _instance, gameObject)) return;

        Application.targetFrameRate = OverworldState.FrameCap;
        OverworldState.WebGLCompatibility = WebGLCompatibility;
        SetCamera();

        StageSelectionMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);

        GameState.ScreenAnimationsCamera = ScreenAnimationsCamera;
        GameState.LoadGameState();
        MapManager.UpdateMap();
        SettingsNoteSizeSlider.onValueChanged.AddListener((v) =>
        {
            GameState.StageNoteSizeSettings = v;
            settingsValuesChanged = true;
            SettingsNoteSizeValue.text = v.ToString("0.00");
        });
    }

    void Update() // Update is for menu related logic
    {
        if (OverworldState.IsInAnimation) return;
        if (MenuToToggle == Menu.None) return;

        if (MenuToToggle == Menu.StageSelection)
        {
            if (StageSelectionMenu.gameObject.activeSelf)
            {
                StageSelectionMenu.CloseStageSelection();
            }
            else if (!OverworldState.IsInMenu)
            {
                OverworldState.IsInMenu = true;
                StageSelectionMenu.gameObject.SetActive(true);
            }
        }
        else if (MenuToToggle == Menu.Settings)
        {
            OverworldState.IsInMenu = true;
            SettingsMenu.gameObject.SetActive(true);
        }
        MenuToToggle = Menu.None;
    }

    void SetCamera()
    {
        float windowaspect = (float)Screen.width / (float)Screen.height;
        if (windowaspect <= maxAspectRatio)
        {
            return;
        }
        float scaleheight = windowaspect / maxAspectRatio;
        var MainCamera = Camera.main;

        Rect rect = MainCamera.rect;
        if (scaleheight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
            MainCamera.rect = rect;
        }
        else
        {
            float scalewidth = 1 / scaleheight;
            rect.width = scalewidth;
            rect.height = 1;
            rect.x = (1 - scalewidth) / 2;
            rect.y = 0;
            MainCamera.rect = rect;
        }
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
        var tmp = Instantiate(ChapterUnlockedAnimationPrefab);
        GameState.SaveGameState();
        StartCoroutine(FadeVolumeTemporarily(BackgroundMusicPlayer,
            tmp.GetComponent<AudioSource>().clip.length));
    }

    public IEnumerator FadeVolumeTemporarily(AudioSource audioSource, float duration)
    {
        // Store the original volume
        float originalVolume = audioSource.volume;

        // Gradually lower the volume to 0
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime * backgroundMusicFadeSpeed;
            yield return null;
        }

        // Ensure volume is exactly 0
        audioSource.volume = 0;

        // Wait for specified duration
        yield return new WaitForSeconds(duration);

        // Gradually restore the volume
        while (audioSource.volume < originalVolume)
        {
            audioSource.volume += Time.deltaTime * backgroundMusicFadeSpeed;
            yield return null;
        }

        // Ensure volume is exactly at original value
        audioSource.volume = originalVolume;
    }

    public void ToggleStageSelection()
    {
        if (MenuToToggle != Menu.None) return;

        MenuToToggle = Menu.StageSelection;
    }

    //public void CloseStageSelection()
    //{
    //    StageSelectionMenu.CloseStageSelection();
    //}

    public void ToggleSettingsMenu()
    {
        if (SettingsMenu.gameObject.activeSelf)
        {
            SettingsMenu.TriggerFadeOut();
            OverworldState.IsInMenu = false;
            if (settingsValuesChanged)
            {
                settingsValuesChanged = false;
                GameState.SaveGameState();
            }
            return;
        }
        if (OverworldState.IsInMenu == true || MenuToToggle != Menu.None) return;
        MenuToToggle = Menu.Settings;
        SettingsNoteSizeSlider.value = GameState.StageNoteSizeSettings;
    }

    public void BeginStage(
        string stageName, string stageDifficulty, bool setBackgroundImage, int stageSpeedCoefficient = 1)
    {
        GameState.LastOverworldSceneName = SceneManager.GetActiveScene().name;
        StageState.StageFileName = stageName;
        StageState.StageDifficultyLevel = stageDifficulty;
        StageState.StageSpeedCoefficient = stageSpeedCoefficient;
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
