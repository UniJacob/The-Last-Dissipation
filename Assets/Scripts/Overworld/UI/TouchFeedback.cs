using UnityEngine;

/// <summary>
/// Script responsible for visual touch-feedback animation/particles.
/// </summary>
public class TouchFeedback : MonoBehaviour
{
    [SerializeField] ParticleSystem TouchAnimation;

    void Start()
    {
        Destroy(TouchAnimation.gameObject, TouchAnimation.main.duration + TouchAnimation.main.startLifetime.constantMax);
    }
}
