using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] Camera MainCamera;
    [SerializeField] public GameObject NotePrefab;
    private NoteProperties NP;

    /* Stage-grid data */
    [SerializeField] public string StageFilePath = "Stage_Texts/test3"; // Path from "Resources" folder
    [SerializeField] public int VerticalUnits = 4 * 6 * 7 * 8 * 9 * 16; // Musically popular note-weights and time signature
    [SerializeField] public int HorizontalUnits = 30;
    [SerializeField] public float VerticalPadding = 5500, HorizontalPadding = 4000;
    [HideInInspector] public Vector3 bottomLeftBorder;
    [HideInInspector] public Vector3 topRightBorder;
    [HideInInspector] public float StageWidth;
    [HideInInspector] public float StageHeight;

    /* Regular expressions for parsing stage files */
    private const string Exp1 = @"^(?<weight>\d+)$";
    private const string Exp2 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)$";
    private const string Exp3 = @"^(?<weight>\d+)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    private const string Exp4 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    public readonly string[] RegularExpressions = { Exp1, Exp2, Exp3, Exp4 };

    /* Stage-load data */
    private TextAsset StageFile;
    private string[] lines;
    /// <summary>
    ///  beats per minute, beats per second or seconds per beat
    /// </summary>
    [HideInInspector] public float BPM, BPS, SPB;
    [HideInInspector] public GameObject[][] Notes;
    [HideInInspector] public int[] Weights;
    private int NotesCounter1 = 0, currentVerUnit = 0, NotesZ_val = 0;
    private bool up = true;

    /* Stage speed and Bar data*/
    [HideInInspector] public float UnitsPerHorUnit;
    [HideInInspector] public float UnitsPerVerUnit;

    private readonly int _debug = 1;

    private void Awake()
    {
        bottomLeftBorder = GetBlBorder();
        topRightBorder = GetTrBorder();
        StageWidth = topRightBorder.x - bottomLeftBorder.x;
        StageHeight = topRightBorder.y - bottomLeftBorder.y;
        UnitsPerHorUnit = StageWidth / HorizontalUnits;
        UnitsPerVerUnit = StageHeight / VerticalUnits;

        StageFile = Resources.Load(StageFilePath) as TextAsset;
        if (StageFile == null)
        {
            throw new Exception($"Stage file not found");
        }

        lines = StageFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        BPM = float.Parse(lines[0]);
        BPS = BPM / 60;
        SPB = 1 / BPS;

        LoadNotes();
        Debug.Log("Stage notes loaded");

        NP = GetComponent<NoteProperties>();
        SetNotesProperties();
        Debug.Log($"Note properties updated to match {BPM} BPM");
    }
    void Start()
    {

    }
    Vector3 GetBlBorder()
    {
        Vector3 tmp = MainCamera.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        tmp.x += HorizontalPadding;
        tmp.y += VerticalPadding;
        return tmp;
    }
    Vector3 GetTrBorder()
    {
        Vector3 tmp = MainCamera.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        tmp.x -= HorizontalPadding;
        tmp.y -= VerticalPadding;
        return tmp;
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

    void SetNotesProperties()
    {
        float timeTillDestruction = 2 * SPB;

        float ScaleInPortion = 0.2f;
        float FadeInPortion = 0.3f;
        float LifeTimePortion = 1;
        float TappedScalePortion = 0.1f;
        float FadeOutPortion = 0.1f;

        float ScaleInTime = ScaleInPortion * timeTillDestruction;
        float FadeInTime = FadeInPortion * timeTillDestruction;
        float TappedScaleTime = TappedScalePortion * timeTillDestruction;
        float FadeOutTime = FadeOutPortion * timeTillDestruction;

        NP.ScaleInTime = ScaleInTime;
        NP.FadeInTime = FadeInTime;

        float ScaleInRate = (1 - NP.SpawnScaleMultiplier) / ScaleInTime;
        float FadeInRate = 1 / FadeInTime;
        float TappedScaleRate = NP.ScaleMultiplierWhenTapped / TappedScaleTime;
        float FadeOutRate = 1 / FadeOutTime;

        NP.ScaleInRate = ScaleInRate;
        NP.FadeInRate = FadeInRate;
        NP.LifeTime = LifeTimePortion * timeTillDestruction;
        NP.TappedScaleRate = TappedScaleRate;
        NP.FadeOutRate = FadeOutRate;

        NP.TappedAnimationScaleRate = 10; // GOTTA WORK ON THIS SHIT WTF
    }
}
