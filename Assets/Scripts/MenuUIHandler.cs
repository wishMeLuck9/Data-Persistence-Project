using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Sets the script to be executed later than all default scripts
// This is helpful for UI, since other things may need to be initialized before setting the UI
[DefaultExecutionOrder(1000)]
public class MenuUIHandler : MonoBehaviour
{
    public ColorPicker ColorPicker;

    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Text highScoreText;

    private Color selectedColor = new Color(0.2f, 0.55f, 1f, 1f);

    public void NewColorSelected(Color color)
    {
        selectedColor = color;
        EnsureDataManager();
        DataManager.Instance.SetSelectedColor(selectedColor);
    }

    private void Start()
    {
        EnsureDataManager();
        EnsurePersistenceUi();

        if (ColorPicker != null)
        {
            ColorPicker.Init();
            //this will call the NewColorSelected function when the color picker have a color button clicked.
            ColorPicker.onColorChanged += NewColorSelected;

            if (DataManager.Instance.TryGetSelectedColor(out Color savedColor))
            {
                selectedColor = savedColor;
                ColorPicker.SelectColor(savedColor);
            }
        }

        if (playerNameInput != null && !string.IsNullOrWhiteSpace(DataManager.Instance.PlayerName))
        {
            playerNameInput.text = DataManager.Instance.PlayerName;
        }

        RefreshHighScore();
    }

    private void OnDestroy()
    {
        if (ColorPicker != null)
        {
            ColorPicker.onColorChanged -= NewColorSelected;
        }
    }

    public void SaveColorClicked()
    {
        SaveSelection();
        RefreshHighScore();
    }

    // Keep the original misspelling because the downloaded scene references it.
    public void LoadColorCliked()
    {
        EnsureDataManager();

        if (DataManager.Instance.TryGetSelectedColor(out Color savedColor))
        {
            selectedColor = savedColor;
            if (ColorPicker != null)
            {
                ColorPicker.SelectColor(savedColor);
            }
        }

        if (playerNameInput != null && !string.IsNullOrWhiteSpace(DataManager.Instance.PlayerName))
        {
            playerNameInput.text = DataManager.Instance.PlayerName;
        }

        RefreshHighScore();
    }

    public void StartNew()
    {
        SaveSelection();
        SceneManager.LoadScene("Main");
    }

    public void Exit()
    {
        Application.Quit();
    }

    private void SaveSelection()
    {
        EnsureDataManager();
        DataManager.Instance.SetSelectedColor(selectedColor);

        if (playerNameInput != null)
        {
            DataManager.Instance.SetPlayerName(playerNameInput.text);
        }
    }

    private void RefreshHighScore()
    {
        if (highScoreText == null || DataManager.Instance == null)
        {
            return;
        }

        highScoreText.text = DataManager.Instance.HighScore <= 0
            ? "Best Score: none yet"
            : "Best Score: " + DataManager.Instance.HighScoreName + " - " + DataManager.Instance.HighScore;
    }

    private void EnsurePersistenceUi()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (playerNameInput == null)
        {
            GameObject inputRoot = new GameObject("Player Name Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputRoot.transform.SetParent(canvas.transform, false);
            RectTransform rect = inputRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(320f, 44f);
            inputRoot.GetComponent<Image>().color = Color.white;

            Text text = CreateText(inputRoot.transform, "Text", "", font, TextAnchor.MiddleLeft, 20, Color.black);
            Stretch(text.rectTransform, new Vector2(14f, 4f), new Vector2(-14f, -4f));

            Text placeholder = CreateText(inputRoot.transform, "Placeholder", "Player name", font, TextAnchor.MiddleLeft, 20, new Color(0.45f, 0.45f, 0.45f));
            Stretch(placeholder.rectTransform, new Vector2(14f, 4f), new Vector2(-14f, -4f));

            playerNameInput = inputRoot.GetComponent<InputField>();
            playerNameInput.textComponent = text;
            playerNameInput.placeholder = placeholder;
        }

        if (highScoreText == null)
        {
            highScoreText = CreateText(canvas.transform, "Best Score Text", "", font, TextAnchor.MiddleCenter, 22, Color.black);
            RectTransform rect = highScoreText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -76f);
            rect.sizeDelta = new Vector2(520f, 34f);
        }
    }

    private static Text CreateText(Transform parent, string name, string value, Font font, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        return text;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void EnsureDataManager()
    {
        if (DataManager.Instance == null)
        {
            new GameObject("Data Manager").AddComponent<DataManager>();
        }
    }
}
