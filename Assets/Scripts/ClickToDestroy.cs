using UnityEngine;

public class ClickToDestroy : MonoBehaviour
{
    private int debug = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (debug > 0)
                    {
                        Debug.Log("Object clicked: " + gameObject.name);
                    }
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
}
