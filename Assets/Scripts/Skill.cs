using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public Stats stats;
    public int levelAmount = 1;
    public Skill skillParent;
    public bool isEnabled = false;
    public RectTransform popup;
    public Color lockedColor;
    public Color unlockedColor;
    public Color upgradedColor;
    public Color maxOutColor;
    private TextMeshProUGUI buttonText;
    private int currentLevel = 0;
    private Player player;
    private Button button;
    private Image image;
    
    void Start()
    {
        player = Player.I;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(AddStats);
        button.interactable = false;
        buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        popup.gameObject.SetActive(false);
        image = GetComponent<Image>();
    }
    void AddStats()
    {
        if (currentLevel < levelAmount)
        {
            isEnabled = true;
            player.stats += stats;
            currentLevel++;
            image.color = upgradedColor;
        }
        else
        {
            button.interactable = false;
            image.color = maxOutColor;
        }
    }

    private void Update()
    {
        if (skillParent == null || (skillParent.isEnabled && currentLevel < levelAmount))
        {
            button.interactable = true;
            if (currentLevel < 1)
            {
                image.color = unlockedColor;
            }
        }else
        {
            button.interactable = false;
            if (currentLevel == levelAmount)
            {
                image.color = maxOutColor;
            }
            else
            {
                image.color = lockedColor;
            }
        }
        buttonText.text = currentLevel.ToString() + "/" + levelAmount.ToString();
    }

    public void showPopup()
    {
        popup.gameObject.SetActive(true);
    }

    public void hidePopup()
    {
        popup.gameObject.SetActive(false);
    }
}
