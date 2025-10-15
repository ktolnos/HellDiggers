using UnityEngine;
using UnityEngine.InputSystem;

public class InputController: MonoBehaviour
{
    public static InputController I;
    
    private void Awake()
    {
        I = this;
        CurrentInputType = InputType.Mouse;
        Cursor.visible = true;
    }
    
    private void Start()
    {
        InputSystem.onActionChange += OnActionChange;
    }
    
    public InputType CurrentInputType { get; private set; }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted && obj is InputAction action)
        {
            if (action.activeControl.device.displayName == "VirtualMouse")
            {
                return;
            }

            var newType = action.activeControl.device is Gamepad ? InputType.Gamepad : InputType.Mouse;
            if (CurrentInputType != newType)
            {
                Debug.Log($"Input changed to {newType}");
            }
            CurrentInputType = newType;
            Cursor.visible = CurrentInputType == InputType.Mouse;
        }
    }
    
    public enum InputType
    {
        Mouse,
        Gamepad
    }
}