using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager I;

    private string PersistentDataPath
    {
        get
        {
#if UNITY_WEBGL
            return "idbfs/Vicious-Loop-cGFwZXJjbG90aGVzcGxhbm5lZGRpbm5lcmV";
#else
            return Application.persistentDataPath;
#endif
        }
    }

    public string SaveFilePath => PersistentDataPath + "/save.json";

    public delegate void OnLoad();

    public OnLoad onLoad;

    private void Awake()
    {
        I = this;
    }

    public void SaveGame()
    {
        var saveState = new SaveState
        {
            money = GM.I.money,
            stats = Player.I.stats,
            prevScore = HighScoreManager.I.previousScore,
            latestScore = HighScoreManager.I.latestScore,
            highScore = HighScoreManager.I.highScore,
            currentGunId = Player.I.currentGunId,
            secondaryGunId = Player.I.secondaryGunId
        };
        var json = JsonUtility.ToJson(saveState, true);
        System.IO.FileInfo file = new System.IO.FileInfo(SaveFilePath);
        file.Directory.Create();
        System.IO.File.WriteAllText(SaveFilePath, json);
#if UNITY_WEBGL
#pragma warning disable CS0618 // Type or member is obsolete
        Application.ExternalEval("_JS_FileSystem_Sync();");
#pragma warning restore CS0618 // Type or member is obsolete
#endif
    }

    public void LoadGame()
    {
        var loadPath = SaveFilePath;
        if (!System.IO.File.Exists(loadPath))
        {
            Debug.LogWarning("Save file not found at " + loadPath);
            return;
        }

        var json = System.IO.File.ReadAllText(loadPath);
        var saveState = JsonUtility.FromJson<SaveState>(json);
        GM.I.money = saveState.money;
        Player.I.stats = saveState.stats;
        Player.I.currentGunId = saveState.currentGunId;
        if (string.IsNullOrEmpty(Player.I.currentGunId))
        {
            Player.I.currentGunId = "pistol";
        }
        Player.I.secondaryGunId = saveState.secondaryGunId;
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
        public string currentGunId;
        public string secondaryGunId;
    }
    
    
    public bool DeleteSaveFile()
    {
        if (System.IO.File.Exists(SaveFilePath))
        {
            System.IO.File.Delete(SaveFilePath);
            Debug.Log("Save file deleted.");
            return true;
        }
        else
        {
            Debug.LogWarning("No save file to delete at " + SaveFilePath);
            return false;
        }
    }
}