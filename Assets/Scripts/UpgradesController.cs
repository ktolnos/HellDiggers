using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradesController: MonoBehaviour
{
    public static UpgradesController I;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button playAgainButton;
    
    public bool IsActive => upgradeUI.activeSelf;
    
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

    private void Update()
    {
        if (!IsActive)
        {
            return;
        }
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            playAgainButton.Select();
        }
    }

    public void ShowUpgrades()
    {
        upgradeUI.SetActive(true);
        playAgainButton.Select();
    }
    
    public void HideUpgrades()
    {
        upgradeUI.SetActive(false);
    }
        
}