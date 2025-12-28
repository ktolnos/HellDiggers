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

        while(true)
        {
            Skill bestCandidate = null;
            int minPrice = int.MaxValue;

            foreach(var s in skills)
            {
                string key = GetStatKey(s);
                if(key == null) continue;
                int gl = currentLevels[key];
                
                // LocalLevel calculation
                int localLevel = Mathf.Clamp(gl - s.levelOffset, 0, s.levelsInThisNode);
                if (localLevel >= s.levelsInThisNode) continue; // Node full

                // Check dependencies
                // 1. Parent requirements
                bool parentMet = true;
                if (s.skillParent != null)
                {
                    string pKey = GetStatKey(s.skillParent);
                    if (pKey != null)
                    {
                        int pGl = currentLevels[pKey];
                        int pLocal = Mathf.Clamp(pGl - s.skillParent.levelOffset, 0, s.skillParent.levelsInThisNode);
                        if (pLocal <= 0) parentMet = false;
                    }
                }

                // 2. Sequence met
                bool sequenceMet = gl >= s.levelOffset;

                if (parentMet && sequenceMet)
                {
                    // Check price
                    if (gl < s.prices.Count)
                    {
                        int price = s.prices[gl];
                        if (price < minPrice)
                        {
                            minPrice = price;
                            bestCandidate = s;
                        }
                    }
                }
            }

            if (bestCandidate != null)
            {
                string key = GetStatKey(bestCandidate);
                int gl = currentLevels[key];
                
                // Get name. Try localized string (maybe missing ref in Editor), fallback to object name
                string displayName = bestCandidate.name;
                if (displayName.Length > 28) displayName = displayName.Substring(0, 25) + "...";
                
                totalCost += minPrice;
                sb.AppendLine($"{step,-5} | {displayName,-40} | {gl + 1,-3} | {minPrice,-10} | {totalCost,-15}");
                currentLevels[key]++;
                step++;
            }
            else
            {
                break;
            }
        }
        sb.AppendLine("--- Progression Simulation Complete ---");
        Debug.Log(sb.ToString());
    }
}
