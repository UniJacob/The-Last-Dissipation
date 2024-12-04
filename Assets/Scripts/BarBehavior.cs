using System;
using System.Collections;
using UnityEngine;

public class BarBehavior : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    private NoteProperties noteProperties;

    [HideInInspector] public float barUPS; // Bar Units Per Second (speed)
    float currentY = 0;
    [HideInInspector] public bool up = true;
    //bool up = true;
    bool isFrozen = false;
    float delayTime, timeWhenUnfrozen = 0;

    float tst = 0, tst1 = 0;
    bool tst2 = false;
    [HideInInspector] public int bounces = 0;

    private void Awake()
    {
        noteProperties = stageManager.GetComponent<NoteProperties>();

        Vector3 currScale = transform.localScale;
        currScale.x = stageManager.StageWidth + stageManager.HorizontalPadding * 1.9f;
        transform.localScale = currScale;

        barUPS = stageManager.StageHeight * (stageManager.BPM / 60);

        delayTime = noteProperties.FadeInTime + noteProperties.ScaleInTime;
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
        if (Math.Abs(currentY) >= stageManager.StageHeight / 2)
        {
            currentY = sign * stageManager.StageHeight - currentY;
            up = !up;


            ++bounces;
            //Debug.LogError("Bar beat time: " + (Time.time - tst) + ", real beat time: " + 1 / (stageManager.BPM / 60));
            //float timeDelay = ((Time.time - tst) - 1 / (stageManager.BPM / 60));
            //if (tst2)
            //{
            //    tst1 += timeDelay;
            //}
            //else
            //{
            //    tst2 = true;
            //}
            //Debug.LogError("timeDelay " + timeDelay + ", timeDelay total " + tst1);
            //tst = Time.time;
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
