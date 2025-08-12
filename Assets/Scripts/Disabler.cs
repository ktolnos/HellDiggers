using System;
using UnityEngine;

public class Disabler: MonoBehaviour
{
    public float delay;
    public MonoBehaviour component;

    private void OnEnable()
    {
        component.enabled = true;
        Invoke(nameof(DisableComponent), delay);
    }
    
    private void DisableComponent()
    {
        if (component != null)
        {
            component.enabled = false;
        }
        else
        {
            Debug.LogWarning("Disabler: Component is null, cannot disable.");
        }
    }
    
}