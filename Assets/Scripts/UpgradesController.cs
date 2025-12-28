using System;
using IPD;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UpgradesController: MonoBehaviour
{
    public static UpgradesController I;
    public Canvas upgradeUI;
    public RectTransform skillParent;
    
    public bool IsActive => upgradeUI.gameObject.activeSelf;
    
    private void Awake()
    {
        I = this;
    }
    
    private void Start()
    {
        if (IsActive)
        {
            ShowUpgrades();
        }
    }

    public void ShowUpgrades()
    {
        if (IsActive)
        {
            return;
        }
        upgradeUI.gameObject.SetActive(true);
        SkillPopup.I.Hide(null);
        GM.OnUIOpen(HideUpgrades);
    }
    
    public void HideUpgrades()
    {
        if (!IsActive)
        {
            return;
        }
        upgradeUI.gameObject.SetActive(false);
    }
        

    [Header("Skill Tree Settings")]
    public List<Skill> skillPrefabs;
    public Vector2 gridSpacing = new Vector2(250, 150);

    public void RebuildSkillTree()
    {
        if (skillParent == null)
        {
            Debug.LogError("Skill parent is not assigned in UpgradesController!");
            return;
        }

        if (skillPrefabs == null || skillPrefabs.Count == 0)
        {
             Debug.LogWarning("No skill prefabs assigned.");
             return;
        }

        // Clear existing children
        int childCount = skillParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (skillParent.GetChild(i).GetComponent<Skill>() == null) continue;
            var child = skillParent.GetChild(i).gameObject;
#if UNITY_EDITOR
             UnityEditor.Undo.DestroyObjectImmediate(child);
#else
             DestroyImmediate(child);
#endif
        }

        // Flatten skills by price: (Price, Prefab)
        var upgradeSteps = new List<(int price, Skill prefab)>();
        foreach(var prefab in skillPrefabs)
        {
            if(prefab == null) continue;
            foreach(var price in prefab.prices)
            {
                upgradeSteps.Add((price, prefab));
            }
        }
        
        // Sort by price
        upgradeSteps.Sort((a, b) => a.price.CompareTo(b.price));

        if (upgradeSteps.Count == 0) return;

        // Tracking
        var placedNodes = new List<Skill>(); // All placed instances
        var occupied = new HashSet<Vector2Int>();
        var lastNodeForPrefab = new Dictionary<Skill, Skill>(); // Prefab -> Last Instance
        var levelsProcessedForPrefab = new Dictionary<Skill, int>(); // Prefab -> Count
        var ancestorInstances = new Dictionary<Skill, HashSet<Skill>>(); // Instance -> Set of ancestor instances (inclusive of self)

        var rng = new System.Random();

        foreach (var step in upgradeSteps)
        {
             Skill prefab = step.prefab;
             
             // First node handling
             if (placedNodes.Count == 0)
             {
                 var instance = InstantiateSkill(prefab, Vector2Int.zero);
                 instance.skillParent = null; 
                 instance.levelOffset = 0;
                 instance.levelsInThisNode = 1;

                 occupied.Add(Vector2Int.zero);
                 placedNodes.Add(instance);
                 
                 lastNodeForPrefab[prefab] = instance;
                 levelsProcessedForPrefab[prefab] = 1;
                 ancestorInstances[instance] = new HashSet<Skill> { instance };
                 continue;
             }

             // Determine Requirement Restrictions
             var requiredAncestors = new List<Skill>();

             if (lastNodeForPrefab.TryGetValue(prefab, out Skill lastNode))
             {
                 // If we have a previous node, THAT is our sole strict requirement for linear progression.
                 // (Because that node already satisfied the prefab's prerequisites).
                 requiredAncestors.Add(lastNode);
             }
             else if (prefab.prerequisites != null)
             {
                 // First node of this prefab? adhere to prerequisites
                 foreach (var prereq in prefab.prerequisites)
                 {
                     if (lastNodeForPrefab.TryGetValue(prereq, out Skill pNode))
                     {
                         requiredAncestors.Add(pNode);
                     }
                 }
             }

             // Find candidates that satisfy ALL requirements
             // Candidate must be a descendant-or-self of every node in requiredAncestors
             var validParents = new List<Skill>();
             
             if (requiredAncestors.Count == 0)
             {
                 validParents.AddRange(placedNodes);
             }
             else
             {
                 foreach (var p in placedNodes)
                 {
                     if (!ancestorInstances.ContainsKey(p)) continue;
                     var pAncestors = ancestorInstances[p];
                     
                     bool allMet = true;
                     foreach (var req in requiredAncestors)
                     {
                         if (!pAncestors.Contains(req))
                         {
                             allMet = false;
                             break;
                         }
                     }
                     if (allMet) validParents.Add(p);
                 }
                 
                 // Fallback warning
                 if (validParents.Count == 0)
                 {
                      Debug.LogWarning($"Could not satisfy lineage for {prefab.name}. Needs descendants of {string.Join(", ", requiredAncestors.Select(r => r.name))}");
                      // Fallback: if we needed lastNode, we MUST attach to lastNode (it satisfies itself).
                      // But if lastNode wasn't found in placedNodes? (It should be there).
                      // If prereqs strictly disjoint, we might fail.
                 }
             }

             // Filter candidates for spatial availability
             var candidates = new List<(Skill parent, Vector2Int pos)>();
             Skill samePrefabLastNode = null;
             if (lastNodeForPrefab.TryGetValue(prefab, out var node)) samePrefabLastNode = node;

             foreach (var p in validParents)
             {
                 // Optimistic check: if p is same prefab, we can merge, so availability doesn't matter yet
                 if (p == samePrefabLastNode)
                 {
                     // Can always merge if it's the correct lastNode? 
                     // Logic: If p is validParent, and p is same prefab, it MUST be lastNode (because we descend).
                     // So we add it as candidate for MERGE.
                     // Position doesn't matter for merge.
                     candidates.Add((p, Vector2Int.zero)); 
                 }
                 else
                 {
                     // Check spatial slots for SPLIT
                     var pPos = GetGridPos(p);
                     var neighbors = new List<Vector2Int>
                     {
                         pPos + Vector2Int.right,
                         pPos + Vector2Int.left,
                         pPos + Vector2Int.right,
                         pPos + Vector2Int.left,
                         pPos + Vector2Int.up,
                         pPos + Vector2Int.down
                     };
                     
                     foreach(var n in neighbors)
                     {
                         if (!occupied.Contains(n))
                         {
                             candidates.Add((p, n));
                         }
                     }
                 }
             }

             if (candidates.Count == 0)
             {
                 Debug.LogWarning($"Could not place upgrade for {prefab.name} (Price {step.price}).");
                 continue;
             }
             
             // Pick one
             var choice = candidates[rng.Next(candidates.Count)];
             var chosenParent = choice.parent;
             
             // Merge Check
             if (chosenParent == samePrefabLastNode)
             {
                 chosenParent.levelsInThisNode++;
                 levelsProcessedForPrefab[prefab]++;
                 // lastNode stays same
             }
             else
             {
                 // Split / New Node
                 var placePos = choice.pos;
                 var newInstance = InstantiateSkill(prefab, placePos);
                 
                 newInstance.skillParent = chosenParent;
                 if (!levelsProcessedForPrefab.ContainsKey(prefab)) levelsProcessedForPrefab[prefab] = 0;
                 newInstance.levelOffset = levelsProcessedForPrefab[prefab];
                 newInstance.levelsInThisNode = 1;
                 
                 UpdateConnector(newInstance, chosenParent);
                 
                 occupied.Add(placePos);
                 placedNodes.Add(newInstance);
                 lastNodeForPrefab[prefab] = newInstance;
                 levelsProcessedForPrefab[prefab]++;
                 
                 // Update ancestors: Parent's ancestors + Parent + Self (Self is implied in set for next gens)
                 // Actually my set definition was "inclusive of self".
                 // So new set = ancestors[parent] U {newInstance}
                 var newAncestors = new HashSet<Skill>(ancestorInstances[chosenParent]);
                 newAncestors.Add(newInstance);
                 ancestorInstances[newInstance] = newAncestors;
             }
        }
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(skillParent.gameObject);
#endif
    }
    
    private Skill InstantiateSkill(Skill prefab, Vector2Int gridPos)
    {
#if UNITY_EDITOR
         var instance = (Skill)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, skillParent);
