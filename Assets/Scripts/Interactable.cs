using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    public GameObject tooltip;
    
    private bool isPlayerInRange = false;
    private InputAction interactAction;
    private IInteractionHandler[] interactionHandlers;
    private bool isInteractable = true;
    public bool IsInteractable
    {
        get => isInteractable;
        set
        {
            isInteractable = value;
            if (!isInteractable)
            {
                tooltip.SetActive(false);
                isPlayerInRange = false;
            }
        }
    }
    
    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        interactionHandlers = GetComponents<IInteractionHandler>();
        tooltip.SetActive(false);
    }

    private void LateUpdate()
    {
        if (isPlayerInRange && interactAction.WasPerformedThisFrame())
        {
            foreach (var interactionHandler in interactionHandlers)
            {
                interactionHandler.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteractable)
        {
            return;
        }
        isPlayerInRange = true;
        tooltip.SetActive(true);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        isPlayerInRange = false;
        tooltip.SetActive(false);
    }
}
