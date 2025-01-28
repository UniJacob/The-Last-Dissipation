using System;
using System.Collections;
using System.Collections.Generic;
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
    public static bool EnsureSingleton<T>(ref T instance, GameObject thisGameObject) where T : MonoBehaviour
    {
        if (instance != null && instance != thisGameObject.GetComponent<T>())
        {
            Debug.LogWarning($"A second instance of singleton {typeof(T).Name} on GameObject " +
                $"{thisGameObject.name} was detected and destroyed.");
            MonoBehaviour.Destroy(thisGameObject);
            return false;
        }
        instance = thisGameObject.GetComponent<T>();
        return true;
    }

    /// <summary>
    /// Parses a text file from the Resources folder into a dictionary of key-value pairs.
    /// The file should contain lines in the format "key = value".
    /// </summary>
    /// <param name="filename">Name of the text file in the Resources folder (without extension)</param>
    /// <returns>Dictionary containing the parsed key-value pairs. Throws an error if file is not found.</returns>
    public static Dictionary<string, string> ParseKeyValueFile(string filename)
    {
        var result = new Dictionary<string, string>();
        TextAsset textAsset = Resources.Load<TextAsset>(filename);

        if (textAsset == null)
        {
            throw new SystemException($"Failed to load file '{filename}'");
        }

        // Split the text into lines
        string[] lines = textAsset.text.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string line in lines)
        {
            // Skip empty lines or comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                continue;

            // Split each line at the equals sign
            string[] parts = line.Split(new[] { '=' }, 2, StringSplitOptions.None);

            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (!string.IsNullOrEmpty(key))
                {
                    if (result.ContainsKey(key))
                    {
                        Debug.LogWarning($"Duplicate key found in {filename}: {key}");
                        continue;
                    }

                    result.Add(key, value);
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format in {filename}: {line}");
            }
        }

        return result;
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

    /// <summary>
    /// A Serializable Dictionary class for when saving/loading a Dictionary to/from a JSON file is required.
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    /// <typeparam name="TValue">Type of the value</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // The actual dictionary
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        // Dictionary wrapper properties/methods
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;
        public int Count => dictionary.Count;

        public void Add(TKey key, TValue value) => dictionary.Add(key, value);
        public bool Remove(TKey key) => dictionary.Remove(key);
        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);
        public void Clear() => dictionary.Clear();

        // Convert back to regular dictionary if needed
        public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(dictionary);

        // Unity serialization
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            if (keys.Count != values.Count)
            {
                Debug.LogError($"Tried to deserialize SerializableDictionary but keys count ({keys.Count}) does not match values count ({values.Count})");
                return;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }
    }

    /// <summary>
    /// A Serializable HashSet class for when saving/loading a HashSet to/from a JSON file is required.
    /// </summary>
    /// <typeparam name="T">Type of the parameters</typeparam>
    [Serializable]
    public class SerializableHashSet<T> : ISerializationCallbackReceiver, ISet<T>
    {
        [SerializeField]
        private List<T> items = new List<T>();

        // The actual HashSet
        private HashSet<T> hashSet = new HashSet<T>();

        // HashSet wrapper properties
        public int Count => hashSet.Count;
        public bool IsReadOnly => false;

        // Constructor
        public SerializableHashSet()
        {
            hashSet = new HashSet<T>();
        }

        public SerializableHashSet(IEnumerable<T> collection)
        {
            hashSet = new HashSet<T>(collection);
        }

        // Core HashSet operations
        public bool Add(T item) => hashSet.Add(item);
        public void Clear() => hashSet.Clear();
        public bool Contains(T item) => hashSet.Contains(item);
        public bool Remove(T item) => hashSet.Remove(item);

        // Additional HashSet operations
        public void ExceptWith(IEnumerable<T> other) => hashSet.ExceptWith(other);
        public void IntersectWith(IEnumerable<T> other) => hashSet.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => hashSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => hashSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => hashSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => hashSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => hashSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => hashSet.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => hashSet.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => hashSet.UnionWith(other);

        // ICollection<T> implementation
        void ICollection<T>.Add(T item) => hashSet.Add(item);
        public void CopyTo(T[] array, int arrayIndex) => hashSet.CopyTo(array, arrayIndex);

        // IEnumerable implementation
        public IEnumerator<T> GetEnumerator() => hashSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => hashSet.GetEnumerator();

        // Convert back to regular HashSet if needed
        public HashSet<T> ToHashSet() => new HashSet<T>(hashSet);

        // Unity serialization
        public void OnBeforeSerialize()
        {
            items.Clear();
            foreach (T item in hashSet)
            {
                items.Add(item);
            }
        }

        public void OnAfterDeserialize()
        {
            hashSet.Clear();
            foreach (T item in items)
            {
                hashSet.Add(item);
            }
        }
    }
}
