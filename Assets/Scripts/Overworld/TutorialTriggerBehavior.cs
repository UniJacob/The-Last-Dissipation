using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script for tutorial triggers, responsible for their behavior.
/// </summary>
public class TutorialTriggerBehavior : MonoBehaviour
{
    //[SerializeField] GameObject TutorialImage;
    [SerializeField] Collider TutorialCollider;
    [Tooltip("The minimum time that has to pass before the player can close the tutorial window.")]
    [SerializeField] float MinimumViewingTime;

    [SerializeField] Image TutorialImage;
    [Tooltip("Path from the 'Resources' Folder.")]
    [SerializeField] string TutorialFolderPath;

    float timeLeft;
    Sprite[] images;
    int currentImageIndex = 0;

    void OnTriggerEnter(Collider other)
    {
        if (GameState.ExhaustedTutorials.Contains(TutorialFolderPath)) { return; }
        GameState.ExhaustedTutorials.Add(TutorialFolderPath);

        TutorialCollider.enabled = false;
        OverworldState.IsInMenu = true;
        TutorialImage.gameObject.SetActive(true);
    }

    void Start()
    {
        // Load all sprites from the specified Resources folder
        images = Resources.LoadAll<Sprite>(TutorialFolderPath);

        // Sort the array alphabetically by name
        System.Array.Sort(images, (a, b) => string.Compare(a.name, b.name));

        if (images.Length > 0)
        {
            TutorialImage.sprite = images[0];
        }
        else
        {
            Debug.LogError($"No images found in Resources/{TutorialFolderPath}");
        }
        timeLeft = MinimumViewingTime;
    }

    void Update()
    {
        if (OverworldState.IsInMenu && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Display the next image. If none is available, close the tutorial.
    /// </summary>
    public void NextImage()
    {
        currentImageIndex++;
        if (currentImageIndex >= images.Length)
        {
            CloseTutorial();
            return;
        }
        TutorialImage.sprite = images[currentImageIndex];
        timeLeft = MinimumViewingTime;
    }

    void CloseTutorial()
    {
        if (timeLeft > 0) return;
        OverworldState.IsInMenu = false;
        OverworldState.PlayerDestination = Vector3.positiveInfinity;
        if (TutorialImage.GetComponent<CanvasFader>() != null)
        {
            TutorialImage.GetComponent<CanvasFader>().TriggerFadeOut();
        }
        else
        {
            TutorialImage.gameObject.SetActive(false);
        }
        GameState.SaveGameState();
    }
}
