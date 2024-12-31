using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A parser for text files representing game dialogs.
/// </summary>
public class OverworldTextParser : MonoBehaviour
{
    [SerializeField] string StoryTextsFolderName = "Story_Texts";
    [Tooltip("Path from Resources/Story_Texts/")]
    [SerializeField] string StageTxtsFolder = "Scene1 (Overworld1)";

    static OverworldTextParser instance;

    void Awake()
    {
        if (Auxiliary.AssureSingleton(ref instance, gameObject))
        {
            LoadDialogs();
        }
    }

    /// <summary>
    /// Parses all text files from Resources/Story_Texts/ as dialogs and stores them in OverworldState.
    /// The files are loaded alphabetically.
    /// </summary>
    /// <exception cref="Exception">If no texts are found</exception>
    void LoadDialogs()
    {
        TextAsset[] DialogsAssets = Resources.LoadAll<TextAsset>(StoryTextsFolderName + "/" + StageTxtsFolder);
        if (DialogsAssets == null || DialogsAssets.Length == 0)
        {
            throw new Exception($"No text files found");
        }

        OverworldState.DialogsMap = new Dictionary<string, TextAsset>();
        foreach (TextAsset asset in DialogsAssets)
        {
            OverworldState.DialogsMap[asset.name] = asset;
        }
    }
}
