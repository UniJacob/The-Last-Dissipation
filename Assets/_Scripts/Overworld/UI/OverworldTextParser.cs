using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A parser for text files representing game dialogs.
/// </summary>
public class OverworldTextParser : MonoBehaviour
{
    [Tooltip("Path from 'Resources/Story_Texts/'.")]
    [SerializeField] string CurrentDialogSubFolder = "IntroDialogs";

    static OverworldTextParser instance;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
        {
            return;
        }
        LoadDialogs();
    }

    /// <summary>
    /// Parses all text files from Resources/Story_Texts/ as dialogs and stores them in OverworldState.
    /// The files are loaded alphabetically.
    /// </summary>
    /// <exception cref="Exception">If no texts are found</exception>
    void LoadDialogs()
    {
        OverworldState.DialogsMap = new Dictionary<string, TextAsset>();
        if (string.IsNullOrEmpty(CurrentDialogSubFolder)) return;

        TextAsset[] DialogsAssets = Resources.LoadAll<TextAsset>("Story_Texts/" + CurrentDialogSubFolder);
        if (DialogsAssets == null || DialogsAssets.Length == 0)
        {
            throw new Exception($"No text files found");
        }

        foreach (TextAsset asset in DialogsAssets)
        {
            OverworldState.DialogsMap[asset.name] = asset;
        }
    }
}
