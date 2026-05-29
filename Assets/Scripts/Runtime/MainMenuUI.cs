using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(DataManager.Instance.PlayerName))
        {
            playerNameInput.text = DataManager.Instance.PlayerName;
        }

        RefreshHighScore();
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(StartGame);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    private void StartGame()
    {
        DataManager.Instance.SetPlayerName(playerNameInput.text);
        SceneManager.LoadScene("Game");
    }

    private void QuitGame()
    {
        DataManager.Instance.SetPlayerName(playerNameInput.text);
        Application.Quit();
    }

    private void RefreshHighScore()
    {
        if (DataManager.Instance.HighScore <= 0)
        {
            highScoreText.text = "Best Score: none yet";
            return;
        }

        highScoreText.text = "Best Score: " + DataManager.Instance.HighScoreName + " - " + DataManager.Instance.HighScore;
    }
}
