using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Title")]
    public TMP_Text titleText; // Assign your title TMP_Text here

    [Header("Controls Text")]
    public TMP_Text controlsText;

    [Header("Buttons")]
    public Button playButton;
    public Button quitButton;
    public Button controlsButton;
    public Button backButton;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; // Change to your actual game scene name

    [TextArea(5, 10)]
    public string controlsInfo =
        "Controls:\n" +
        "WASD - Move\n" +
        "Space - Jump\n" +
        "Left Shift - Run\n" +
        "Mouse - Look\n" +
        "Left Mouse - Grapple\n";

    void Start()
    {
        // Set controls text (hidden at start)
        if (controlsText != null)
            controlsText.gameObject.SetActive(false);

        // Hide back button at start
        if (backButton != null)
            backButton.gameObject.SetActive(false);

        // Assign button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        if (controlsButton != null)
            controlsButton.onClick.AddListener(ShowControls);
        if (backButton != null)
            backButton.onClick.AddListener(HideControls);
    }

    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogError("Game scene name not set in MainMenu!");
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ShowControls()
    {
        // Hide main buttons and title
        if (playButton != null)
            playButton.gameObject.SetActive(false);
        if (quitButton != null)
            quitButton.gameObject.SetActive(false);
        if (controlsButton != null)
            controlsButton.gameObject.SetActive(false);
        if (titleText != null)
            titleText.gameObject.SetActive(false);

        // Show controls text and back button
        if (controlsText != null)
        {
            controlsText.text = controlsInfo;
            controlsText.gameObject.SetActive(true);
        }
        if (backButton != null)
            backButton.gameObject.SetActive(true);
    }

    public void HideControls()
    {
        // Show main buttons and title
        if (playButton != null)
            playButton.gameObject.SetActive(true);
        if (quitButton != null)
            quitButton.gameObject.SetActive(true);
        if (controlsButton != null)
            controlsButton.gameObject.SetActive(true);
        if (titleText != null)
            titleText.gameObject.SetActive(true);

        // Hide controls text and back button
        if (controlsText != null)
            controlsText.gameObject.SetActive(false);
        if (backButton != null)
            backButton.gameObject.SetActive(false);
    }
}