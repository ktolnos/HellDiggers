using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedByDefault : MonoBehaviour
{
    public Selectable selectable;
    public bool onlyOnGamepad = false;

    private void Awake()
    {
        if (selectable == null)
        {
            selectable = GetComponent<Selectable>();
        }
    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null && 
            (!onlyOnGamepad || InputController.I.CurrentInputType == InputController.InputType.Gamepad))
        {
            selectable.Select();
        }
    }
}
