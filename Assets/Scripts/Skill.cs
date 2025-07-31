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
    public bool isEnabled = false;
    public RectTransform popup;
    public Color lockedColor;
    public Color unlockedColor;
    [FormerlySerializedAs("upgradedColor")] public Color tooExpensiveColor;
    public Color maxOutColor;
    private TextMeshProUGUI buttonText;
    private int currentLevel = 0;
    private Player player;
    private Button button;
    private Image image;
    public List<int> prices;
    
    void Start()
    {
        player = Player.I;
        button = GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.AddListener(AddStats);
            button.interactable = false;
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        }
        popup.gameObject.SetActive(false);
        image = GetComponent<Image>();
    }
    void AddStats()
    {
        if (currentLevel >= prices.Count) return;
        isEnabled = true;
        player.stats += stats;
        Player.I.money -= prices[currentLevel];
        currentLevel++;
    }

    private void Update()
    {
        if ((skillParent == null || skillParent.isEnabled) && currentLevel < prices.Count)
        {
            if (button != null)
            {
                button.interactable = true;
            }
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

        if (buttonText != null)
        {
            buttonText.text = currentLevel + "/" + prices.Count;
        }
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
