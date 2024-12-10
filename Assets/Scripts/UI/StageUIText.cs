using TMPro;
using UnityEngine;

public class StageUIText : MonoBehaviour
{
    [SerializeField] StageManager SM;
    public TMP_Text StageName, DiffLevel;

    void Start()
    {
        StageName.text = SM.StageFilePath.Substring(SM.StageFilePath.IndexOf('/') + 1);
        DiffLevel.text = "?";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
