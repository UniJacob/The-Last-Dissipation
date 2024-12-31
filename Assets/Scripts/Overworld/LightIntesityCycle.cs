using UnityEngine;

/// <summary>
/// A script that increases and decreases the light's intensity in a cycle.
/// </summary>
public class LightIntesityCycle : MonoBehaviour
{
    [SerializeField] float MaxIntensity = 300, MinIntensity = 100, IntensitySpeed = 75;
    [SerializeField] float LerpMargin = 30;
    [SerializeField] float TimeBetweenGradientChange = 2;
    [SerializeField] bool Lerp = false;
    bool rising = true;
    float timer;
    void Start()
    {
        GetComponent<Light>().intensity = MinIntensity;
        timer = 0;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }
        GradualChangeIntensity();
    }

    /// <summary>
    /// Gradualy dims or srengthen the light (using its intensity property).
    /// </summary>
    void GradualChangeIntensity()
    {
        if (Lerp)
        {
            if (rising)
            {
                GetComponent<Light>().intensity =
                    Mathf.Lerp(GetComponent<Light>().intensity, MaxIntensity, Time.deltaTime * IntensitySpeed);

                CheckIntensity(MaxIntensity, LerpMargin);
            }
            else
            {
                GetComponent<Light>().intensity =
                    Mathf.Lerp(GetComponent<Light>().intensity, MinIntensity, Time.deltaTime * IntensitySpeed);
                CheckIntensity(MinIntensity, LerpMargin);
            }
        }
        else
        {
            if (rising)
            {
                GetComponent<Light>().intensity =
                    Mathf.MoveTowards(GetComponent<Light>().intensity, MaxIntensity, Time.deltaTime * IntensitySpeed);
                CheckIntensity(MaxIntensity);
            }
            else
            {
                GetComponent<Light>().intensity =
                    Mathf.MoveTowards(GetComponent<Light>().intensity, MinIntensity, Time.deltaTime * IntensitySpeed);
                CheckIntensity(MinIntensity);
            }
        }
    }

    /// <summary>
    /// Checks if the light intensity has reached its desired value and updates class variables if so.
    /// </summary>
    /// <param name="condition"></param>
    void CheckIntensity(float desiredValue, float lerpMargin = 0)
    {
        if (Mathf.Abs(GetComponent<Light>().intensity - desiredValue) <= lerpMargin)
        {
            rising = !rising;
            timer = TimeBetweenGradientChange;
        }
    }
}
