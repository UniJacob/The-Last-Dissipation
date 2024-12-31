using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which provides information regarding the state of the current overworld.
/// </summary>
public static class OverworldState
{
    /// <summary>
    /// Should be true when building for WebGL: disables text animation and switches to mouse input.
    /// </summary>
    public static bool WebGLCompatibility = false;

    /// <summary>
    /// FPS cap when in the overworld.
    /// </summary>
    public const int FrameCap = 120;

    /// <summary>
    /// Whether a dialog is currently occuring.
    /// </summary>
    public static bool InDialog = false;

    /// <summary>
    /// Whether a tutorial is currently displayed.
    /// </summary>
    public static bool InTutorial = false;

    /// <summary>
    /// Current destination of the player (position on the ground).
    /// </summary>
    public static Vector3 PlayerDestination = Vector3.positiveInfinity;

    /// <summary>
    /// Maps dialog TextAssets to their names.
    /// </summary>
    public static Dictionary<string, TextAsset> DialogsMap;
}
