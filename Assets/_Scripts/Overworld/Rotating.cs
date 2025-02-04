using UnityEngine;

/// <summary>
/// Script that makes a game object have rotating animation.
/// </summary>
public class Rotating : MonoBehaviour
{
    [SerializeField] float HorizontalRotationSpeed = 10;
    [SerializeField] float VerticalRotationSpeed = 10;
    [SerializeField] float ForwardRotationSpeed = 10;
    [Tooltip("If true will rotate around current pivot, otherwise will rotate around the center of the game object.")]
    [SerializeField] bool RotateAroundPivot = true;

    void Update()
    {
        if (RotateAroundPivot)
        {
            transform.Rotate(Vector3.right, VerticalRotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, HorizontalRotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.forward, ForwardRotationSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 center = GetComponent<Renderer>().bounds.center;
            transform.RotateAround(center, Vector3.right, VerticalRotationSpeed * Time.deltaTime);
            transform.RotateAround(center, Vector3.up, HorizontalRotationSpeed * Time.deltaTime);
            transform.RotateAround(center, Vector3.forward, ForwardRotationSpeed * Time.deltaTime);
        }
    }
}
