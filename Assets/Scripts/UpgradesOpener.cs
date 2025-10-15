using UnityEngine;

public class UpgradesOpener: MonoBehaviour, IInteractionHandler
{
    public void Interact()
    {
        UpgradesController.I.ShowUpgrades();
    }
}