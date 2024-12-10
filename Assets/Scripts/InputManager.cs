using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] float MinTouchDistance = 1;
    Vector2 lastWorldPosTouched = new Vector2(Mathf.Infinity, Mathf.Infinity);
    void Update()
    {
        if (Touchscreen.current == null)
        {
            Debug.LogWarning("No touchscreen found");
            if (Input.GetMouseButtonDown(0))
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Vector2 MousePos = Mouse.current.position.ReadValue();
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(MousePos);
                    CheckIfTouchedObject(mousePosition);
                }
        }
        else
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                var touchState = touch.ReadValue();
                if (touchState.phase == UnityEngine.InputSystem.TouchPhase.Began)
                //|| touchState.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    Vector2 screenPos = touchState.position;
                    if (float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y))
                    {
                        continue;
                    }
                    var worldPosTouched = Camera.main.ScreenToWorldPoint(touchState.position);
                    if (Vector2.Distance(lastWorldPosTouched, worldPosTouched) < MinTouchDistance)
                    {
                        continue;
                    }
                    lastWorldPosTouched = worldPosTouched;
                    Debug.Log($"Touch Began at {worldPosTouched}");
                    CheckIfTouchedObject(worldPosTouched);
                }

                //    case UnityEngine.InputSystem.TouchPhase.Began:
                //    case UnityEngine.InputSystem.TouchPhase.Moved:
                //    case UnityEngine.InputSystem.TouchPhase.Stationary:
                //    case UnityEngine.InputSystem.TouchPhase.Ended:
                //    case UnityEngine.InputSystem.TouchPhase.Canceled:
            }
        }
    }
    void CheckIfTouchedObject(Vector2 touchPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
        if (hit.collider != null)
        {
            var note = hit.collider.gameObject;
            try
            {
                note.GetComponent<NoteBehavior>().WasTapped = true;
                note.GetComponent<CircleCollider2D>().enabled = false;
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"Object {name} doesn't have NoteBehavior component");
                //Destroy(gameObject);
                //throw;
            }
        }
    }
}
