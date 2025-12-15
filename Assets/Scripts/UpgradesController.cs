using System;
using IPD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UpgradesController: MonoBehaviour
{
    public static UpgradesController I;
    public Canvas upgradeUI;
    
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
        
}