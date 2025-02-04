using TMPro;
using UnityEngine;

/// <summary>
/// A script responsible for dynamically changing text elements in the HUD.
/// </summary>
public class HUDTextSetter : MonoBehaviour
{
    [SerializeField] ScoreManager ScoreManager;

    [SerializeField] TMP_Text StageName;
    [SerializeField] TMP_Text DiffLevel;

    [SerializeField] TMP_Text Miss;
    [SerializeField] TMP_Text Bad;
    [SerializeField] TMP_Text Good;
    [SerializeField] TMP_Text Perfect;

    [SerializeField] TMP_Text ScoreText;
    [SerializeField] float ScoreAnimationDuration = 2f;
    [SerializeField] float ScoreAnimationDelay = 2f;
    [Tooltip("e.g., \"0.00\" for 2 decimal places")]
    [SerializeField] string NumberFormat = "0.000";
    [SerializeField] AnimationCurve ScoreAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] RectTransform TpBar;

    float finalScore;
    float currentTime;
    bool isAnimating;
    float ScoreAnimationTimer;
    float tpBarMaxWidth;
    RectTransform parentRect;

    void Start()
    {
        if (StageState.InAdjustDelayMode)
        {
            StageName.text = "Adjusting Music Delay";
            DiffLevel.text = "";
        }
        else
        {
            StageName.text = StageState.StageFileName.Substring(StageState.StageFileName.IndexOf('/') + 1);
            DiffLevel.text = "Difficulty " + StageState.StageDifficultyLevel;
        }
        //float parentWidth = 0;
        //var _parent = transform;
        //while (parentWidth == 0)
        //{
        //    _parent = _parent.parent;
        //    parentWidth = _parent.GetComponent<RectTransform>().sizeDelta.x;
        //}
        //tpBarMaxWidth = parentWidth;
        //Debug.Log(TpBar.parent.GetComponent<RectTransform>().sizeDelta.x);
        //tpBarMaxWidth = parentRect.rect.width;
        //Debug.Log(parentRect);

        PrepareScoreAnimation();
    }

    void Update()
    {
        AnimateScore();
    }

    void PrepareScoreAnimation()
    {
        ScoreText.text = "";
        ScoreAnimationTimer = ScoreAnimationDelay;
        var tmp = TpBar.sizeDelta;
        tmp.x = 0f;
        TpBar.sizeDelta = tmp;
        parentRect = TpBar.transform.parent.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Updates the score values displayed in the stage summary menu.
    /// </summary>
    public void UpdateScoreTrackers()
    {
        Miss.text = ScoreManager.MissCount.ToString();
        Bad.text = ScoreManager.BadCount.ToString();
        Good.text = ScoreManager.GoodCount.ToString();
        Perfect.text = ScoreManager.PerfectCount.ToString();

        finalScore = ScoreManager.GetFinalScore();
        BeginAnimation(finalScore);
    }

    void AnimateScore()
    {
        if (!isAnimating) return;
        if (ScoreAnimationTimer > 0)
        {
            ScoreAnimationTimer -= Time.deltaTime;
            return;
        }

        currentTime += Time.deltaTime;
        float normalizedTime = currentTime / ScoreAnimationDuration;

        if (normalizedTime >= 1f)
        {
            normalizedTime = 1;
            isAnimating = false;
        }

        float evaluatedTime = ScoreAnimationCurve.Evaluate(normalizedTime);
        float currentScoreDisplay = Mathf.Lerp(0, finalScore, evaluatedTime);
        ScoreText.text = currentScoreDisplay.ToString(NumberFormat);
        float currentBarWidth = 
            Mathf.Lerp(0, parentRect.rect.width * finalScore / ScoreManager.MaxScore, evaluatedTime);
        var newSizeDelta = TpBar.sizeDelta;
        newSizeDelta.x = currentBarWidth;
        TpBar.sizeDelta = newSizeDelta;
    }

    void BeginAnimation(float newTarget)
    {
        finalScore = newTarget;
        currentTime = 0f;
        isAnimating = true;
    }

    public void Restart()
    {
        PrepareScoreAnimation();
    }
}
