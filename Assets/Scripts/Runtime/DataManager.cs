using System;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public string PlayerName => data.playerName;
    public string HighScoreName => data.highScoreName;
    public int HighScore => data.highScore;

    private SaveData data = new SaveData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "data-persistence-save.json");

    [Serializable]
    private class SaveData
    {
        public string playerName = "";
        public string highScoreName = "";
        public int highScore;
        public bool hasSelectedColor;
        public float selectedColorR = 0.2f;
        public float selectedColorG = 0.55f;
        public float selectedColorB = 1f;
        public float selectedColorA = 1f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void SetPlayerName(string playerName)
    {
        data.playerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
        Save();
    }

    public bool TrySaveHighScore(int score)
    {
        if (score <= data.highScore)
        {
            return false;
        }

        data.highScore = score;
        data.highScoreName = string.IsNullOrWhiteSpace(data.playerName) ? "Player" : data.playerName;
        Save();
        return true;
    }

    public void SetSelectedColor(Color color)
    {
        data.hasSelectedColor = true;
        data.selectedColorR = color.r;
        data.selectedColorG = color.g;
        data.selectedColorB = color.b;
        data.selectedColorA = color.a;
        Save();
    }

    public bool TryGetSelectedColor(out Color color)
    {
        color = new Color(data.selectedColorR, data.selectedColorG, data.selectedColorB, data.selectedColorA);
        return data.hasSelectedColor;
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            return;
        }

        string json = File.ReadAllText(SavePath);
        data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
    }
}
