using System;
using System.Collections.Generic;
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
    private int currentLevel = 0;
    private Player player;
    private Button button;
    private Image image;
    public List<int> prices;
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
    }
    void AddStats()
    {
        if (currentLevel >= prices.Count) return;
        player.stats += stats;
        Player.I.money -= prices[currentLevel];
        currentLevel++;
    }

    private void Update()
    {
        if ((skillParent == null || skillParent.currentLevel > 0) && currentLevel < prices.Count)
        {
            button.interactable = true;
            var canAfford = prices[currentLevel] < Player.I.money;
            image.color = canAfford ? unlockedColor : tooExpensiveColor;
            button.interactable = canAfford;
        }else
        {
            button.interactable = false;
            if (currentLevel == prices.Count)
            {
                image.color = maxOutColor;
            }
            else
            {
                image.color = lockedColor;
            }
        }
        
        lvlText.text = currentLevel + "/" + prices.Count;
        priceText.text = currentLevel < prices.Count ? prices[currentLevel].ToString() : "Maxed";
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
