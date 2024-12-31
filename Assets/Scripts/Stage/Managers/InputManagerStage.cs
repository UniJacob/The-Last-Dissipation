using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// A script responsible for handling player input in stages.
/// </summary>
public class InputManagerStage : MonoBehaviour
{
    [Tooltip("Minimum distance between 2 different touches so that they wouldn't count as 1")]
    [SerializeField] float MinTouchDistance = 0.1f;
    [SerializeField] Camera mainCamera;

    Vector2 lastScreenPosTouched = new Vector2(Mathf.Infinity, Mathf.Infinity);
    static InputManagerStage instance;

    private void Awake()
    {
        Auxiliary.AssureSingleton(ref instance, gameObject);
    }


    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        //touchScreenPresent = !OverworldState.WebGLCompatibility;
        //if (!touchScreenPresent)
        if (OverworldState.WebGLCompatibility)
        {
            InputSystem.EnableDevice(Mouse.current);
            Debug.Log("Using mouse");
        }
        else
        {
            Debug.Log("Using touchscreen");
        }
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
    /// Handles screen touches.
    /// </summary>
    private void TouchUpdate()
    {
        foreach (var touch in Touchscreen.current.touches)
        {
            var touchState = touch.ReadValue();
            if (touchState.phase == TouchPhase.Began)
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
                var worldPosTouched2D = mainCamera.ScreenToWorldPoint(screenPosTouched);
                Debug.Log($"Tapped at {worldPosTouched2D}");
                TouchedNoteAt(worldPosTouched2D);
            }
            //    case UnityEngine.InputSystem.TouchPhase.Began:
            //    case UnityEngine.InputSystem.TouchPhase.Moved:
            //    case UnityEngine.InputSystem.TouchPhase.Stationary:
            //    case UnityEngine.InputSystem.TouchPhase.Ended:
            //    case UnityEngine.InputSystem.TouchPhase.Canceled:
        }
    }

    /// <summary>
    /// Handles mouse clicks.
    /// </summary>
    private void MouseUpdate()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 screenPosTouched = Mouse.current.position.ReadValue();
                var worldPosTouched2D = mainCamera.ScreenToWorldPoint(screenPosTouched);
                Debug.Log($"Clicked at {worldPosTouched2D}");
                TouchedNoteAt(worldPosTouched2D);
            }
        }
    }

    /// <summary>
    /// Checks if there is a Note object in a given position, and make it "tapped".
    /// </summary>
    /// <param name="touchPosition">Position to check</param>
    void TouchedNoteAt(Vector2 touchPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
        if (hit.collider != null)
        {
            var note = hit.collider.gameObject;
            note.GetComponent<NoteBehavior>().Tap();
        }
    }
}
