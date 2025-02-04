using UnityEngine;

/// <summary>
/// Script that makes a game object have floating-like animation.
/// </summary>
public class Floating : MonoBehaviour
{
    [SerializeField] float HorizontalRange = 5;
    [SerializeField] float FloatingSpeed = 1;
    [Tooltip("Whether the object should start in a random height in [-HorizontalRange, HorizontalRange].")]
    [SerializeField] bool RandomStartHeight = true;
    float startY, randomStart = 0;

    void Start()
    {
        startY = transform.position.y;
        if (RandomStartHeight)
        {
            randomStart = Random.Range(0, 360);
        }
    }

    void Update()
    {
        float newY = startY + HorizontalRange * Mathf.Sin(randomStart + Time.time * FloatingSpeed);
        transform.position = new(transform.position.x, newY, transform.position.z);
    }
}
