using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A script resposible for the movement of the player.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] bool DisableInDialog = true;
    [SerializeField] NavMeshAgent PlayerNavMeshAgent;

    void Start()
    {
        transform.position = GameState.LastPlayerPosition;
    }

    void Update()
    {
        if (DisableInDialog && OverworldState.IsInDialog)
        {
            OverworldState.PlayerDestination = Vector3.positiveInfinity;
        }
        else if (OverworldState.IsInMenu || OverworldState.IsInAnimation)
        {
            OverworldState.PlayerDestination = Vector3.positiveInfinity;
        }
        AdvanceTowardsDestination(OverworldState.PlayerDestination);
    }

    /// <summary>
    /// Makes the player advance towards a given destination.
    /// <param name="destination">Given destination vector</param>
    /// <param name="hasCorrectY">Whether the destination takes the height of this object into cosideration.</param>
    void AdvanceTowardsDestination(Vector3 destination, bool hasCorrectY = false)
    {
        if (Vector3.Equals(destination, Vector3.positiveInfinity))
        {
            PlayerNavMeshAgent.destination = transform.position;
            return;
        }
        PlayerNavMeshAgent.destination = destination;
    }
}
