using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text messageText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Transform paddle;
    [SerializeField] private Rigidbody2D ball;
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private float paddleSpeed = 7f;
    [SerializeField] private float ballSpeed = 7f;

    private int score;
    private int remainingBricks;
    private bool roundActive;

    private void Awake()
    {
        restartButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Start()
    {
        restartButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);
        messageText.text = "Press Space to launch";
        UpdateScore(0);
        UpdateHighScoreText();
        CreateBrickRows();
    }

    private void Update()
    {
        MovePaddle();

        if (!roundActive && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(LaunchBall());
        }
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(Restart);
        menuButton.onClick.RemoveListener(ReturnToMenu);
    }

    public void AddScore(int points)
    {
        UpdateScore(score + points);
    }

    public void BrickDestroyed()
    {
        remainingBricks--;
        if (remainingBricks <= 0)
        {
            GameOver(true);
        }
    }

    public void GameOver(bool won)
    {
        if (!roundActive && ball.velocity == Vector2.zero)
        {
            return;
        }

        roundActive = false;
        ball.velocity = Vector2.zero;
        ball.transform.position = new Vector3(0f, -3.3f, 0f);

        bool newBest = DataManager.Instance.TrySaveHighScore(score);
        string result = won ? "You cleared the board!" : "Game over";
        messageText.text = newBest ? result + " New best score!" : result;
        UpdateHighScoreText();
        restartButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    private void MovePaddle()
    {
        float move = Input.GetAxisRaw("Horizontal") * paddleSpeed * Time.deltaTime;
        Vector3 position = paddle.position;
        position.x = Mathf.Clamp(position.x + move, -6.2f, 6.2f);
        paddle.position = position;
    }

    private IEnumerator LaunchBall()
    {
        roundActive = true;
        messageText.text = "";
        yield return null;
        Vector2 direction = new Vector2(Random.Range(-0.6f, 0.6f), 1f).normalized;
        ball.velocity = direction * ballSpeed;
    }

    private void CreateBrickRows()
    {
        const int columns = 8;
        const int rows = 4;
        const float xSpacing = 1.45f;
        const float ySpacing = 0.55f;
        Vector2 start = new Vector2(-5.05f, 2.4f);

        remainingBricks = columns * rows;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector2 position = start + new Vector2(column * xSpacing, row * ySpacing);
                var brick = Instantiate(brickPrefab, position, Quaternion.identity);
                brick.name = "Brick " + row + "-" + column;
                brick.GetComponent<Brick>().PointValue = (row + 1) * 5;
            }
        }
    }

    private void UpdateScore(int newScore)
    {
        score = newScore;
        scoreText.text = "Score: " + score;
    }

    private void UpdateHighScoreText()
    {
        if (DataManager.Instance.HighScore <= 0)
        {
            highScoreText.text = "Best: none";
            return;
        }

        highScoreText.text = "Best: " + DataManager.Instance.HighScoreName + " - " + DataManager.Instance.HighScore;
    }

    private void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
