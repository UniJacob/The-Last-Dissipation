using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// A script responsible for handling player input in stages.
/// </summary>
public class InputManagerStage : MonoBehaviour
{
    [Tooltip("Minimum distance between 2 different touches so that they wouldn't count as 1")]
    [SerializeField] float MinTouchDistance = 0.1f;
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject TouchFeedbackPrefab;

    [Tooltip("If greater than 0, debug logs will be enabled")]
    [SerializeField] int DebugLog = 0;

    Vector2 lastScreenPosTouched = new Vector2(Mathf.Infinity, Mathf.Infinity);
    static InputManagerStage instance;

    private void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
            return;
        EnhancedTouchSupport.Enable();
    }


    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.screenPosition == Vector2.positiveInfinity)
                {
                    continue;
                }
                if (Vector2.Distance(lastScreenPosTouched, touch.screenPosition) < MinTouchDistance)
                {
                    continue;
                }
                lastScreenPosTouched = touch.screenPosition;
                var worldPosTouched2D = mainCamera.ScreenToWorldPoint(lastScreenPosTouched);
                if (DebugLog > 0)
                {
                    Debug.Log($"Tapped at {worldPosTouched2D}");
                }
                if (TouchFeedbackPrefab != null && StageState.IsEnded)
                {
                    DisplayTouchFeedback();
                }
                CheckTouchedNoteAt(worldPosTouched2D);
            }
        }
    }

    /// <summary>
    /// Displays visual touch feedback.
    /// </summary>
    void DisplayTouchFeedback()
    {
        var pos = mainCamera.ScreenToWorldPoint(lastScreenPosTouched);
        if (DebugLog > 0)
        {
            Debug.Log($"Touch feedback at {pos}");
        }
        Instantiate(TouchFeedbackPrefab, pos, Quaternion.identity);
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
                CheckTouchedNoteAt(worldPosTouched2D);
            }
        }
    }

    /// <summary>
    /// Checks if there is a Note object in a given position, and make it "tapped".
    /// </summary>
    /// <param name="touchPosition">Position to check</param>
    void CheckTouchedNoteAt(Vector2 touchPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
        if (hit.collider != null)
        {
            var note = hit.collider.gameObject;
            note.GetComponent<NoteBehavior>().Tap();
        }
    }
}
