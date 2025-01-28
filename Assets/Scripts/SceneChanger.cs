using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A singleton script responsible for changing scenes (plus fade in/out animations).
/// </summary>
public class SceneChanger : MonoBehaviour
{
    [SerializeField] CanvasGroup BlackScreenCanvasGroup;
    [SerializeField] UnityEngine.UI.Image BlackScreenImage;
    [SerializeField] float BlackScreenFadeInTime = 1;
    [SerializeField] float BlackScreenFadeOutTime = 1;
    [SerializeField] Color BlackScreenColor = Color.black;

    public static SceneChanger instance;
    bool isChangingScene = false;
    bool BlackScreenFadeIn = false;
    bool BlackScreenFadeOut = false;
    float BlackScreenFadeInSpeed;
    float BlackScreenFadeOutSpeed;
    string NewSceneName = "";

    void Awake()
    {
        if (!Auxiliary.EnsureSingleton(ref instance, gameObject))
        {
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        BlackScreenImage.color = BlackScreenColor;
        BlackScreenFadeInSpeed = 1 / BlackScreenFadeInTime;
        BlackScreenFadeOutSpeed = 1 / BlackScreenFadeOutTime;
    }
    void Update()
    {
        if (isChangingScene)
        {
            FadeToNewScene();
        }
    }

    void FadeToNewScene()
    {
        if (BlackScreenFadeIn)
        {
            BlackScreenCanvasGroup.alpha += BlackScreenFadeInSpeed * Time.deltaTime;
            if (BlackScreenCanvasGroup.alpha >= 1)
            {
                BlackScreenCanvasGroup.alpha = 1;
                BlackScreenFadeIn = false;
                GameState.SaveGameState();
                SceneManager.LoadScene(NewSceneName);
                BlackScreenFadeOut = true;
            }
        }
        else if (BlackScreenFadeOut)
        {
            BlackScreenCanvasGroup.alpha -= BlackScreenFadeOutSpeed * Time.deltaTime;
            if (BlackScreenCanvasGroup.alpha <= 0)
            {
                BlackScreenCanvasGroup.alpha = 0;
                BlackScreenFadeOut = false;
            }
        }
        else
        {
            isChangingScene = false;
        }
    }

    /// <summary>
    /// Changes to a new scene with fade in/out effect.
    /// </summary>
    /// <param name="newSceneName">Name of the new scene</param>
    public void StartSceneChange(string newSceneName)
    {
        NewSceneName = newSceneName;
        BlackScreenFadeIn = true;
        isChangingScene = true;
    }
}
