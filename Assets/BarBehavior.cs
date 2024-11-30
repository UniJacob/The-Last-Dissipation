using System.Collections;
using UnityEngine;

public class BarBehavior : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    private NoteProperties noteProperties;

    float barUPS; // Bar Units Per Second (speed)
    float currentY = 0;
    bool up = true, delayed = false, isFrozen = false;
    float timeWhenUnfrozen = 0;

    void Start()
    {
        noteProperties = stageManager.GetComponent<NoteProperties>();

        Vector3 currScale = transform.localScale;
        currScale.x = stageManager.StageWidth + stageManager.HorizontalPadding * 1.9f;
        transform.localScale = currScale;

        barUPS = stageManager.StageHeight * (stageManager.BPM / 60);

        float delayTime = noteProperties.FadeInTime + noteProperties.ScaleInTime;
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
        if (Mathf.Abs(currentY) > stageManager.StageHeight / 2)
        {
            currentY = sign * stageManager.StageHeight - currentY;
            up = !up;
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
