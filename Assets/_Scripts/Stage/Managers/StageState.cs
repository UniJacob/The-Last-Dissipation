using System;
using UnityEngine;

/// <summary>
/// A static class which provides information regarding the state of the current stage, and other more general stage parameters.
/// Some function are self-explanatory.
/// </summary>
public static class StageState
{
    /* General settings */
    /// <summary>
    /// FPS cap when in a stage.
    /// </summary>
    public const int FrameCap = 120;

    /// <summary>
    /// Name of the stage scene (currently there is only one).
    /// </summary>
    public const string StageSceneName = "Stage";

    /// <summary>
    /// How many different vertical positions Note objects can be spawned at. 
    /// This should never be changed since stage files rely on it.
    /// </summary>
    public const int HorizontalUnits = 30;


    /* Screen size, "grid" and ratio related */
    /// <summary>
    /// Ratio between the height of the area where Note objects can spawn to the height of the screen.
    /// </summary>
    const float SpawnAreaHeightRatio = 0.67f;

    /// <summary>
    /// Ratio between the width of the area where Note objects can spawn to the width of the screen.
    /// </summary>
    const float SpawnAreaWidthRatio = 0.8f;

    /// <summary>
    /// World coordinates of the bottom left corner of the screen.
    /// </summary>
    public static Vector3 ScreenBL { get; private set; }

    /// <summary>
    /// World coordinates of the top right corner of the screen.
    /// </summary>
    public static Vector3 ScreenTR { get; private set; }

    /// <summary>
    /// World coordinates of the bottom left corner of the area where Note objects can spawn in.
    /// </summary>
    public static Vector3 SpawnAreaBL { get; private set; }

    /// <summary>
    /// World coordinates of the top right corner of the area where Note objects can spawn in.
    /// </summary>
    public static Vector3 SpawnAreaTR { get; private set; }

    /// <summary>
    /// Width of the screen (world units).
    /// </summary>
    public static float ScreenWidth { get; private set; }

    static float ScreenHeight;

    /// <summary>
    /// Width of the spawn area of Note objects (world units).
    /// </summary>
    static float SpawnAreaWidth;

    /// <summary>
    /// Height of the area where Note objects can spawn in (world units).
    /// </summary>
    public static float SpawnAreaHeight { get; private set; }

    /// <summary>
    /// World units per one HorizontalUnit.
    /// </summary>
    public static float UnitsPerHorUnit { get; private set; }


    /* File parsing related */
    public static string StageFileName = "tst";

    public static string StageDifficultyLevel = "difficulty";

    /// <summary>
    /// Path from Resources folder to the folder where stage-text files are saved.
    /// </summary>
    public const string TxtsPath = "Stage_Texts/";

    /// <summary>
    /// Path from Resources folder to the folder where stage-audio files are saved.
    /// </summary>
    public const string AudiosPath = "Stage_Audios/";

    /// <summary>
    /// Path from Resources folder to the folder where stage-image files are saved.
    /// </summary>
    public const string StageImagesPath = "Stage_Images/";

    /// <summary>
    /// Path from Resources folder to the folder where stage-thumbnail files are saved.
    /// </summary>
    public const string StageThumbnailsPath = "Stage_Images/Stage_Thumbnails/";

