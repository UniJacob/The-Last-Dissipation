using UnityEngine;

public class StageNoteSpawner : MonoBehaviour
{
    [SerializeField] GameObject StageManager;
    private StageManager SM;

    GameObject[][] Notes;
    int[] Weights;

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
            if (Notes[i] != null)
            {
                foreach (GameObject note in Notes[i])
                {
                    note.SetActive(true);
                    note.GetComponent<NoteBehavior>().TimeAtActivation = Time.time;
                }
            }
            float waitTime = SM.SPB / Weights[i] - BadDelay;
            await Awaitable.WaitForSecondsAsync(waitTime);
            BadDelay = Time.time - lastTime - waitTime;
            lastTime = Time.time;
        }
    }
}