#else
         var instance = Instantiate(prefab, skillParent);
#endif
         SetSkillPosition(instance, gridPos);
         return instance;
    }

    private void SetSkillPosition(Skill skill, Vector2Int gridPos)
    {
        var rect = skill.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(gridPos.x * gridSpacing.x, gridPos.y * gridSpacing.y);
            // Ensure z is 0? 2D UI usually is.
        }
    }
    
    private Vector2Int GetGridPos(Skill skill)
    {
        var rect = skill.GetComponent<RectTransform>();
        if (rect == null) return Vector2Int.zero;
        return new Vector2Int(Mathf.RoundToInt(rect.anchoredPosition.x / gridSpacing.x), Mathf.RoundToInt(rect.anchoredPosition.y / gridSpacing.y));
    }

    private void UpdateConnector(Skill child, Skill parent)
    {
         if(child.connector == null) return;
         
         var childRect = child.GetComponent<RectTransform>();
         var parentRect = parent.GetComponent<RectTransform>();
         
         if (childRect == null || parentRect == null) return;

         var direction = (parentRect.anchoredPosition - childRect.anchoredPosition);
         // 0 rotation connects to the right
         // Atan2(y, x) returns angle in radians. 
         // If dir is (1,0) [Right], angle is 0.
         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
         child.connector.localRotation = Quaternion.Euler(0, 0, angle);
         child.connector.localPosition = Vector3.zero;
    }

}