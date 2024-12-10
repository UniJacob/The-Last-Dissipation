using System;
using System.Collections;
using UnityEngine;

public class BarBehavior : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    private NoteProperties NP;

    /// <summary>
    /// Bar Units Per Second (speed)
    /// </summary>
    [HideInInspector] public float barUPS;
    [HideInInspector] public bool up = true;
    float currentY = 0;
    bool isFrozen = false;
    float delayTime, timeWhenUnfrozen = 0;

    /* Debug */
    private int bounces = 0;
    private bool playing = false;

    private void Awake()
    {
        NP = stageManager.GetComponent<NoteProperties>();
        Vector3 currScale = transform.localScale;
        currScale.x = stageManager.Width;
        transform.localScale = currScale;

        barUPS = stageManager.SpawnHeight * stageManager.BPS;
        delayTime = NP.FadeInTime + NP.ScaleInTime;
    }

    void Start()
    {
        StartCoroutine(FreezeCoroutine(delayTime));
    }

    void Update()
    {
        if (isFrozen) return;
        Vector3 currPosition = transform.position;
        int sign = (up ? 1 : -1);
        float speed = sign * barUPS;
        if (timeWhenUnfrozen > 0)
        {
            float unfrozenDeltaTime = Time.time - timeWhenUnfrozen; // Time from when the bar was unfrozen to the current frame time
            timeWhenUnfrozen = 0;
            currentY += speed * unfrozenDeltaTime;
        }
        currentY += speed * Time.deltaTime;
        if (Math.Abs(currentY) >= stageManager.SpawnHeight / 2)
        {
            currentY = sign * stageManager.SpawnHeight - currentY;
            up = !up;
            ++bounces;
            if (!playing)
            {
                playing = true;
                stageManager.PlayMusic();
            }
        }
        currPosition.y = currentY;
        transform.position = currPosition;
    }

    private IEnumerator FreezeCoroutine(float freezeTime)
    {
        isFrozen = true;
        yield return new WaitForSeconds(freezeTime);
        isFrozen = false;
        timeWhenUnfrozen = Time.time;
        Debug.Log($"Bar was freezed for {freezeTime} seconds");
    }
}
