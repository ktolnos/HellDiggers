using System;
using IPD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UpgradesController: MonoBehaviour
{
    public static UpgradesController I;
    public Canvas upgradeUI;
    private InputAction closeUpgradesAction;
    
    public bool IsActive => upgradeUI.gameObject.activeSelf;
    
    private void Awake()
    {
        I = this;
        closeUpgradesAction = InputSystem.actions.FindAction("Cancel");
    }
    
    private void Start()
    {
        if (IsActive)
        {
            ShowUpgrades();
        }
    }

    private void Update()
    {
        if (!IsActive)
        {
            return;
        }
        if (closeUpgradesAction.WasPerformedThisFrame())
        {
            HideUpgrades();
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
        GM.OnUIOpen();
    }
    
    public void HideUpgrades()
    {
        if (!IsActive)
        {
            return;
        }
        upgradeUI.gameObject.SetActive(false);
        GM.OnUIClose();
    }
        
}