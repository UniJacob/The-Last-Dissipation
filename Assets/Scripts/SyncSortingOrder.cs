using UnityEngine;

/// <summary>
/// A script responsible for syncing the SortingOrder of the GameObject and its children.
/// </summary>
public class SyncSortingOrder : MonoBehaviour
{
    [Tooltip("If not 'int.MaxValue', this will be the new SortingOrder of the GameObject")]
    [SerializeField] int NewSortingOrder = int.MaxValue;

    void Start()
    {
        if (NewSortingOrder == int.MaxValue)
        {
            NewSortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        }
        SetSortingOrders();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetSortingOrders()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in spriteRenderers)
        {
            sprite.sortingOrder = NewSortingOrder;
        }
    }
}
