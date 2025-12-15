using System;
using UnityEngine;

public class UpgradesOpener: MonoBehaviour, IInteractionHandler
{
    private void Awake()
    {
        Interact();
    }

    public void Interact()
    {
        UpgradesController.I.ShowUpgrades();
    }
}