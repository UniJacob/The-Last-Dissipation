using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script responsible for overworld-portals functions.
/// </summary>
public class PortalBehavior : MonoBehaviour
{
    public string StageFileName;
    public string NewSceneName;
    [SerializeField] SceneChanger SceneChanger;

    /// <summary>
    /// Perform relevant actions when the portal has been entered (by the player).
    /// </summary>
    public void Enter()
    {
        if (!string.IsNullOrEmpty(StageFileName))
        {
            StageState.StageFileName = StageFileName;
            //SceneManager.LoadScene(StageState.StageSceneName);
            SceneChanger.ChangeScene(StageState.StageSceneName);
            return;
        }
        else if (!string.IsNullOrEmpty(NewSceneName))
        {
            //SceneManager.LoadScene(NewSceneName);
            SceneChanger.ChangeScene(NewSceneName);
            return;
        }
    }
}
