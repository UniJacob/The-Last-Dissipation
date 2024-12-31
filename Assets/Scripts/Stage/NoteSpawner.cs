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

    void Start()
    {
        SpawnCanceller = new CancellationTokenSource();
        BeginStage(SpawnCanceller.Token);
    }

    /// <summary>
    /// Begins activating Note objects in their correct timings.
    /// Automatically adjusts for unwanted delays caused by system constraints.
    /// </summary>
    /// <param name="cancellationToken"></param>
    async void BeginStage(CancellationToken cancellationToken)
    {
        float lastTime = Time.time, BadDelay = 0;
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
                float waitTime = StageState.SPB / StageManager.Weights[i] - BadDelay;
                await Awaitable.WaitForSecondsAsync(waitTime, cancellationToken);
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
