using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A singleton script responsible for many aspects regarding stage management.
/// Some function are self-explanatory.
/// </summary>
public class StageManager : MonoBehaviour
{
    static StageManager instance;

    [SerializeField] Camera MainCamera;
    [SerializeField] GameObject NotePrefab;
    [SerializeField] BarBehavior Bar;
    [SerializeField] NoteProperties NoteProperties;
    [SerializeField] ScoreManager ScoreManager;
    [SerializeField] MenuManagerStage MenuManager;
    [SerializeField] NoteSpawner NoteSpawner;
    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] HUDTextSetter HUDTextSetter;
    [SerializeField] Image BackgroundImage;
    [Tooltip("How many seconds to wait after the music ends before showing the stage summary menu.")]
    [SerializeField] float ExtraMusicTimerTime = 1;

    [SerializeField] bool alwaysHardRestart = false;
    [SerializeField] bool loadFileFromResources = true;


    /// <summary>
    /// All Note objects of the stage.
    /// </summary>
    public GameObject[][] Notes { get; private set; }

    /// <summary>
    /// All Note objects weights of the stage.
    /// </summary>
    public int[] Weights { get; private set; }

    int NotesCounter1;
    float currentVerticalPos;
    int NotesZ_val;
    bool up;
    float MusicTimer;
    bool stopped;
    float musicPlayDelayStopwatch;
    float musicPlayDelay;

    [Tooltip("If greater than 0, debug logs will be enabled")]
    public int _DebugLog = 1;


    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
        {
            return;
        }

        Application.targetFrameRate = StageState.FrameCap;
        StageState.SetWorldSpaceParameters(ref MainCamera, ref NoteProperties);
        if (_DebugLog > 1)
        {
            Debug.Log($"Screen size: {StageState.ScreenBL}, {StageState.ScreenTR}");
            Debug.Log($"Spawn area size: {StageState.SpawnAreaBL}, {StageState.SpawnAreaTR}");
        }
        LoadStageFile();
        ResetTrackers();
        NoteProperties.SetPropertiesFromSPB(StageState.SPB);
        if (_DebugLog > 0)
            Debug.Log($"Note properties updated to match {StageState.BPM} BPM");
        SetBackgroundImage();
        AdjustMusicDelay();
        LoadNotes();
        if (_DebugLog > 0)
            Debug.Log("Stage notes loaded");
    }

    void Update()
    {
        if (stopped) return;
        if (MusicPlayer.clip == null) return;

        if (musicPlayDelayStopwatch < musicPlayDelay)
        {
            musicPlayDelayStopwatch += Time.deltaTime;
            if (musicPlayDelayStopwatch >= musicPlayDelay)
            {
                PlayMusic(musicPlayDelayStopwatch - musicPlayDelay);
            }
            return;
        }
        MusicTimer -= Time.deltaTime;
        if (MusicTimer <= 0)
        {
            if (StageState.InAdjustDelayMode)
            {
                Restart();
            }
            else
            {
                EndStage();
            }
        }
    }

    void SetCamera() // Not used for now
    {
        float targetaspect = 16.0f / 9.0f;
        float windowaspect = (float)Screen.width / (float)Screen.height;
        float scaleheight = windowaspect / targetaspect;

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

    void ResetTrackers()
    {
        NotesCounter1 = 0;
        currentVerticalPos = StageState.SpawnAreaHeight / 2;
        NotesZ_val = 0;
        up = false;
        if (MusicPlayer.clip != null)
        {
            MusicTimer = MusicPlayer.clip.length + ExtraMusicTimerTime + Mathf.Abs(GameState.StageMusicDelay);
        }
        stopped = false;
        musicPlayDelayStopwatch = 0;
    }

    /// <summary>
    /// Loads all Note objects of the stage.
    /// </summary>
    /// <param name="stageSpeedCoefficient">BPM coefficient of the stage</param>
    void LoadNotes()
    {
        Notes = new GameObject[StageState.StageTextLines.Length - 1][];
        Weights = new int[StageState.StageTextLines.Length - 1];
        for (int i = 2; i < StageState.StageTextLines.Length - 1; ++i)
        {
            var spawn_tuple = ParseLine(StageState.StageTextLines[i]);
            if (spawn_tuple == null)
            {
                continue;
            }
            InstantiateNotes(spawn_tuple.Item1 * StageState.StageSpeedCoefficient,
                spawn_tuple.Item2, spawn_tuple.Item3);
            ++NotesCounter1;
        }
    }

    /// <summary>
    /// Parses a line from the stage file.
    /// </summary>
    /// <param name="line">Given stage file line</param>
    /// <returns> Tuple containing: 1. Weight of current Note-group
    ///                             2. List of Short-Notes positions
    ///                             3. List of Long-Notes positions</returns>
    Tuple<int, List<int>, List<int>> ParseLine(string line)
    {
        Match match = null;
        foreach (string exp in StageState.RegularExpressions)
        {
            if (Regex.IsMatch(line, exp))
            {
                match = Regex.Match(line, exp);
                break;
            }
        }
        if (match == null)
        {
            if (line == "")
            {
                return null;
            }
            throw new Exception($"The line '{line}' in the stage file {StageState.StageFileName} does not match any known pattern");
        }

        int weight = int.Parse(match.Groups["weight"].Value);
        List<int> posList = ParseIntegerList(match.Groups["shortNotes"].Value);
        List<int> longPosList = ParseIntegerList(match.Groups["longNotes"].Value);

        if (_DebugLog > 1)
        {
            string tmp = $"{weight}-weight\n";
            if (posList != null)
            {
                tmp += $"Short notes: ({string.Join(", ", posList)})\n";
            }
            if (longPosList != null)
            {
                tmp += $"Long notes: ({string.Join(", ", longPosList)})";
            }
            Debug.Log(tmp);
        }

        return new(weight, posList, longPosList);
    }

    /// <summary>
    /// Parses a string representing an integer list. For example, -3,4,200 would be the list [-3, 4, 200].
    /// </summary>
    /// <param name="strIntList">Given string that represents an integer list</param>
    /// <returns></returns>
    List<int> ParseIntegerList(string strIntList)
    {
        if (strIntList == null || strIntList == "")
        {
            return null;
        }
        string[] parts = strIntList.Split(',');
        List<int> list = new List<int>();
        foreach (string part in parts)
        {
            list.Add(int.Parse(part.Trim()));
        }
        return list;
    }

    /// <summary>
    /// Instantiates Note objects, in positions dictated by their musical weight.
    /// </summary>
    /// <param name="weight">Musical weight of the notes</param>
    /// <param name="shortNotesPositions">'Short' Note objects to instantiates</param>
    /// <param name="longNotesPositions">'Long' Note objects to instantiates</param>                // Currently not implemented
    void InstantiateNotes(int weight, List<int> shortNotesPositions = null,
        List<int> longNotesPositions = null)
    {
        if (shortNotesPositions != null)
        {
            int NotesCounter2 = 0;
            Notes[NotesCounter1] = new GameObject[shortNotesPositions.Count];
            foreach (int position in shortNotesPositions)
            {
                Vector3 spawnPosition = new Vector3(
                    position * StageState.UnitsPerHorUnit,
                    currentVerticalPos,
                    NotesZ_val++);
                Notes[NotesCounter1][NotesCounter2] = Instantiate(NotePrefab, spawnPosition, Quaternion.identity);
                Notes[NotesCounter1][NotesCounter2].SetActive(false);
                ++NotesCounter2;
            }
        }
        if (longNotesPositions != null)
        {
            throw new NotImplementedException();
        }

        int sign = (up ? 1 : -1);
        currentVerticalPos += sign * (StageState.SpawnAreaHeight / weight);
        if (Mathf.Abs(currentVerticalPos) > StageState.SpawnAreaHeight / 2)
        {
            currentVerticalPos = sign * StageState.SpawnAreaHeight - currentVerticalPos;
            up = !up;
        }
        Weights[NotesCounter1] = weight;
    }

    /// <summary>
    /// Starts playing the music track of the stage.
    /// </summary>
    /// <param name="playbackTime">Time in seconds from which to start the playback.</param>
    void PlayMusic(float playbackTime)
    {
        if (MusicPlayer.clip == null) return;
        if (playbackTime > 0)
        {
            MusicPlayer.time = playbackTime;
        }
        MusicPlayer.Play();
    }

    /// <summary>
    /// Ends the stage and displays the stage-summary menu.
    /// </summary>
    public void EndStage()
    {
        StopStage();
        HUDTextSetter.UpdateScoreTrackers();
        MenuManager.PartialEndStage();
        StageState.IsEnded = true;
        UpdateHighscore();
    }

    void StopStage()
    {
        stopped = true;
        if (MusicPlayer.enabled)
        {
            MusicPlayer.Stop();
        }
        NoteSpawner.Stop();
        Bar.Stop();
        DestroyAllNotes();
    }

    /// <summary>
    /// Restarts the stage.
    /// </summary>
    /// <param name="hardRestart">If true will reload the stage file and 
    /// re-adjust music playback delay.</param>
    public void Restart(bool hardRestart = false)
    {
        if (!stopped)
        {
            StopStage();
        }
        if (hardRestart || alwaysHardRestart)
        {
            LoadStageFile();
            AdjustMusicDelay();
        }
        ScoreManager.Restart();
        ResetTrackers();
        LoadNotes();
        MenuManager.Restart();
        NoteSpawner.Restart();
        Bar.Restart();
        HUDTextSetter.Restart();
        StageState.IsEnded = false;
        if (_DebugLog > 0)
            Debug.Log("Stage restarted");
    }

    /// <summary>
    /// Loads the stage file that is referenced in StageState.
    /// </summary>
    void LoadStageFile()
    {
        StageState.LoadStageFile(StageState.StageSpeedCoefficient, loadFileFromResources);
        if (_DebugLog > 0)
            Debug.Log("Stage file loaded");
        MusicPlayer.clip = StageState.MusicClip;
        MusicPlayer.clip?.LoadAudioData();
        if (_DebugLog > 0)
            Debug.Log("Music file loaded");
    }

    public void Quit()
    {
        if (!stopped)
        {
            StopStage();
        }
        if (_DebugLog > 0)
        {
            Debug.Log("Quitting Stage");
        }
        StageState.InAdjustDelayMode = false;
        SceneChanger.instance.StartSceneChange(GameState.LastOverworldSceneName);
    }

    /// <summary>
    /// Destroys all the Notes that are currently instantiated.
    /// </summary>
    void DestroyAllNotes()
    {
        for (int i = 0; i < Notes.Length; ++i)
        {
            if (Notes[i] != null)
            {
                foreach (GameObject note in Notes[i])
                {
                    if (note != null)
                    {
                        Destroy(note);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates the highscore of the stage in needed.
    /// </summary>
    void UpdateHighscore()
    {
        float currentScore = ScoreManager.GetFinalScore();
        if (GameState.HighscoresMap.ContainsKey(StageState.StageFileName))
        {
            if (currentScore <= GameState.HighscoresMap[StageState.StageFileName])
            {
                return;
            }
        }
        GameState.HighscoresMap[StageState.StageFileName] = currentScore;
        GameState.SaveGameState();
    }

    void AdjustMusicDelay()
    {
        musicPlayDelay = StageState.SPB / 2 + NoteProperties.ScaleInTime + NoteProperties.FadeInTime;
        if (GameState.StageMusicDelay > 0)
        {
            musicPlayDelay += GameState.StageMusicDelay;
        }
    }

    void SetBackgroundImage()
    {
        if (StageState.BackgroundImage != null)
        {
            BackgroundImage.sprite = StageState.BackgroundImage;
        }
        var aspectRatioFitter = BackgroundImage.gameObject.GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter != null)
        {
            if (StageState.InAdjustDelayMode)
            {
                aspectRatioFitter.aspectRatio = (float)16 / 9;
            }
            else
            {
                aspectRatioFitter.aspectRatio = (float)Screen.width / Screen.height;
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(StageState.SpawnAreaBL, StageState.SpawnAreaTR);
    }
}
