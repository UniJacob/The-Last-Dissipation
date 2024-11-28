using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class TimedSpawnerFromText : MonoBehaviour
{
    [SerializeField] GameObject NotePrefab;

    TextAsset StageFile;
    string[] lines;
    float BPM;
    float VUPS; // Vertical Units Per Second
    float SPVU; // Seconds Per Vertical Unit

    GameObject[][] Notes;
    int[] Weights;
    int NotesCounter1 = 0, NotesCounter2 = 0;



    void Start()
    {
        StageFile = Resources.Load(StageSettings.StageFilePath) as TextAsset;
        if (StageFile == null)
        {
            Debug.LogError("Stage file not found");
            throw new FileNotFoundException();
        }

        lines = StageFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        BPM = float.Parse(lines[0]);
        VUPS = BPM / 60 * StageSettings.VerUnits / 4; // 4 should be denominator, maybe generalize for 7/8, 9/8 songs
        SPVU = 1 / VUPS;

        LoadNotes();

        //SpawnRoutine();
    }

    void LoadNotes()
    {
        Notes = new GameObject[lines.Length - 1][];
        Weights = new int[lines.Length - 1];
        for (int i = 0; i < lines.Length - 1; ++i)
        {
            ParseString(lines[i + 1]);
            ++NotesCounter1;
        }
    }
    void ParseString(string input)
    {
        if (Regex.IsMatch(input, StageSettings.Exp1)) // Breaks
        {
            var match = Regex.Match(input, StageSettings.Exp1);
            int n = int.Parse(match.Groups["weight"].Value);

            Debug.Log($"{n}-weight break");

            InstantiateNotes(n);
        }
        else if (Regex.IsMatch(input, StageSettings.Exp2)) // Short notes only
        {
            var match = Regex.Match(input, StageSettings.Exp2);
            int n = int.Parse(match.Groups["weight"].Value);
            List<int> posList = ParseIntegerList(match.Groups["shortNotes"].Value);

            Debug.Log($"{n}-weight: notes in positions {string.Join(", ", posList)}");

            InstantiateNotes(n, posList);
        }
        else if (Regex.IsMatch(input, StageSettings.Exp3)) // Long notes only
        {
            var match = Regex.Match(input, StageSettings.Exp3);
            int n = int.Parse(match.Groups["weight"].Value);
            List<int> longPosList = ParseIntegerList(match.Groups["longNotes"].Value);

            Debug.Log($"{n}-weight long notes in positions {string.Join(", ", longPosList)}");
        }
        else if (Regex.IsMatch(input, StageSettings.Exp4)) // Short and long notes
        {
            var match = Regex.Match(input, StageSettings.Exp4);
            int n = int.Parse(match.Groups["weight"].Value);
            List<int> posList = ParseIntegerList(match.Groups["shortNotes"].Value);
            List<int> longPosList = ParseIntegerList(match.Groups["longNotes"].Value);

            Debug.Log($"{n}-weight: short notes in positions {string.Join(", ", posList)} " +
                $"and long notes in positions {string.Join(", ", longPosList)}");
        }
        else
        {
            Debug.Log($"Input string does not match any known pattern: {input}");
        }
    }

    List<int> ParseIntegerList(string input)
    {
        string[] parts = input.Split(',');
        List<int> numbers = new List<int>();
        foreach (string part in parts)
        {
            numbers.Add(int.Parse(part.Trim()));
        }
        return numbers;
    }

    int currentVerUnit = 0, depthCounter = 0;
    bool up = true;
    void InstantiateNotes(int weight, List<int> shortNotesPositions = null, List<int> longNotesPositions = null)
    {
        weight /= 2;
        if (shortNotesPositions != null)
        {
            Notes[NotesCounter1] = new GameObject[shortNotesPositions.Count];
            foreach (int position in shortNotesPositions)
            {
                Vector3 spawnPosition = new Vector3(
                    (StageSettings.topRightBorder.x * 2 / StageSettings.HorUnits) * position,
                    (StageSettings.topRightBorder.y * 2 / StageSettings.VerUnits) * currentVerUnit,
                    depthCounter++);
                Notes[NotesCounter1][NotesCounter2] = Instantiate(NotePrefab, spawnPosition, Quaternion.identity);
                Notes[NotesCounter1][NotesCounter2].SetActive(true); // should be false
                ++NotesCounter2;
            }
        }
        if (up)
        {
            currentVerUnit += StageSettings.VerUnits / weight;
            while (currentVerUnit > StageSettings.VerUnits / 2)
            {
                up = !up;
                currentVerUnit = (currentVerUnit - StageSettings.VerUnits) * -1;
            }
        }
        else
        {
            currentVerUnit -= StageSettings.VerUnits / weight;
            while (currentVerUnit < -StageSettings.VerUnits / 2)
            {
                up = !up;
                currentVerUnit = (currentVerUnit + StageSettings.VerUnits) * -1;
            }
        }

        Weights[NotesCounter1] = weight;
        NotesCounter2 = 0;
    }


    async void SpawnRoutine()
    {
        //while (true)
        //{
        //    float timeBetweenSpawnsInSeconds = Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);
        //    await Awaitable.WaitForSecondsAsync(timeBetweenSpawnsInSeconds);
        //    if (!this) break;   // might be destroyed when moving to a new scene
        //    Vector3 positionOfSpawnedObject = new Vector3(
        //        transform.position.x + Random.Range(-maxXDistance, +maxXDistance),
        //        transform.position.y + Random.Range(-maxYDistance, +maxYDistance),
        //        transform.position.z + zValue++);
        //    GameObject newObject = Instantiate(prefabToSpawn.gameObject, positionOfSpawnedObject, Quaternion.identity);
        //    newObject.transform.parent = parentOfAllInstances;
        //}
    }
}
