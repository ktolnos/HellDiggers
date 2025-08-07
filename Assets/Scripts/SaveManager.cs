using System;
using UnityEngine;

public class SaveManager: MonoBehaviour
{
    public static SaveManager I;
    
    public string SaveFilePath => Application.persistentDataPath + "/save.json";
    public string SaveTempFilePath => Application.persistentDataPath + "/save_tmp.json";

    public delegate void OnLoad();
    public OnLoad onLoad;

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        var saveState = new SaveState
        {
            money = GM.I.money,
            stats = Player.I.stats,
            prevScore = HighScoreManager.I.previousScore,
            latestScore = HighScoreManager.I.latestScore,
            highScore = HighScoreManager.I.highScore
        };
        var json = JsonUtility.ToJson(saveState, true);
        System.IO.File.WriteAllText(SaveTempFilePath, json);
        if (System.IO.File.Exists(SaveFilePath))
        {
            System.IO.File.Delete(SaveFilePath);
        }
        System.IO.File.Move(SaveTempFilePath, SaveFilePath);
    }

    public void LoadGame()
    {
        var loadPath = SaveFilePath;
        if (!System.IO.File.Exists(loadPath))
        {
            loadPath = SaveTempFilePath;
        }
        if (!System.IO.File.Exists(loadPath))
        {
            Debug.LogWarning("Save file not found at " + loadPath);
            return;
        }
        var json = System.IO.File.ReadAllText(loadPath);
        var saveState = JsonUtility.FromJson<SaveState>(json);
        GM.I.money = saveState.money;
        Player.I.stats = saveState.stats;
        HighScoreManager.I.previousScore = saveState.prevScore;
        HighScoreManager.I.latestScore = saveState.latestScore;
        HighScoreManager.I.highScore = saveState.highScore;
        onLoad?.Invoke();
    }

    [Serializable]
    private class SaveState
    {
        public int money;
        public int prevScore;
        public int latestScore;
        public int highScore;
        public Stats stats;
    }
}