using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// A singleton script responsible for handling player inputs in the Overworld.
/// </summary>
public class InputManagerOverworld : MonoBehaviour
{
    static InputManagerOverworld instance;

    [SerializeField] Camera MainCamera;
    [SerializeField] Camera TouchFeedbackCamera;
    [Tooltip("Max distance to cast a ray from the camera when tapping (to check for ray collisions)")]
    [SerializeField] float MaxRayDistance = 300;
    [SerializeField] float MinTouchDistance = 0.1f;
    [SerializeField] GameObject Floor;
    [SerializeField] GameObject TouchFeedbackPrefab;
    [SerializeField] float TouchFeedbackDistance = 10;

    [Tooltip("If greater than 0, debug logs will be enabled")]
    [SerializeField] int DebugLog = 0;

    /// <summary>
    /// The LayerMask of the floor (and other objects).
    /// </summary>
    int physicalLayerMask;
    Vector2 lastScreenPosTouched = Vector2.positiveInfinity;

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
            return;
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        if (OverworldState.WebGLCompatibility)
        {
            InputSystem.EnableDevice(Mouse.current);
            if (DebugLog > 0)
            {
                Debug.Log("Using mouse");
            }
        }
        else if (DebugLog > 0)
        {
            Debug.Log("Using touchscreen");
        }

        physicalLayerMask = 1 << Floor.layer;
    }

    void Update()
    {
        bool updatePlayerDestination = true;
        if (OverworldState.WebGLCompatibility)
        {
            updatePlayerDestination = MouseUpdate();
        }
        else
        {
            updatePlayerDestination = TouchUpdate();
        }
        if (updatePlayerDestination)
        {
            UpdatePlayerDestination(lastScreenPosTouched);
        }
    }

    /// <summary>
    /// Handles screen touches and uses them to asign values such as PlayerDestination.
    /// <return>Whether a new touch was received.</return>
    /// </summary>
    bool TouchUpdate()
    {
        bool isNewTouch = false;
        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                DisplayTouchFeedback(touch.screenPosition);
            }
            else if (touch.phase != TouchPhase.Moved)
            {
                continue;
            }
            if (Vector2.Distance(lastScreenPosTouched, touch.screenPosition) < MinTouchDistance)
            {
                continue;
            }
            lastScreenPosTouched = touch.screenPosition;
            isNewTouch = true;

        }
        return isNewTouch;
    }

    /// <summary>
    /// Displays visual touch feedback.
    /// </summary>
    /// <param name="feedBackPos">2D position in which to display touch-animation</param>
    void DisplayTouchFeedback(Vector2 feedBackPos)
    {
        Vector3 tmp = feedBackPos;
        tmp.z = TouchFeedbackDistance;
        var pos = TouchFeedbackCamera.ScreenToWorldPoint(tmp);
        if (DebugLog > 0)
        {
            Debug.Log($"Touch feedback at {pos}");
        }
        Instantiate(TouchFeedbackPrefab, pos, Quaternion.identity);
    }

    /// <summary>
    /// Handles mouse clicks and uses them to asign values such as PlayerDestination.
    /// <return>Whether a new click was received.</return>
    /// </summary>
    bool MouseUpdate()
    {
        bool changed = false;
        if (Mouse.current.leftButton.isPressed)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                lastScreenPosTouched = Mouse.current.position.ReadValue();
                changed = true;
            }
        }
        return changed;
    }

    void UpdatePlayerDestination(Vector2 screenPosTouched)
    {
        if (GetPhysicalObjectTouchPosition(screenPosTouched, out Vector3 touchLocation, out GameObject touchedGO))
        {
            if (DebugLog > 0)
                Debug.Log($"Tapped on {touchedGO.name} in {touchLocation}");
            if (touchedGO.CompareTag(Floor.tag))
            {
                OverworldState.PlayerDestination = touchLocation;
            }
        }
    }

    /// <summary>
    /// Finds the corresponding 3D world-position to a given 2D screen-space position and checks whether it's "physical".
    /// "Physical" positions are positions that are directly on top of objects with a specific LayerMask.
    /// </summary>
    /// <param name="screenPosTouched">2D screen-space position</param>
    /// <param name="worldPositionTouched">Corresponding 3D world-position the function returns</param>
    /// <param name="GameObjectTouched">The GameObject that the position is part of</param>
    /// <returns>True if the position is "physical", false otherwise.</returns>
    bool GetPhysicalObjectTouchPosition(Vector2 screenPosTouched, out Vector3 worldPositionTouched, out GameObject GameObjectTouched)
    {
        worldPositionTouched = Vector3.positiveInfinity;
        GameObjectTouched = null;
        Ray ray = MainCamera.ScreenPointToRay(screenPosTouched);
        if (Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance, physicalLayerMask))
        {
            worldPositionTouched = hit.point;
            GameObjectTouched = hit.collider.gameObject;
            return true;
        }
        return false;
    }
}
