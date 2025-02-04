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
        BlackScreenImage.color = BlackScreenColor;
        BlackScreenFadeInSpeed = 1 / BlackScreenFadeInTime;
        BlackScreenFadeOutSpeed = 1 / BlackScreenFadeOutTime;

        DontDestroyOnLoad(gameObject);
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
                if (SceneManager.GetActiveScene().name != NewSceneName)
                {
                    //Vector3 tmp = Vector3.positiveInfinity;
                    //if (OverworldManager.instance != null) // If overworld before scene change exists
                    //{
                        //    if (!OverworldState.LastPlayerPosition.Equals(Vector3.positiveInfinity))
                        //    {
                        //        tmp = GameState.LastPlayerPosition; // tmp is new position
                        //        GameState.LastPlayerPosition = OverworldState.LastPlayerPosition;
                        //    }

                    //}
                    if (NewSceneName == StageState.StageSceneName)
                    {
                        if (OverworldManager.instance != null) // If overworld before scene change exists
                        {
                            GameState.LastPlayerPosition = OverworldManager.instance.Player.transform.position;
                        }
                    }
                    SceneManager.LoadScene(NewSceneName);
                    if (OverworldManager.instance != null) // If overworld after scene change exists
                    {
                        OverworldManager.instance.Player.transform.position = GameState.LastPlayerPosition;
                    }
                    GameState.SaveGameState();
                }
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
    /// <param name="FadeOutToBlack">Whether to fade out to black before fading in (or immediately 
    /// fade in from black)</param>
    public void StartSceneChange(string newSceneName, bool FadeOutToBlack = true)
    {
        NewSceneName = newSceneName;
        BlackScreenFadeIn = true;
        isChangingScene = true;
        if (!FadeOutToBlack)
        {
            BlackScreenCanvasGroup.alpha = 1;
        }
    }
}
