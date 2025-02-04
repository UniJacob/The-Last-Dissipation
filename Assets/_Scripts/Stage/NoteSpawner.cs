using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// A script responsible for spawning Note objects at precise timings.
/// Some function are self-explanatory.
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [SerializeField] StageManager StageManager;

    CancellationTokenSource SpawnCanceller;
    float waitTime;
    float lastTime;

    void Start()
    {
        Debug.Log("Starting Note spawn");
        SpawnCanceller = new CancellationTokenSource();
        BeginWaitForFirstBeat();
    }

    async void BeginWaitForFirstBeat()
    {
        lastTime = Time.time;
        waitTime = StageState.SPB / 2;
        if (GameState.StageMusicDelay < 0)
        {
            waitTime += -GameState.StageMusicDelay;
        }
        try
        {
            await Awaitable.WaitForSecondsAsync(waitTime, SpawnCanceller.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        BeginNoteSpawn();
    }

    /// <summary>
    /// Begins activating Note objects in their correct timings.
    /// Automatically adjusts for unwanted delays caused by system constraints.
    /// </summary>
    /// <param name="cancellationToken"></param>
    async void BeginNoteSpawn()
    {
        float BadDelay = Time.time - lastTime - waitTime;
        lastTime = Time.time;
        try
        {
            for (int i = 0; i < StageManager.Notes.Length; ++i)
            {
                if (StageManager.Notes[i] != null)
                {
                    foreach (GameObject note in StageManager.Notes[i])
                    {
                        note.GetComponent<NoteBehavior>().Activate();
                    }
                }
                waitTime = StageState.SPB / StageManager.Weights[i] - BadDelay;
                await Awaitable.WaitForSecondsAsync(waitTime, SpawnCanceller.Token);
                BadDelay = Time.time - lastTime - waitTime;
                lastTime = Time.time;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    public void Stop()
    {
        SpawnCanceller.Cancel();
        SpawnCanceller.Dispose();
        SpawnCanceller = null;
    }

    public void Restart()
    {
        Start();
    }
}
