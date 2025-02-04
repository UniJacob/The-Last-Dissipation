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
    [SerializeField] float MinimumViewingTime = 0.2f;

    [SerializeField] Image TutorialImage;
    [Tooltip("Path from the 'Resources' Folder.")]
    [SerializeField] string TutorialFolderPath;
    [SerializeField] Mode TriggerType;
    [SerializeField] bool DestroyGameObjectOnCompletion;

    enum Mode {Collider, Button};
    float timeLeft;
    Sprite[] images;
    int currentImageIndex = 0;

    void Update()
    {
        if (OverworldState.IsInMenu && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (TriggerType == Mode.Collider)
        {
            StartTutorial();
        }
        else
        {
            Debug.LogWarning("Tutorial Trigger without Collider mode tried to activate on collision");
        }
    }

    public void StartTutorial()
    {
        if (GameState.ExhaustedTutorials.Contains(TutorialFolderPath))
        {
            if (DestroyGameObjectOnCompletion)
            {
                Destroy(gameObject);
            }
            return;
        }
        GameState.ExhaustedTutorials.Add(TutorialFolderPath);

        images = Resources.LoadAll<Sprite>(TutorialFolderPath);
        System.Array.Sort(images, (a, b) => string.Compare(a.name, b.name)); // Sort the array alphabetically by name

        if (images.Length > 0)
        {
            TutorialImage.sprite = images[0];
        }
        else
        {
            Debug.LogError($"No images found in Resources/{TutorialFolderPath}");
        }
        timeLeft = MinimumViewingTime;

        TutorialCollider.enabled = false;
        OverworldState.IsInMenu = true;
        TutorialImage.gameObject.SetActive(true);
        TutorialImage.GetComponent<Button>().onClick.AddListener(NextImage);
    }

    /// <summary>
    /// Display the next image. If none is available, close the tutorial.
    /// </summary>
    void NextImage()
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
        if (DestroyGameObjectOnCompletion)
        {
            Destroy(gameObject);
        }
    }
}
