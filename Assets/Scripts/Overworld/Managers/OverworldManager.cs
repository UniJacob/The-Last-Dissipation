using UnityEngine;

/// <summary>
/// A script responsible for general management when the play is in the overworld.
/// </summary>
public class OverworldManager : MonoBehaviour
{
    [SerializeField] bool WebGLCompatibility = false;

    void Awake()
    {
        Application.targetFrameRate = OverworldState.FrameCap;
        OverworldState.WebGLCompatibility = WebGLCompatibility;
    }
}
