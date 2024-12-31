using System;
using UnityEngine;

/// <summary>
/// Provides auxiliary functions for other scripts to use.
/// </summary>
public static class Auxiliary
{
    /// <summary>
    /// Ensures that a MonoBehaviour script enforces the singleton pattern.
    /// Destroys the GameObject if another instance already exists.
    /// </summary>
    /// <typeparam name="T">Type of the MonoBehaviour singleton</typeparam>
    /// <param name="instance">Reference to the current instance field of the singleton</param>
    /// <param name="thisGameObject">The GameObject hosting the script</param>
    /// <returns>False if thisGameObject has been destroyed (since it's not the 1st instance), true otherwise.</returns>
    public static bool AssureSingleton<T>(ref T instance, GameObject thisGameObject) where T : MonoBehaviour
    {
        if (instance != null && instance != thisGameObject.GetComponent<T>())
        {
            Debug.LogWarning($"A second instance of singleton {typeof(T).Name} on GameObject {thisGameObject.name} was detected and destroyed.");
            MonoBehaviour.Destroy(thisGameObject);
            return false;
        }
        instance = thisGameObject.GetComponent<T>();
        return true;
    }

    /// <summary>
    /// Converts a TextAsset to a string array, such that every element in the array is a sentence.
    /// Assumes two different sentences are seperated by \n or \r\n.
    /// </summary>
    /// <param name="textAsset"></param>
    /// <returns></returns>
    public static string[] TextAssetToSentences(ref TextAsset textAsset)
    {
        return textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }
}