    /// <summary>
    /// Regular expressions for parsing stage files.
    /// </summary>
    public static readonly string[] RegularExpressions = { Exp1, Exp2, Exp3, Exp4 };
    const string Exp1 = @"^(?<weight>\d+)$";
    const string Exp2 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)$";
    const string Exp3 = @"^(?<weight>\d+)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    const string Exp4 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";

    /// <summary>
    /// The default size/scale of Notes.
    /// </summary>
    public static float DefaultNoteScale = 0.57f;


    /* Stage-specific variables */
    /// <summary>
    /// The lines of the current stage file.
    /// </summary>
    public static string[] StageTextLines { get; private set; }

    /// <summary>
    /// Music clip of the current stage.
    /// </summary>
    public static AudioClip MusicClip { get; private set; } = null;

    /// <summary>
    ///  Beats per minute,
    /// </summary>
    public static float BPM { get; private set; }

    /// <summary>
    ///  Beats per second,
    /// </summary>
    public static float BPS { get; private set; }

    /// <summary>
    ///  Seconds per beat
    /// </summary>
    public static float SPB { get; private set; }

    /// <summary>
    /// Speed of the bar (units per second);
    /// </summary>
    public static float BarSpeed { get; private set; }

    /// <summary>
    /// Whether the stage is paused.
    /// </summary>
    public static bool IsPaused = false;

    /// <summary>
    /// Whether the stage has ended.
    /// </summary>
    public static bool IsEnded = false;

    /// <summary>
    /// Background image of the current stage.
    /// </summary>
    public static Sprite BackgroundImage = null;

    /// <summary>
    /// BPM coefficient of the stage. For example, if it is set to 2 then the bar
    /// will move twice as slow but Notes will spawn twice as fast.
    /// </summary>
    public static int StageSpeedCoefficient = 1;

    /// <summary>
    /// Whether the Adjust Music Delay stage is currently played.
    /// </summary>
    public static bool InAdjustDelayMode = false;

    /* Functions */
    /// <summary>
    /// Sets various stage parameters according to the size of the screen shown by a given camera.
    /// Also requires a NoteProperties for assuring Notes won't be partially hidden by screen bounds and menus.
    /// </summary>
    /// <param name="MainCamera">Given main camera of the stage</param>
    /// <param name="NoteProperties">Given NoteProperties</param>
    public static void SetWorldSpaceParameters(ref Camera MainCamera, ref NoteProperties NoteProperties)
    {
        ScreenBL = MainCamera.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        SpawnAreaBL = new Vector3(
            (ScreenBL.x * SpawnAreaWidthRatio + NoteProperties.NoteSize),
            (ScreenBL.y * SpawnAreaHeightRatio + NoteProperties.NoteSize),
            0);

        ScreenTR = MainCamera.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        SpawnAreaTR = new Vector3(
            (ScreenTR.x * SpawnAreaWidthRatio - NoteProperties.NoteSize),
            (ScreenTR.y * SpawnAreaHeightRatio - NoteProperties.NoteSize),
           0);

        ScreenWidth = ScreenTR.x - ScreenBL.x;
        ScreenHeight = ScreenTR.y - ScreenBL.y;
        SpawnAreaWidth = SpawnAreaTR.x - SpawnAreaBL.x;
        SpawnAreaHeight = SpawnAreaTR.y - SpawnAreaBL.y;

        UnitsPerHorUnit = SpawnAreaWidth / HorizontalUnits;
    }

    /// <summary>
    /// Loads a stage-text '.txt' file (or "stage file" for short) and adjusts various class variables.
    /// </summary>
    public static void LoadStageFile(int stageSpeedCoefficient, bool fromResourcesFolder)
    {
        var tmp = TxtsPath + StageFileName;

        //TextAsset StageFile = Resources.Load(tmp) as TextAsset;
        TextAsset StageFile;
        if (Application.isEditor && !fromResourcesFolder)
        {
            string filePath = "E:/PC/Desktop/tld_tmp/" + tmp + ".txt";
            string fileContent = System.IO.File.ReadAllText(filePath);
            StageFile = new TextAsset(fileContent);
        }
        else
        {
            StageFile = Resources.Load(tmp) as TextAsset;
        }

        if (StageFile == null)
        {
            throw new Exception($"Stage file not found at Resources/{tmp}");
        }
        StageTextLines = StageFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int i = 2; i < StageTextLines.Length; ++i)
        {
            if (StageTextLines[i].Contains(' '))
            {
                StageTextLines[i] = StageTextLines[i].Substring(0, StageTextLines[i].IndexOf(' '));
            }
        }
        SetBPM(stageSpeedCoefficient);
        SetMusicClip();
        BarSpeed = SpawnAreaHeight * BPS;
    }

    static void SetBPM(int stageSpeedCoefficient)
    {
        BPM = float.Parse(StageTextLines[0]);
        BPM /= stageSpeedCoefficient;
        BPS = BPM / 60;
        SPB = 1 / BPS;
    }

    static void SetMusicClip()
    {
        if (StageTextLines[1] == "")
        {
            Debug.LogWarning("Notice - stage has no music");
            MusicClip = null;
            return;
        }
        string MusicFilePath = AudiosPath + StageTextLines[1];
        AudioClip clip = Resources.Load(MusicFilePath) as AudioClip;
        if (clip == null)
        {
            throw new Exception($"Music file not found at Resources/{MusicFilePath}");
        }
        MusicClip = clip;
    }
}
