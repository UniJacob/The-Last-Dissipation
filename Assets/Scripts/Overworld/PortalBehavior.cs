using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script responsible for overworld-portals functions.
/// </summary>
public class PortalBehavior : MonoBehaviour
{
    [SerializeField] OverworldManager OverworldManager;
    [SerializeField] int AssociatedChapterNumber;

    /// <summary>
    /// Perform relevant actions when the portal has been entered (by the player).
    /// </summary>
    public void Enter()
    {
        OverworldManager.UnlockChapter(AssociatedChapterNumber);
        OverworldManager.OpenStageSelection();
    }
}
