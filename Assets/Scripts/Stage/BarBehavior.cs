using System;
using UnityEngine;

/// <summary>
/// A script responsible for the stage-bar behavior and timing.
/// </summary>
public class BarBehavior : MonoBehaviour
{
    [SerializeField] StageManager StageManager;
    [SerializeField] NoteProperties NoteProperties;

    /// <summary>
    /// Bar Units Per Second (speed)
    /// </summary>
    [HideInInspector] public float barUPS;

    /// <summary>
    /// Whether the bar is currently going up (if false - it's going down).
    /// </summary>
    [HideInInspector] public bool up = true;

    float currentY;
    float FreezeTimer;
    bool Bounced;
    bool stopped;

    void Start()
    {
        Vector3 currScale = transform.localScale;
        currScale.x = StageState.ScreenWidth;
        transform.localScale = currScale;
        barUPS = StageState.SpawnAreaHeight * StageState.BPS;

        transform.position = Vector3.zero;
        stopped = false;
        currentY = 0;
        Bounced = false;
        up = true;
        FreezeTimer = NoteProperties.FadeInTime + NoteProperties.ScaleInTime;
        if (StageManager._debug > 0)
            Debug.Log($"Freezing the bar for {FreezeTimer} seconds");
    }

    /// <summary>
    /// Moves the bar up and down in sync with the beats of the music.
    /// </summary>
    void Update()
    {
        if (FreezeTimer > 0)
        {
            FreezeTimer -= Time.deltaTime;
            return;
        }
        if (stopped) return;
        Vector3 currPosition = transform.position;
        int sign = (up ? 1 : -1);
        float speed = sign * barUPS;
        if (FreezeTimer != 0)
        {
            currentY += speed * -FreezeTimer;
            FreezeTimer = 0;
        }
        currentY += speed * Time.deltaTime;
        if (Math.Abs(currentY) >= StageState.SpawnAreaHeight / 2)
        {
            currentY = sign * StageState.SpawnAreaHeight - currentY;
            up = !up;
            if (!Bounced)
            {
                float extraDistance = StageState.SpawnAreaHeight / 2 - currentY;
                float MusicStartDelay = extraDistance / barUPS;
                Bounced = true;
                StageManager.PlayMusic(MusicStartDelay);
            }
        }
        currPosition.y = currentY;
        transform.position = currPosition;
    }
    public void Stop()
    {
        stopped = true;
    }
    public void Restart()
    {
        Start();
    }
}
