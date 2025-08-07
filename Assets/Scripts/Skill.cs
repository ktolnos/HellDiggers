using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public Stats stats;
    public Skill skillParent;
    public RectTransform popup;
    
    public Color lockedColor;
    public Color unlockedColor;
    [FormerlySerializedAs("upgradedColor")] public Color tooExpensiveColor;
    public Color maxOutColor;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;
    public bool hasDescription = false;
    private int CurrentLevel => (int) myStatField.GetValue(Player.I.stats);
    private Player player;
    private Button button;
    private Image image;
    public List<int> prices;
    private FieldInfo myStatField;
    
    void Start()
    {
        player = Player.I;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(AddStats);
        button.interactable = false;
        popup.gameObject.SetActive(false);
        image = GetComponent<Image>();
        if (!hasDescription)
        {
            descriptionText.gameObject.SetActive(false);
        }
        
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
    }
    
    void AddStats()
    {
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
            button.interactable = true;
            var canAfford = prices[CurrentLevel] < GM.I.money;
            image.color = canAfford ? unlockedColor : tooExpensiveColor;
            canAfford |= GM.I.isFree; // Allow free upgrades
            button.interactable = canAfford;
        }else
        {
            button.interactable = false;
            if (CurrentLevel == prices.Count)
            {
                image.color = maxOutColor;
            }
            else
            {
                image.color = lockedColor;
            }
        }
        
        lvlText.text = CurrentLevel + "/" + prices.Count;
        priceText.text = CurrentLevel < prices.Count ? prices[CurrentLevel].ToString() : "Maxed";
    }

    public void ShowPopup()
    {
        popup.gameObject.SetActive(true);
    }

    public void HidePopup()
    {
        popup.gameObject.SetActive(false);
    }
}
