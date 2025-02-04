using System;
using UnityEngine;

/// <summary>
/// A script responsible for the stage-bar behavior and timing.
/// </summary>
public class BarBehavior : MonoBehaviour
{
    [SerializeField] StageManager StageManager;
    [SerializeField] NoteProperties NoteProperties;

    [Tooltip("If greater than 0, debug logs will be enabled")]
    [SerializeField] float _DebugLog = 1;

    /// <summary>
    /// Whether the bar is currently going up (if false - it's going down).
    /// </summary>
    bool up = true;
    float currentY;
    float FreezeTimer;
    bool stopped;
    float barXScaleRatio = 0.93f;

    void Start()
    {
        Vector3 currScale = transform.localScale;
        currScale.x = StageState.ScreenWidth * barXScaleRatio;
        transform.localScale = currScale;

        transform.position = new Vector3(0,0, transform.position.z);
        stopped = false;
        currentY = 0;
        up = true;
        FreezeTimer = NoteProperties.FadeInTime + NoteProperties.ScaleInTime;
        if (GameState.StageMusicDelay < 0)
        {
            FreezeTimer += -GameState.StageMusicDelay;
        }
        if (_DebugLog > 0)
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
        float speed = sign * StageState.BarSpeed;
        if (FreezeTimer < 0)
        {
            currentY += speed * -FreezeTimer;
            FreezeTimer = 0;
        }
        currentY += speed * Time.deltaTime;
        if (Math.Abs(currentY) >= StageState.SpawnAreaHeight / 2)
        {
            currentY = sign * StageState.SpawnAreaHeight - currentY;
            up = !up;
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
