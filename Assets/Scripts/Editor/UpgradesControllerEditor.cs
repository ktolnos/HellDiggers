using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradesController))]
public class UpgradesControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpgradesController script = (UpgradesController)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Rebuild Tree"))
        {
            script.RebuildSkillTree();
        }
    }
}
