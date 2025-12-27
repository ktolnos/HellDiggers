using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Skill : MonoBehaviour, ISelectHandler, IDeselectHandler 
{
    public LocalizedString skillName;
    public LocalizedString description;
    public Stats stats;
    [HideInInspector] public Skill skillParent;
    
    public Color lockedColor;
    public Color unlockedColor;
    [FormerlySerializedAs("upgradedColor")] public Color tooExpensiveColor;
    public Color maxOutColor;
    public int GlobalLevel => (int) myStatField.GetValue(Player.I.stats);
    public int LocalLevel => Mathf.Clamp(GlobalLevel - levelOffset, 0, levelsInThisNode);
    public int levelOffset = 0;
    public int levelsInThisNode = 1;
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
    private Sprite icon;
    public Sprite lockedIcon;
    private State currentState = State.Hidden;
    
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
        icon = iconImage.sprite;
    }

    void AddStats()
    {
        if (!interactable) return;
        if (!GM.I.isFree)
        {
        if (LocalLevel >= levelsInThisNode) return;
        if (!GM.I.isFree)
        {
            if (GlobalLevel >= prices.Count) return;
            GM.I.money -= prices[GlobalLevel];
        }
        }
        
        player.stats += stats;
        SaveManager.I.SaveGame();
    }

    private bool UnlockRequirementsMet()
    {
        // Check dependencies:
        // 1. Must satisfy linear progression (GlobalLevel >= levelOffset)
        // 2. Physical parent interaction
        bool sequenceMet = GlobalLevel >= levelOffset;
        bool parentMet = skillParent == null || skillParent.LocalLevel > 0;
        return parentMet && sequenceMet;
    }

    private void Update()
    {
        if (LocalLevel >= levelsInThisNode)
        {
            SetState(State.Maxed);
        }
        else if (UnlockRequirementsMet())
        {
            var canAfford = false;
            if (GlobalLevel < prices.Count)
            {
                canAfford = prices[GlobalLevel] < GM.I.money;
            }
            canAfford |= GM.I.isFree; 
            SetState(canAfford ? State.Unlocked : State.TooExpensive);
        }
        else if (skillParent.UnlockRequirementsMet())
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
    
    private enum State
    {
        Hidden,
        Locked,
        Unlocked,
        TooExpensive,
        Maxed,
    }
}
