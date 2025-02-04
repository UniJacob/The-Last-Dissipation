using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script responsible for the stage selection menu.
/// </summary>
public class StageSelector : MonoBehaviour
{
    [SerializeField] GameObject TrackSelectArea;
    [SerializeField] GameObject ChapterSelectArea;
    [SerializeField] GameObject ConfirmationArea;
    [SerializeField] RawImage ConfirmationAreaImage;
    [SerializeField] TMP_Text ConfirmationAreaStageName;
    [SerializeField] TMP_Text ConfirmationAreaStageDifficulty;
    [SerializeField] GameObject NextChapterButton;
    [SerializeField] GameObject PreviousChapterButton;
    [SerializeField] ScrollRect TrackSelectAreaScrollRect;
    [SerializeField] TMP_Text ChapterTitle;
    [SerializeField] GameObject[] ChaptersContainers;

    static int lastShownChapterIndex = 0;
    int currentShownChapterIndex = 0;
    string selectedStageDiff = "";
    int StageSpeedCoefficient = 1;

    void Start()
    {
        ChapterSelectArea.SetActive(true);
        TrackSelectArea.SetActive(true);
        ConfirmationArea.SetActive(false);

        foreach (var chapterContainer in ChaptersContainers)
        {
            chapterContainer.SetActive(false);
        }
        currentShownChapterIndex = lastShownChapterIndex;
        ChaptersContainers[currentShownChapterIndex].SetActive(true);

        UpdateForNewChapterUnlock();
        SwitchChapterUpdate();
    }

    public void UpdateForNewChapterUnlock()
    {
        if (currentShownChapterIndex <= 0)
        {
            PreviousChapterButton.SetActive(false);
        }
        if (currentShownChapterIndex >= GameState.UnlockedChapters)
        {
            NextChapterButton.SetActive(false);
        }
        else
        {
            NextChapterButton.SetActive(true);
        }
    }

    /// <summary>
    /// Displays and sets the confirmation menu (after selecting a stage).
    /// </summary>
    /// <param name="stageName">Stage name that will be displayed on the menu.</param>
    /// <param name="stageThumbnail">Stage image that will be displayed on the menu.</param>
    public void SetConfirmationMenu(
        string stageName, string difficulty, RawImage stageThumbnail, int stageSpeedCoefficient = 1)
    {
        ConfirmationAreaImage.texture = stageThumbnail.texture;
        ConfirmationAreaImage.uvRect = stageThumbnail.uvRect;
        ConfirmationAreaStageName.text = stageName;
        selectedStageDiff = difficulty;
        ConfirmationAreaStageDifficulty.text = "Difficulty " + difficulty;
        StageSpeedCoefficient = stageSpeedCoefficient;
        ToggleConfirmationMenu();
    }

    /// <summary>
    /// Displays or hides the confirmation menu.
    /// </summary>
    public void ToggleConfirmationMenu()
    {
        TrackSelectArea.SetActive(!TrackSelectArea.activeSelf);
        ChapterSelectArea.SetActive(!ChapterSelectArea.activeSelf);
        ConfirmationArea.SetActive(!ConfirmationArea.activeSelf);
    }

    public void ConfirmStageSelection()
    {
        lastShownChapterIndex = currentShownChapterIndex;
        OverworldManager.instance.BeginStage(
            ConfirmationAreaStageName.text, selectedStageDiff, true, StageSpeedCoefficient);
    }

    public void CloseStageSelection()
    {
        StageSpeedCoefficient = 1;
        TrackSelectArea.SetActive(true);
        ChapterSelectArea.SetActive(true);
        ConfirmationArea.SetActive(false);
        OverworldState.IsInMenu = false;
        GetComponent<CanvasFader>().TriggerFadeOut();
    }

    public void ShowNextChapter()
    {
        ChaptersContainers[currentShownChapterIndex++].SetActive(false);
        ChaptersContainers[currentShownChapterIndex].SetActive(true);
        if (currentShownChapterIndex >= GameState.UnlockedChapters)
        {
            NextChapterButton.SetActive(false);
        }
        PreviousChapterButton.SetActive(true);
        SwitchChapterUpdate();
    }

    public void ShowPreviousChapter()
    {
        ChaptersContainers[currentShownChapterIndex--].SetActive(false);
        ChaptersContainers[currentShownChapterIndex].SetActive(true);
        if (currentShownChapterIndex == 0)
        {
            PreviousChapterButton.SetActive(false);
        }
        NextChapterButton.SetActive(true);
        SwitchChapterUpdate();
    }

    void SwitchChapterUpdate()
    {
        TrackSelectAreaScrollRect.content =
            ChaptersContainers[currentShownChapterIndex].GetComponent<RectTransform>();
        ChapterTitle.text = "Chapter " + currentShownChapterIndex;
    }
}
