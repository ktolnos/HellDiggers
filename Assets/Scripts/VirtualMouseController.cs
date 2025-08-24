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
    
    public VirtualMouseInput input;
    public Image virtualMouseImage;
    public Image aimImage;
    public Canvas canvas;
    private float offset = 32f;
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

    private void Update()
    {
        input.transform.localScale = Vector3.one * (1f / canvas.scaleFactor);
    }

    private void LateUpdate()
    {
        Cursor.visible = mouse;
        var isDead = !Player.I.IsAlive;

        var useVirtualMouse = !mouse && isDead;
        virtualMouseImage.enabled = useVirtualMouse;
        aimImage.enabled = !mouse && !isDead;
        if (useVirtualMouse)
        {
            input.leftButtonAction.action.Enable();
            input.stickAction.action.Enable();
        }
        else
        {
            input.leftButtonAction.action.Disable();
            input.stickAction.action.Disable();
        }
        
        if (useVirtualMouse)
        {
            Vector2 virtualMousePosition = input.virtualMouse.position.ReadValue();
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, offset, Screen.width - offset);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, offset, Screen.height - offset);
            InputState.Change(input.virtualMouse.position, virtualMousePosition);
        }
        else
        {
            if (!mouse)
            {
                var look = _lookAction.ReadValue<Vector2>();
                if (look.magnitude > 0.3f)
                {
                    lastLook = look;
                }
                mousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f) + lastLook.normalized * (aimRadius * canvas.scaleFactor);
                aimImage.rectTransform.anchoredPosition = lastLook.normalized * (aimRadius * aimImageDistanceMult);
            }
            else
            {
                mousePosition = Mouse.current.position.ReadValue();
            }
            
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