using TMPro;
using UnityEngine;

/// <summary>
/// A script for the cursor that represesnts the end of dialog-sentences.
/// </summary>
public class WaitCursorBehavior : MonoBehaviour
{
    [SerializeField] float BlinkTime = 0.3f;
    float timer = 0;

    void Update()
    {
        if (timer < BlinkTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            GetComponent<TMP_Text>().enabled = !GetComponent<TMP_Text>().enabled;
        }
    }

    /// <summary>
    /// Wrapper for gameObject.SetActive with additional actions.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive2(bool active)
    {
        timer = 0;
        gameObject.SetActive(active);
    }
}
