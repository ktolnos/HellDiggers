#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveManager saveManager = (SaveManager)target;
        if (GUILayout.Button("Save Game"))
        {
            saveManager.SaveGame();
        }
        if (GUILayout.Button("Load Game"))
        {
            saveManager.LoadGame();
        }
        if (GUILayout.Button("Delete Save File"))
        {
            var saveFilePath = SaveManager.I.SaveFilePath;
            if (System.IO.File.Exists(saveFilePath))
            {
                System.IO.File.Delete(saveFilePath);
                Debug.Log("Save file deleted.");
            }
            else
            {
                Debug.LogWarning("No save file found to delete.");
            }
            if (System.IO.File.Exists(saveManager.SaveTempFilePath))
            {
                System.IO.File.Delete(saveManager.SaveTempFilePath);
                Debug.Log("Temporary save file deleted.");
            }
            else
            {
                Debug.LogWarning("No temporary save file found to delete.");
            }
        }
        
        if (GUILayout.Button("Print Save File"))
        {
            var saveFilePath = saveManager.SaveFilePath;
            if (System.IO.File.Exists(saveFilePath))
            {
                var json = System.IO.File.ReadAllText(saveFilePath);
                Debug.Log("Save file content:\n\n" + json);
            }
            else
            {
                Debug.LogWarning("No save file found to read.");
            }
            
        }
        
        if (GUILayout.Button("Open Save File Location"))
        {
            EditorUtility.RevealInFinder(saveManager.SaveFilePath);
        }
    }
        
}

#endif