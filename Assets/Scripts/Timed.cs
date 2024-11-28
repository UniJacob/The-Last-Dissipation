using UnityEngine;

public class Timed : MonoBehaviour
{
    [SerializeField] float lifeTime = 3;
    private float timer = 0;

    void Update()
    {
        if (timer > lifeTime)
        {
            try
            {
                GetComponent<NoteBehavior>().SetMissed();
            }
            catch (System.Exception)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
