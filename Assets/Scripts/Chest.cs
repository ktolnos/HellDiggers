using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Chest : MonoBehaviour, IInteractionHandler
{
    private Interactable interactable;
    public GameObject dropPrefab;
    SpriteAnimator animator;
    
    public void Interact()
    {
        GameObjectPoolManager.I.GetOrRegisterPool(dropPrefab, Level.I.pooledObjectsParent).InstantiateTemporarily(
            transform.position, transform.rotation, time:1000f);
        TryGetComponent(out animator);
        animator.PlayOnce();
        TryGetComponent(out interactable);
        interactable.IsInteractable = false;
    }
}
