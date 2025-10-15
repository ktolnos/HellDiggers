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
    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
        lastLook = Vector2.right;
    }
    private void LateUpdate()
    {
        var isDead = !Player.I.IsAlive;

        aimImage.enabled = InputController.I.CurrentInputType == InputController.InputType.Gamepad && !isDead;
        
        if (InputController.I.CurrentInputType == InputController.InputType.Gamepad)
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
}