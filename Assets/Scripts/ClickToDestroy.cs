using UnityEngine;

public class ClickToDestroy : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                try
                {
                    GetComponent<NoteBehavior>().WasTapped = true;
                }
                catch (System.Exception)
                {
                    Destroy(gameObject);
                    throw;
                }
            }
        }
    }
}
