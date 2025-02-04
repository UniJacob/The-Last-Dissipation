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
    [SerializeField] Light LightComponent;
    [SerializeField] float FadeInTime = 0;

    bool rising = true;
    float timer;
    float fadeInTimer;
    void Start()
    {
        if (LightComponent == null)
        {
            LightComponent = GetComponent<Light>();
        }
        if (FadeInTime > 0)
        {
            fadeInTimer = FadeInTime;
            LightComponent.intensity = 0;
        }
        else
        {
            LightComponent.intensity = MinIntensity;
        }
        timer = 0;
    }

    void Update()
    {
        if (fadeInTimer > 0)
        {
            float normalized = fadeInTimer / FadeInTime;
            LightComponent.intensity = (1 - normalized) * MinIntensity;
            fadeInTimer -= Time.deltaTime;
            return;
        }
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
                LightComponent.intensity =
                    Mathf.Lerp(LightComponent.intensity, MaxIntensity, Time.deltaTime * IntensitySpeed);

                CheckIntensity(MaxIntensity, LerpMargin);
            }
            else
            {
                LightComponent.intensity =
                    Mathf.Lerp(LightComponent.intensity, MinIntensity, Time.deltaTime * IntensitySpeed);
                CheckIntensity(MinIntensity, LerpMargin);
            }
        }
        else
        {
            if (rising)
            {
                LightComponent.intensity =
                    Mathf.MoveTowards(LightComponent.intensity, MaxIntensity, Time.deltaTime * IntensitySpeed);
                CheckIntensity(MaxIntensity);
            }
            else
            {
                LightComponent.intensity =
                    Mathf.MoveTowards(LightComponent.intensity, MinIntensity, Time.deltaTime * IntensitySpeed);
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
        if (Mathf.Abs(LightComponent.intensity - desiredValue) <= lerpMargin)
        {
            rising = !rising;
            timer = TimeBetweenGradientChange;
        }
    }
}
