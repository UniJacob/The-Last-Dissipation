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

    static SceneChanger instance;
    bool BlackScreenFadeIn = false;
    bool BlackScreenFadeOut = false;
    float BlackScreenFadeInSpeed;
    float BlackScreenFadeOutSpeed;
    string newSceneName = "";

    void Awake()
    {
        if (!Auxiliary.AssureSingleton(ref instance, gameObject))
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
        if (BlackScreenFadeIn)
        {
            BlackScreenCanvasGroup.alpha += BlackScreenFadeInSpeed * Time.deltaTime;
            if (BlackScreenCanvasGroup.alpha >= 1)
            {
                BlackScreenCanvasGroup.alpha = 1;
                BlackScreenFadeIn = false;
                SceneManager.LoadScene(newSceneName);
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
    }

    /// <summary>
    /// Changes to a scene given it's name with fade in/out effect.
    /// </summary>
    /// <param name="sceneName">Name of the new scene</param>
    public void ChangeScene(string sceneName)
    {
        newSceneName = sceneName;
        BlackScreenFadeIn = true;
    }
}
