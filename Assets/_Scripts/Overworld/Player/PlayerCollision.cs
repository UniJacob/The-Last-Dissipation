using UnityEngine;

/// <summary>
/// A script responsible for handling the player's collisions and triggers.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    /// <summary>
    /// Handles player triggers, such as when touching portals.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        go.GetComponent<PortalBehavior>()?.Enter();
        go.GetComponent<GateBehavior>()?.ActivateGate();
    }
}
