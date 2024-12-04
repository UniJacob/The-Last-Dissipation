using UnityEngine;

public class StageNoteSpawner : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private StageManager SM;

    GameObject[][] Notes;
    int[] Weights;

    //public static double extraDelay = 0;

    private void Awake()
    {
        SM = StageManager.GetComponent<StageManager>();
        Notes = SM.Notes;
        Weights = SM.Weights;
    }

    void Start()
    {
        BeginStage();
    }

    private async void BeginStage()
    {
        float lastTime = 0, BadDelay = 0;
        for (int i = 0; i < Notes.Length; ++i)
        {
            //float timeBeforeWait = Time.time;
            if (Notes[i] != null)
            {
                foreach (GameObject note in Notes[i])
                {
                    note.SetActive(true);
                    note.GetComponent<NoteBehavior>().TimeAtActivation = Time.time;
                }
            }
            float waitTime = SM.SPB / Weights[i] - BadDelay;
            //float unwantedDelay = NoteBehavior.timeErrorCounter > 0 ? NoteBehavior.timeError / NoteBehavior.timeErrorCounter : 0;
            //NoteBehavior.timeErrorCounter = 0;
            //NoteBehavior.timeError = 0;
            //Debug.LogError(waitTime);
            await Awaitable.WaitForSecondsAsync(waitTime);
            BadDelay = Time.time - lastTime - waitTime;
            lastTime = Time.time;
            //multiplier += 0.001f;
            //badDelay = Time.time - timeBeforeWait - waitTime;
            //await Awaitable.WaitForSecondsAsync(waitTime - badDelay - unwantedDelay);
            //Debug.LogError("badDelay = " + badDelay);
        }
    }
}
