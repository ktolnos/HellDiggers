using System;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

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
        
        if (GUILayout.Button("Print Progression"))
        {
            PrintProgression(script);
        }
    }

    private void PrintProgression(UpgradesController script)
    {
        // Use skillParent if assigned, else try finding children on script (though script is logic, parent is UI)
        Transform root = script.skillParent != null ? script.skillParent : script.transform;
        var skills = root.GetComponentsInChildren<Skill>(true);
        if (skills.Length == 0) return;

        // Map identifier (Field Name) -> Current Level
        Dictionary<string, int> currentLevels = new Dictionary<string, int>(); 

        // Helper to get field name from Skill
        string GetStatKey(Skill s) {
             if (s.stats == null) return null;
             foreach (var field in typeof(Stats).GetFields()) {
                 int val = (int)field.GetValue(s.stats);
                 if (val > 0) return field.Name;
             }
             return null;
        }
        
        // Initialize
        foreach(var s in skills) {
            string k = GetStatKey(s);
            if(k!=null && !currentLevels.ContainsKey(k)) currentLevels[k] = 0;
        }

        int totalCost = 0;
        int step = 1;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- Progression Simulation Start ---");
        sb.AppendLine($"{"Step",-5} | {"Name",-40} | {"Lvl",-3} | {"Cost",-10} | {"Total",-15}");
        sb.AppendLine(new string('-', 70));
        var skillsWithCost = new List<Tuple<int, Skill>>();
        foreach (var skill in skills)
        {
            foreach (var skillPrice in skill.prices)
            {
                skillsWithCost.Add(new Tuple<int, Skill>(skillPrice, skill));
            }
        }
        skillsWithCost.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        foreach (var skillTuple in skillsWithCost)
        {
            var skill = skillTuple.Item2;
            string statKey = GetStatKey(skill);
            if (statKey == null) continue;

            int currLevel = currentLevels[statKey];
            if (skill.currentLevel <= currLevel) continue; // Already purchased

            int price = skill.prices[currLevel];
            totalCost += price;
            currentLevels[statKey] = currLevel + 1;

            sb.AppendLine($"{step,-5} | {skill.name,-40} | {currLevel + 1,-3} | {price,-10} | {totalCost,-15}");
            step++;
        }
        sb.AppendLine("--- Progression Simulation Complete ---");
        Debug.Log(sb.ToString());
    }
}
