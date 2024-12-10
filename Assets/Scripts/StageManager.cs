using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] Camera MainCamera;
    [SerializeField] GameObject NotePrefab;
    NoteProperties NP;

    /* Stage-grid data */
    [Tooltip("Path from \"Resources\" folder")]
    public string StageFilePath = "Stage_Texts/test3";
    [Tooltip("Precentage of vertical/horizontal space to not use")]
    [SerializeField] float VerticalPadding = 0.33f, HorizontalPadding = 0.2f;
    public readonly int HorizontalUnits = 30;
    readonly int VerticalUnits = 4 * 6 * 7 * 8 * 9 * 16; // Musically popular note-weights and time signatures
    Vector3 BL_Corner, TR_Corner;
    Vector3 SpawnBL_Corner, SpawnTR_Corner;
    [HideInInspector] public float Width;
    [HideInInspector] public float Height;
    float SpawnWidth;
    [HideInInspector] public float SpawnHeight;
    float UnitsPerHorUnit;
    float UnitsPerVerUnit;

    /* Regular expressions for parsing stage files */
    const string Exp1 = @"^(?<weight>\d+)$";
    const string Exp2 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)$";
    const string Exp3 = @"^(?<weight>\d+)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    const string Exp4 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    public readonly string[] RegularExpressions = { Exp1, Exp2, Exp3, Exp4 };

    /* Stage-load data */
    /// <summary>
    ///  BPM = beats per minute,
    ///  BPS = beats per second,
    ///  SPB = seconds per beat
    /// </summary>
    [HideInInspector] public float BPM, BPS, SPB;
    [HideInInspector] public GameObject[][] Notes;
    [HideInInspector] public int[] Weights;
    TextAsset StageFile;
    string[] lines;
    int NotesCounter1 = 0, currentVerUnit = 0, NotesZ_val = 0;
    bool up = true;

    /* Note Animation Timing */
    //readonly float ScaleInPortion = 0.2f;
    //readonly float FadeInPortion = 0.3f;
    //readonly float LifeTimePortion = 0.7f;
    //readonly float TappedScalePortion = 0.1f;
    //readonly float FadeOutPortion = 0.1f;

    /* Debug */
    readonly int _debug = 1;

    void Awake()
    {
        NP = GetComponent<NoteProperties>();
        SetCamera();
        SetStageAreaParameters();

        StageFile = Resources.Load(StageFilePath) as TextAsset;
        if (StageFile == null)
        {
            throw new Exception($"Stage file not found");
        }
        lines = StageFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; ++i)
        {
            if (lines[i].Contains(' '))
            {
                lines[i] = lines[i].Substring(0, lines[i].IndexOf(' '));
            }
        }

        SetBPM();
        LoadNotes();
        Debug.Log("Stage notes loaded");
        //SetNotesProperties();
        NP.SetPropertiesFromSPB(SPB);
        Debug.Log($"Note properties updated to match {BPM} BPM");
    }
    public void SetCamera()
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
    void SetStageAreaParameters()
    {
        float HoriPrec = 1 - HorizontalPadding, VertiPrec = 1 - VerticalPadding;

        BL_Corner = MainCamera.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        SpawnBL_Corner = new Vector3(
            (BL_Corner.x * HoriPrec + NP.DefaultSize / 2),
            (BL_Corner.y * VertiPrec + NP.DefaultSize / 2),
            0);
        Debug.Log("Bottom left corners: " + BL_Corner + ", " + SpawnBL_Corner);

        TR_Corner = MainCamera.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        SpawnTR_Corner = new Vector3(
            (TR_Corner.x * HoriPrec - NP.DefaultSize / 2),
            (TR_Corner.y * VertiPrec - NP.DefaultSize / 2),
           0);
        Debug.Log("Top right corners: " + TR_Corner + ", " + SpawnTR_Corner);

        Width = TR_Corner.x - BL_Corner.x;
        Height = TR_Corner.y - BL_Corner.y;
        SpawnWidth = SpawnTR_Corner.x - SpawnBL_Corner.x;
        SpawnHeight = SpawnTR_Corner.y - SpawnBL_Corner.y;

        UnitsPerHorUnit = SpawnWidth / HorizontalUnits;
        UnitsPerVerUnit = SpawnHeight / VerticalUnits;
    }
    void SetBPM()
    {
        BPM = float.Parse(lines[0]);
        BPS = BPM / 60;
        SPB = 1 / BPS;
    }
    void LoadNotes()
    {
        Notes = new GameObject[lines.Length - 1][];
        Weights = new int[lines.Length - 1];
        for (int i = 0; i < lines.Length - 1; ++i)
        {
            var spawn_tuple = ParseLine(lines[i + 1]);
            InstantiateNotes(spawn_tuple.Item1, spawn_tuple.Item2, spawn_tuple.Item3);
            ++NotesCounter1;
        }
    }
    Tuple<int, List<int>, List<int>> ParseLine(string line)
    {
        Match match = null;
        foreach (string exp in RegularExpressions)
        {
            if (Regex.IsMatch(line, exp))
            {
                match = Regex.Match(line, exp);
                break;
            }
        }
        if (match == null)
        {
            throw new Exception($"Line '{line}' in file {StageFile.name} does not match any known pattern");
        }

        int weight = int.Parse(match.Groups["weight"].Value);
        List<int> posList = ParseIntegerList(match.Groups["shortNotes"].Value);
        List<int> longPosList = ParseIntegerList(match.Groups["longNotes"].Value);

        if (_debug > 1)
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
    List<int> ParseIntegerList(string list_S)
    {
        if (list_S == null || list_S == "")
        {
            return null;
        }
        string[] parts = list_S.Split(',');
        List<int> list = new List<int>();
        foreach (string part in parts)
        {
            list.Add(int.Parse(part.Trim()));
        }
        return list;
    }
    void InstantiateNotes(int weight, List<int> shortNotesPositions = null, List<int> longNotesPositions = null)
    {
        if (shortNotesPositions != null)
        {
            int NotesCounter2 = 0;
            Notes[NotesCounter1] = new GameObject[shortNotesPositions.Count];
            foreach (int position in shortNotesPositions)
            {
                Vector3 spawnPosition = new Vector3(
                    position * UnitsPerHorUnit,
                    currentVerUnit * UnitsPerVerUnit,
                    NotesZ_val++);
                Notes[NotesCounter1][NotesCounter2] = Instantiate(NotePrefab, spawnPosition, Quaternion.identity);
                Notes[NotesCounter1][NotesCounter2].SetActive(false);
                ++NotesCounter2;
            }
        }

        int sign = (up ? 1 : -1);
        currentVerUnit += sign * (VerticalUnits / weight);
        if (Mathf.Abs(currentVerUnit) > VerticalUnits / 2)
        {
            currentVerUnit = sign * VerticalUnits - currentVerUnit;
            up = !up;
        }
        Weights[NotesCounter1] = weight;
    }
    public void PlayMusic()
    {
        GetComponentInChildren<AudioSource>().Play();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(SpawnBL_Corner, SpawnTR_Corner);
    }
}
