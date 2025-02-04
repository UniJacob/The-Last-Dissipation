using UnityEngine;

/// <summary>
/// A singleton script responsible for managing map-related stuff.
/// </summary>
public class MapManager : MonoBehaviour
{
    static MapManager instance;

    [Header("GameObjects and the progression needed to unlock them ")]
    [SerializeField] GameObject[] LockedByProgression;
    [SerializeField] float[] ProgressionRequired;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject)) return;
    }

    void OnValidate()
    {
        if (LockedByProgression.Length != ProgressionRequired.Length)
            Debug.LogError($"Lengths of LockedByProgression and ProgressionRequired must be the same");
    }

    /// <summary>
    /// Enables/disables certain GameObjects based on the progression in the game.
    /// </summary>
    public void UpdateMap()
    {
        for (int i = 0; i < LockedByProgression.Length; i++)
        {
            if (GameState.TotalGameProgression >= ProgressionRequired[i])
            {
                LockedByProgression[i].SetActive(true);
            }
            else
            {
                LockedByProgression[i].SetActive(false);
            }
        }
    }
}
