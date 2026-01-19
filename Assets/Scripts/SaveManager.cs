using System;
using System.Collections.Generic;
using System.Linq;
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
        var upgradeData = UpgradesController.I.GetUpgradeSaveData();
        var upgradeIds = new List<string>();
        var upgradeLevels = new List<int>();
        foreach (var kvp in upgradeData)
        {
            upgradeIds.Add(kvp.Key);
            upgradeLevels.Add(kvp.Value);
        }

        var saveState = new SaveState
        {
            money = GM.I.GetTotalMoney(),
            upgradeIds = upgradeIds,
            upgradeLevels = upgradeLevels,
            prevScore = HighScoreManager.I.previousScore,
            latestScore = HighScoreManager.I.latestScore,
            highScore = HighScoreManager.I.highScore,
            currentGunId = Player.I.currentGunId,
            secondaryGunId = Player.I.secondaryGunId,
            purchasedGuns = GunStation.purchasedGuns.ToList(),
            settingsState = new SettingsState
            {
                musicVolume = SoundManager.I.musicVolume,
                sfxVolume = SoundManager.I.sfxVolume,
                masterVolume = SoundManager.I.masterVolume
            }
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
        
        // Reconstruct dictionary
        var upgradeData = new Dictionary<string, int>();
        if (saveState.upgradeIds != null && saveState.upgradeLevels != null)
        {
            for(int i=0; i<Mathf.Min(saveState.upgradeIds.Count, saveState.upgradeLevels.Count); i++)
            {
                upgradeData[saveState.upgradeIds[i]] = saveState.upgradeLevels[i];
            }
        }
        UpgradesController.I.LoadUpgradeSaveData(upgradeData);
        // LoadUpgradeSaveData triggers Player.UpdateStats, so we don't need to call it again.

        Player.I.currentGunId = saveState.currentGunId;
        if (string.IsNullOrEmpty(Player.I.currentGunId))
        {
            Player.I.currentGunId = "pistol";
        }
        Player.I.secondaryGunId = saveState.secondaryGunId;
        HighScoreManager.I.previousScore = saveState.prevScore;
        HighScoreManager.I.latestScore = saveState.latestScore;
        HighScoreManager.I.highScore = saveState.highScore;
        GunStation.purchasedGuns = new HashSet<string>(saveState.purchasedGuns);
        
        SoundManager.I.musicVolume = saveState.settingsState.musicVolume;
        SoundManager.I.sfxVolume = saveState.settingsState.sfxVolume;
        SoundManager.I.masterVolume = saveState.settingsState.masterVolume;
        
        onLoad?.Invoke();
    }

    [Serializable]
    private class SaveState
    {
        public int money;
        public int prevScore;
        public int latestScore;
        public int highScore;
        public List<string> upgradeIds;
        public List<int> upgradeLevels;
        public string currentGunId;
        public string secondaryGunId;
        public List<string> purchasedGuns;
        public SettingsState settingsState = new SettingsState();
    }
    
    [Serializable]
    private class SettingsState
    {
        public float musicVolume = 0.1f;
        public float sfxVolume = 0.1f;
        public float masterVolume = 0.1f;
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