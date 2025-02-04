using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// A static class which provides information regarding the state of the current overworld, 
/// including save-file information and user-settings.
/// </summary>
public static class GameState
{
    /// <summary>
    /// Overlay camera used for screen animations (such as tap-feedback animation).
    /// </summary>
    public static Camera ScreenAnimationsCamera;

    static bool wasGameLoadedOnce = false;

    /// <summary>
    /// Which chapter was selected last in stage selection menu.
    /// </summary>
    public static int LastSelectedChapter = 0;

    /// <summary>
    /// A number representing the progression in the game so far.
    /// </summary>
    public static float TotalGameProgression { get; private set; } = 0;

    /// <summary>
    /// A Dictionary that maps each stage name to its difficulty level.
    /// </summary>
    public static Dictionary<string, string> DifficultiesMap { get; private set; }
        = Auxiliary.ParseKeyValueFile("DifficultiesMap");


    /***************** Variables that should be saved to a save-file *****************/
    /// <summary>
    /// The name of the last overworld scene entered by the player.
    /// </summary>
    public static string LastOverworldSceneName = "Overworld1";

    /// <summary>
    /// The last position of the player after the last scene change.
    /// </summary>
    public static Vector3 LastPlayerPosition = new Vector3(0, -4.75f, 9.6f);

    /// <summary>
    /// How many (and which) chapters were unlocked by now.
    /// For example if the value is '2' then chapters 0, 1 and 2 are currently unlocked.
    /// </summary>
    public static int UnlockedChapters = -1;

    /// <summary>
    /// A dictionary that lists the highscores of all stages.
    /// </summary>
    public static Auxiliary.SerializableDictionary<string, float> HighscoresMap = new();

    /// <summary>
    /// A HashSet containing the names of dialogs that were completed at least once.
    /// </summary>
    public static Auxiliary.SerializableHashSet<string> ExhaustedDialogs = new();

    /// <summary>
    /// A HashSet containing the names of tutorials that were completed at least once.
    /// </summary>
    public static Auxiliary.SerializableHashSet<string> ExhaustedTutorials = new();

    /// <summary>
    /// By how many seconds to delay the playback of the music in stages.
    /// If negative, music will start playing earlier in relation to the bar/Note spawns.
    /// </summary>
    public static float StageMusicDelay = 0;

    /// <summary>
    /// The size of Notes according to the game settings.
    /// </summary>
    public static float StageNoteSizeSettings = StageState.DefaultNoteScale;


    /* Save-load related */
    [Serializable]
    struct GameData
    {
        public string LastOverworldSceneName;
        public int UnlockedChapters;
        public Auxiliary.SerializableDictionary<string, float> HighscoresMap;
        public Auxiliary.SerializableHashSet<string> ExhaustedDialogs;
        public Auxiliary.SerializableHashSet<string> ExhaustedTutorials;
        public Vector3 LastPlayerPosition;
        public Settings settings;

        public GameData(int tmp)
        {
            LastOverworldSceneName = GameState.LastOverworldSceneName;
            LastPlayerPosition = GameState.LastPlayerPosition;
            UnlockedChapters = GameState.UnlockedChapters;
            HighscoresMap = GameState.HighscoresMap;
            ExhaustedDialogs = GameState.ExhaustedDialogs;
            ExhaustedTutorials = GameState.ExhaustedTutorials;
            settings = new Settings(1);
        }
    }

    [Serializable]
    struct Settings
    {
        public float StageMusicDelay;
        public float StageNoteSizeSettings;

        public Settings(int tmp)
        {
            StageMusicDelay = GameState.StageMusicDelay;
            StageNoteSizeSettings = GameState.StageNoteSizeSettings;
        }
    }

    static string SavePath => Path.Combine(Application.persistentDataPath, "tld_save.json");
    static GameData gameData = new GameData(1);

    /// <summary>
    /// Loads game progression from a save file.
    /// </summary>
    /// <param name="forceLoad">Whether to still load a save file if one 
    /// has already been loaded in this session.</param>
    public static void LoadGameState(bool forceLoad = false)
    {
        if (!(wasGameLoadedOnce && !forceLoad))
        {
            wasGameLoadedOnce = true;
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    var loadedGameData = JsonUtility.FromJson<GameData>(json);
                    gameData = loadedGameData;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load GameData from save file: {e.Message}");
                    throw;
                }

                Debug.Log("Game loaded successfully");
            }
            else
            {
                Debug.LogWarning("No save file found, creating a new one");
                gameData = new(1);
                SaveGameState();
            }
            AssignSaveFileParameters("load");
            SceneChanger.instance.StartSceneChange(LastOverworldSceneName, false);
        }
        SetProgressionFromHighscores();
    }

    /// <summary>
    /// Saves game progression to a save file.
    /// </summary>
    public static void SaveGameState()
    {
        AssignSaveFileParameters("save");

        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved successfully to {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// Changes save file parameters
    /// </summary>
    /// <param name="mode">If "load" will change GameState's save-parameters to be GameData's parameters,
    /// if "save" will change GameData's save-parameters to be GameState's parameters.</param>
    static void AssignSaveFileParameters(string mode)
    {
        if (mode == "load")
        {
            LastOverworldSceneName = gameData.LastOverworldSceneName;
            LastPlayerPosition = gameData.LastPlayerPosition;
            UnlockedChapters = gameData.UnlockedChapters;
            HighscoresMap = gameData.HighscoresMap;
            ExhaustedDialogs = gameData.ExhaustedDialogs;
            ExhaustedTutorials = gameData.ExhaustedTutorials;
            StageMusicDelay = gameData.settings.StageMusicDelay;
            StageNoteSizeSettings = gameData.settings.StageNoteSizeSettings;
        }
        else if (mode == "save")
        {
            gameData.LastOverworldSceneName = LastOverworldSceneName;
            //if (OverworldManager.instance != null)
            //{
            //    LastPlayerPosition = OverworldManager.instance.Player.transform.position;
            //}
            gameData.LastPlayerPosition = LastPlayerPosition;
            gameData.UnlockedChapters = UnlockedChapters;
            gameData.HighscoresMap = HighscoresMap;
            gameData.ExhaustedDialogs = ExhaustedDialogs;
            gameData.ExhaustedTutorials = ExhaustedTutorials;
            gameData.settings.StageMusicDelay = StageMusicDelay;
            gameData.settings.StageNoteSizeSettings = StageNoteSizeSettings;
        }
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            gameData = new GameData(1);
            Debug.Log("Save file deleted");
        }
    }


    /* Functions */
    static void SetProgressionFromHighscores()
    {
        TotalGameProgression = HighscoresMap.Values.Sum();
    }
}
