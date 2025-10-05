using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class Skill : MonoBehaviour, ISelectHandler, IDeselectHandler 
{
    public LocalizedString skillName;
    public LocalizedString description;
    public Stats stats;
    public Skill skillParent;
    
    public Color lockedColor;
    public Color unlockedColor;
    [FormerlySerializedAs("upgradedColor")] public Color tooExpensiveColor;
    public Color maxOutColor;
    public int CurrentLevel => (int) myStatField.GetValue(Player.I.stats);
    private Player player;
    private Button button;
    public Image statusIndicator;
    public List<int> prices;
    private FieldInfo myStatField;
    [HideInInspector] public RectTransform rectTransform;
    public Image selectedIndicator;
    private bool interactable;
    
    private void Awake()
    {
        foreach (var fieldInfo in typeof(Stats).GetFields())
        {
            if (fieldInfo.GetValue(stats) is > 0)
            {
                if (myStatField != null)
                {
                    Debug.LogWarning("Multiple stat fields found in Stats class: " + myStatField.Name + " and " + fieldInfo.Name);
                    break;
                }
                myStatField = fieldInfo;
            }
        }
        rectTransform = GetComponent<RectTransform>();
    }
    
    void Start()
    {
        player = Player.I;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(AddStats);
    }

    void AddStats()
    {
        if (!interactable) return;
        if (!GM.I.isFree)
        {
            if (CurrentLevel >= prices.Count) return;
            GM.I.money -= prices[CurrentLevel];
        }
        
        player.stats += stats;
        SaveManager.I.SaveGame();
    }

    private void Update()
    {
        if ((skillParent == null || skillParent.CurrentLevel > 0) && CurrentLevel < prices.Count)
        {
            var canAfford = prices[CurrentLevel] < GM.I.money;
            statusIndicator.color = canAfford ? unlockedColor : tooExpensiveColor;
            canAfford |= GM.I.isFree; // Allow free upgrades
            interactable = canAfford;
        }else
        {
            interactable = false;
            if (CurrentLevel == prices.Count)
            {
                statusIndicator.color = maxOutColor;
            }
            else
            {
                statusIndicator.color = lockedColor;
            }
        }

        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            ShowPopup();
        }
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        ShowPopup();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HidePopup();
    }

    public void ShowPopup()
    {
        button.Select();
        SkillPopup.I.Show(this);
        selectedIndicator.enabled = true;
    }

    public void HidePopup()
    {
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        SkillPopup.I.Hide(this);
        selectedIndicator.enabled = false;
    }
}
