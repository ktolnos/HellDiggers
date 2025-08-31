using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class VirtualMouseController : MonoBehaviour
{
    public static VirtualMouseController I;
    
    public Image aimImage;
    public float aimRadius = 5f;
    public float aimImageDistanceMult = 5f;
    private InputAction _lookAction;
    private Vector2 lastLook;

    public Vector2 mousePosition;
    private bool mouse = true;
    
    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
        lastLook = Vector2.right;
        InputSystem.onActionChange += OnActionChange;
    }
    private void LateUpdate()
    {
        Cursor.visible = mouse;
        var isDead = !Player.I.IsAlive;

        aimImage.enabled = !mouse && !isDead;
        
        if (!mouse)
        {
            var look = _lookAction.ReadValue<Vector2>();
            if (look.magnitude > 0.3f)
            {
                lastLook = look;
            }
            mousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f) + lastLook.normalized * aimRadius;
            aimImage.rectTransform.anchoredPosition = lastLook.normalized * (aimRadius * aimImageDistanceMult);
        }
        else
        {
            mousePosition = Mouse.current.position.ReadValue();
        }
    }
    
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted && obj is InputAction action)
        {
            if (action.activeControl.device.displayName == "VirtualMouse")
            {
                return;
            }

            var newMouse = action.activeControl.device is not Gamepad;
            if (mouse != newMouse)
            {
                Debug.Log($"VirtualMouseController: Mouse input changed to {newMouse}");
            }
            mouse = newMouse;
            Cursor.visible = mouse;
        }
    }
}