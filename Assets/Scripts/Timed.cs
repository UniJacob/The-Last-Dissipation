using UnityEngine;

public class Timed : MonoBehaviour
{
    private float lifeTime, timer = 0;

    private void Start()
    {
        lifeTime = GetComponent<NoteProperties>().LifeTime;
    }
    void Update()
    {
        if (timer >= lifeTime)
        {
            //GetComponent<NoteBehavior>().SetMissed();
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
