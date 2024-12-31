using UnityEngine;

/// <summary>
/// A script for tutorial triggers, responsible for their behavior.
/// Functions are self-explanatory.
/// </summary>
public class TutorialTriggerBehavior : MonoBehaviour
{
    [SerializeField] GameObject TutorialImage;
    [SerializeField] Collider TutorialCollider;
    [Tooltip("The minimum time that has to pass before the player can close the tutorial window.")]
    [SerializeField] float MinimumViewingTime;
    float timeLeft;

    void OnTriggerEnter(Collider other)
    {
        TutorialCollider.enabled = false;
        OverworldState.InTutorial = true;
        TutorialImage.SetActive(true);
    }
    void Start()
    {
        timeLeft = MinimumViewingTime;
    }

    void Update()
    {
        if (OverworldState.InTutorial && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
    }

    public void CloseTutorial()
    {
        if (timeLeft > 0) return;
        OverworldState.InTutorial = false;
        TutorialImage.SetActive(false);
        OverworldState.PlayerDestination = Vector3.positiveInfinity;
    }
}
