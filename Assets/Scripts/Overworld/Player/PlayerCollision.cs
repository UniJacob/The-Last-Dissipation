using UnityEngine;

/// <summary>
/// A script responsible for handling the player's collisions and triggers.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    /// <summary>
    /// Handles player triggers, such as when it touches a portal.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        //if (go.CompareTag(PortalTag))
        //if (go.GetComponent<PortalBehavior>() != null)
        //{
        //    go.GetComponent<PortalBehavior>().Enter();
        //}
        go.GetComponent<PortalBehavior>()?.Enter();
    }
}
