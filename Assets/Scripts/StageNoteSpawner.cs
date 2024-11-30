using System.Collections.Generic;
using UnityEngine;

public class StageNoteSpawner : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private StageManager SM;

    float BPM;
    GameObject[][] Notes;
    int[] Weights;

    void Start()
    {
        SM = StageManager.GetComponent<StageManager>();
        Notes = SM.Notes;
        Weights = SM.Weights;
        BPM = SM.BPM;

        BeginStage();
    }

    private async void BeginStage()
    {
        for (int i = 0; i < Notes.Length; ++i)
        {
            if (Notes[i] != null)
            {
                foreach (GameObject note in Notes[i])
                {
                    note.SetActive(true);
                }
            }
            float waitTime = 1 / (BPM / 60) / Weights[i];
            await Awaitable.WaitForSecondsAsync(waitTime);
        }
    }
}
