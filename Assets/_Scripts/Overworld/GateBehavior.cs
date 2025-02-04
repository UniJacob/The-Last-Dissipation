using UnityEngine;

public class GateBehavior : MonoBehaviour
{
    [SerializeField] string AssociatedSceneName;
    [SerializeField] Vector3 NewPlayerPosition;

    public void ActivateGate()
    {
        if (OverworldState.IsInDialog || OverworldState.IsInMenu) return;

        GameState.LastOverworldSceneName = AssociatedSceneName;
        GameState.LastPlayerPosition = NewPlayerPosition;
        SceneChanger.instance.StartSceneChange(AssociatedSceneName);
    }
}
