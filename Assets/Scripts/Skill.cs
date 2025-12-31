using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Skill : MonoBehaviour, ISelectHandler, IDeselectHandler 
{
    public string upgradeId;
    public LocalizedString skillName;
    public LocalizedString description;
    
    [Header("Stats Configuration")]
    public Stats stats;        // Flat additions per level
    public UpgradeType upgradeType;
    
    public List<Skill> prerequisites;
    public Skill skillParent;
    
    public Color lockedColor;
    public Color unlockedColor;
    [FormerlySerializedAs("upgradedColor")] public Color tooExpensiveColor;
    public Color maxOutColor;

    public int currentLevel;
    public int MaxLevel => prices.Count;
    
    private Player player;
    [HideInInspector] public Button button;
    public Image statusIndicator;
    public List<int> prices;
    private FieldInfo myStatField;
    [HideInInspector] public RectTransform rectTransform;
    public Image selectedIndicator;
    private bool interactable;
    public RectTransform connector;
    public Image iconImage;
    public Sprite icon;
    public Sprite lockedIcon;
    private State currentState = State.Hidden;
    
    private void Awake()
    {
        foreach (var fieldInfo in typeof(Stats).GetFields())
        {
            if (fieldInfo.GetValue(stats) is not 0)
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
        if (iconImage != null) iconImage.sprite = icon;
    }
    
    void Start()
    {
        player = Player.I;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(BuySkill);
    }

    void BuySkill()
    {
        if (!interactable) return;
        if (currentLevel >= MaxLevel) return;
        
        if (!GM.I.isFree)
        {
            int price = prices[currentLevel];
            if (GM.I.money < price) return;
            GM.I.money -= price;
        }
        
        currentLevel++;
        UpgradesController.I.OnSkillPurchased(); // Trigger global stats update
        SaveManager.I.SaveGame();
    }

    public bool UnlockRequirementsMet()
    {
        bool parentMet = skillParent == null || skillParent.currentLevel > 0;
        return parentMet;
    }

    private void Update()
    {
        if (currentLevel >= MaxLevel)
        {
            SetState(State.Maxed);
        }
        else if (UnlockRequirementsMet())
        {
            var canAfford = false;
            if (currentLevel < prices.Count)
            {
                canAfford = prices[currentLevel] < GM.I.money;
            }
            canAfford |= GM.I.isFree; 
            SetState(canAfford ? State.Unlocked : State.TooExpensive);
        }
        else if (skillParent != null && skillParent.UnlockRequirementsMet())
        {
            SetState(State.Locked);
        }
        else
        {
            SetState(State.Hidden);
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
        if (currentState == State.Hidden || currentState == State.Locked) return;
        button.Select();
        SkillPopup.I.Show(this);
        selectedIndicator.enabled = true;
        if (InputController.I.CurrentInputType == InputController.InputType.Gamepad)
        {
            UpgradesPanel.I.CenterOnSkill(this);
        }
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
    
    private void SetState(State state)
    {
        currentState = state;
        if (state != State.Hidden)
        {
            button.gameObject.SetActive(true);
            connector.gameObject.SetActive(skillParent != null);
        }
        button.interactable = state == State.Unlocked;
        interactable = state == State.Unlocked;
        iconImage.sprite = state == State.Locked ? lockedIcon : icon;
        switch (state)
        {
            case State.Hidden:
                button.gameObject.SetActive(false);
                if (connector != null)
                {
                    connector.gameObject.SetActive(false);
                }
                break;
            case State.Locked:
                statusIndicator.color = lockedColor;
                break;
            case State.Unlocked:
                statusIndicator.color = unlockedColor;
                break;
            case State.TooExpensive:
                statusIndicator.color = tooExpensiveColor;
                break;
            case State.Maxed:
                statusIndicator.color = maxOutColor;
                break;
        }
    }

    public string GetByText()
    {
        var amount = myStatField.GetValue(stats);
        var sign = amount is > 0 ? "+" : "-";
        var text = $"{sign}{amount}";
        if (upgradeType == UpgradeType.Percentage)
        {
            return text + "%";
        }

        return text;
    }
    
    private enum State
    {
        Hidden,
        Locked,
        Unlocked,
        TooExpensive,
        Maxed,
    }
    
    public enum UpgradeType
    {
        Flat,
        Percentage
    }
}
