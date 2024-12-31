using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// A singleton script responsible for handling player inputs in the Overworld.
/// </summary>
public class InputManagerOverworld : MonoBehaviour
{
    static InputManagerOverworld instance;

    [SerializeField] Camera MainCamera;
    [Tooltip("Max distance to cast a ray from the camera when tapping (to check for ray collisions)")]
    [SerializeField] float MaxRayDistance = 300;
    [SerializeField] float MinTouchDistance = 0.1f;
    [SerializeField] GameObject Floor;

    [Tooltip("If greater than 0, debug logs will be enabled")]
    [SerializeField] int DebugLog = 0;

    /// <summary>
    /// The LayerMask of the floor (and other objects).
    /// </summary>
    int physicalLayerMask;
    Vector2 lastScreenPosTouched = new Vector2(Mathf.Infinity, Mathf.Infinity);

    private void Awake()
    {
        Auxiliary.AssureSingleton(ref instance, gameObject);
    }
    private void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        //touchScreenPresent = Touchscreen.current != null;
        //if (OverworldState.WebGLCompatibility)
        //{
        //    Debug.LogWarning("Using Mouse input");
        //}
        if (OverworldState.WebGLCompatibility)
        {
            InputSystem.EnableDevice(Mouse.current);
            Debug.Log("Using mouse");
        }
        else
        {
            Debug.Log("Using touchscreen");
        }

        physicalLayerMask = 1 << Floor.layer;
    }

    void Update()
    {
        if (OverworldState.WebGLCompatibility)
        {
            MouseUpdate();
        }
        else
        {
            TouchUpdate();
        }
    }

    /// <summary>
    /// Handles screen touches and uses them to asign values such as PlayerDestination.
    /// </summary>
    private void TouchUpdate()
    {
        foreach (var touch in Touchscreen.current.touches)
        {
            var touchState = touch.ReadValue();
            if (touchState.phase == TouchPhase.Began || touchState.phase == TouchPhase.Moved)
            {
                var screenPosTouched = touchState.position;
                if (float.IsInfinity(screenPosTouched.x) || float.IsInfinity(screenPosTouched.y))
                {
                    continue;
                }
                if (Vector2.Distance(lastScreenPosTouched, screenPosTouched) < MinTouchDistance)
                {
                    continue;
                }
                lastScreenPosTouched = screenPosTouched;
                UpdatePlayerDestination(screenPosTouched);

            }
            //    case UnityEngine.InputSystem.TouchPhase.Began:
            //    case UnityEngine.InputSystem.TouchPhase.Moved:
            //    case UnityEngine.InputSystem.TouchPhase.Stationary:
            //    case UnityEngine.InputSystem.TouchPhase.Ended:
            //    case UnityEngine.InputSystem.TouchPhase.Canceled:
        }
    }

    /// <summary>
    /// Handles mouse clicks and uses them to asign values such as PlayerDestination.
    /// </summary>
    private void MouseUpdate()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 screenPosTouched = Mouse.current.position.ReadValue();
                UpdatePlayerDestination(screenPosTouched);
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
}
