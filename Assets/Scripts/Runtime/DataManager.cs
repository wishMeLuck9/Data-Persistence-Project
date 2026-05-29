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
    private string SavePath => Path.Combine(Application.persistentDataPath, "breakout-save.json");

    [Serializable]
    private class SaveData
    {
        public string playerName = "";
        public string highScoreName = "";
        public int highScore;
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
