using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainScene : MonoBehaviour
{
    public static UIMainScene Instance { get; private set; }
    
    public interface IUIInfoContent
    {
        string GetName();
        string GetData();
        void GetContent(ref List<Building.InventoryEntry> content);
    }
    
    public InfoPopup InfoPopup;
    public ResourceDatabase ResourceDB;
    public int ScorePerResource = 100;

    protected IUIInfoContent m_CurrentContent;
    protected List<Building.InventoryEntry> m_ContentBuffer = new List<Building.InventoryEntry>();

    private Text persistenceStatusText;
    private int score;


    private void Awake()
    {
        EnsureDataManager();
        Instance = this;
        InfoPopup.gameObject.SetActive(false);
        ResourceDB.Init();
    }

    private void Start()
    {
        EnsurePersistenceHud();
        WireBackToMenuButton();
        ApplySavedUnitColor();
        UpdatePersistenceHud();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {
        if (m_CurrentContent == null)
            return;
        
        //This is not the most efficient, as we reconstruct everything every time. A more efficient way would check if
        //there was some change since last time (could be made through a IsDirty function in the interface) or smarter
        //update (match an entry content ta type and just update the count) but simplicity in this tutorial we do that
        //every time, this won't be a bottleneck here. 

        InfoPopup.Data.text = m_CurrentContent.GetData();
        
        InfoPopup.ClearContent();
        m_ContentBuffer.Clear();
        
        m_CurrentContent.GetContent(ref m_ContentBuffer);
        foreach (var entry in m_ContentBuffer)
        {
            Sprite icon = null;
            if (ResourceDB != null)
                icon = ResourceDB.GetItem(entry.ResourceId)?.Icone;
            
            InfoPopup.AddToContent(entry.Count, icon);
        }
    }

    public void SetNewInfoContent(IUIInfoContent content)
    {
        if (content == null)
        {
            InfoPopup.gameObject.SetActive(false);
        }
        else
        {
            InfoPopup.gameObject.SetActive(true);
            m_CurrentContent = content;
            InfoPopup.Name.text = content.GetName();
        }
    }

    public void AddScore(int transportedResourceCount)
    {
        if (transportedResourceCount <= 0)
        {
            return;
        }

        score += transportedResourceCount * ScorePerResource;

        if (DataManager.Instance != null)
        {
            DataManager.Instance.TrySaveHighScore(score);
        }

        UpdatePersistenceHud();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void UpdatePersistenceHud()
    {
        if (persistenceStatusText == null || DataManager.Instance == null)
        {
            return;
        }

        string playerName = string.IsNullOrWhiteSpace(DataManager.Instance.PlayerName)
            ? "Player"
            : DataManager.Instance.PlayerName;

        string bestScore = DataManager.Instance.HighScore <= 0
            ? "none"
            : DataManager.Instance.HighScoreName + " - " + DataManager.Instance.HighScore;

        persistenceStatusText.text = "Player: " + playerName + "    Score: " + score + "    Best: " + bestScore;
    }

    private void EnsurePersistenceHud()
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

        GameObject hudObject = new GameObject("Persistence Status", typeof(RectTransform), typeof(Text));
        hudObject.transform.SetParent(canvas.transform, false);

        persistenceStatusText = hudObject.GetComponent<Text>();
        persistenceStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        persistenceStatusText.fontSize = 24;
        persistenceStatusText.alignment = TextAnchor.UpperLeft;
        persistenceStatusText.color = Color.black;

        RectTransform rect = persistenceStatusText.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(-220f, 36f);
    }

    private void WireBackToMenuButton()
    {
        foreach (Button button in FindObjectsOfType<Button>())
        {
            Text label = button.GetComponentInChildren<Text>();
            if (label != null && label.text == "Back to menu")
            {
                button.onClick.RemoveListener(BackToMenu);
                button.onClick.AddListener(BackToMenu);
            }
        }
    }

    private void ApplySavedUnitColor()
    {
        if (DataManager.Instance == null || !DataManager.Instance.TryGetSelectedColor(out Color savedColor))
        {
            return;
        }

        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            unit.SetDisplayColor(savedColor);
        }
    }

    private static void EnsureDataManager()
    {
        if (DataManager.Instance == null)
        {
            new GameObject("Data Manager").AddComponent<DataManager>();
        }
    }
}
