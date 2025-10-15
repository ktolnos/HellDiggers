/* Input Prompt Display v.1.0.0
 * --------------------------------------------------------------------------------------------------------------------------------------------------
 * 
 * This file is part of Input Prompt Display, which is released under the Unity Asset Store End User License Agreement.
 * To view a copy of this license, visit https://unity3d.com/legal/as_terms
 * 
 * Input Prompt Display © 2023 by Flight Paper Studio LLC is licensed under CC BY 4.0. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 */

using IPD;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

/// <summary>
/// A class for controlling the example scene included with the Input Prompt Display.
/// </summary>
public class Example : MonoBehaviour
{
	#region Example Data Constants

	private const string NAVIGATE_ACTION = "Navigate";
	private const string UP_COMPOSITE = "up";
	private const string DOWN_COMPOSITE = "down";
	private const string LEFT_COMPOSITE = "left";
	private const string RIGHT_COMPOSITE = "right";
	private const string SUBMIT_ACTION = "Submit";
	private const string CANCEL_ACTION = "Cancel";

	private const string XELU_STYLE = "xelu";
	private const string JSTATZ_STYLE = "JStatz";

	#endregion // Example Data Constants

	#region UI Elements

	[SerializeField]
	private Button xeluStyleButton;

	[SerializeField]
	private Button jstatzStyleButton;

	[SerializeField]
	private InputPromptImage navigateUpPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateUpSecondaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateDownPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateDownSecondaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateLeftPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateLeftSecondaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateRightPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage navigateRightSecondaryInputPrompt;

	[SerializeField]
	private InputPromptImage submitPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage submitSecondaryInputPrompt;

	[SerializeField]
	private InputPromptImage cancelPrimaryInputPrompt;

	[SerializeField]
	private InputPromptImage cancelSecondaryInputPrompt;

	[SerializeField]
	private TextMeshProUGUI controlSchemeText;

	[SerializeField]
	private TextMeshProUGUI deviceNameText;

	[SerializeField]
	private TextMeshProUGUI devicePathText;

	#endregion // UI Elements

	#region Example Data

	[SerializeField]
	private bool isDebugControlsOn = true;

	private InputDevice currentDevice = null;

	#endregion // Example Data

	#region MonoBehaviour Functions

	private async void Awake ( )
	{
		// Load input prompt data
		await InputPromptUtility.Load ( );

		// Update the UI
		DebugControlSchemeChange ( InputUser.all [ 0 ], InputUser.all [ 0 ].pairedDevices [ 0 ] );
	}

	private void OnEnable ( )
	{
		// Add subscriptions
		InputUser.onChange += OnInputDeviceChange;
	}

	private void OnDisable ( )
	{
		// Remove subscriptions
		InputUser.onChange -= OnInputDeviceChange;
	}

	#endregion // MonoBehaviour Functions

	#region Public Functions

	/// <summary>
	/// Sets the styles for the input prompts displayed.
	/// </summary>
	/// <param name="style"> The chosen style. </param>
	public void SetStyle ( string style )
	{
		// Set buttons
		xeluStyleButton.interactable = style != XELU_STYLE;
		jstatzStyleButton.interactable = style != JSTATZ_STYLE;

		// Update navigate up action input prompts
		navigateUpPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = UP_COMPOSITE,
			Style = style
		} );
		navigateUpSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = UP_COMPOSITE,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );

		// Update navigate down action input prompts
		navigateDownPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = DOWN_COMPOSITE,
			Style = style
		} );
		navigateDownSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = DOWN_COMPOSITE,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );

		// Update navigate left action input prompts
		navigateLeftPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = LEFT_COMPOSITE,
			Style = style
		} );
		navigateLeftSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = LEFT_COMPOSITE,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );

		// Update navigate right action input prompts
		navigateRightPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = RIGHT_COMPOSITE,
			Style = style
		} );
		navigateRightSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = NAVIGATE_ACTION,
			Composite = RIGHT_COMPOSITE,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );

		// Update submit action input prompts
		submitPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = SUBMIT_ACTION,
			Style = style
		} );
		submitSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = SUBMIT_ACTION,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );

		// Update cancel action input prompts
		cancelPrimaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = CANCEL_ACTION,
			Style = style
		} );
		cancelSecondaryInputPrompt.Initialize ( new DisplaySettingsModel
		{
			Action = CANCEL_ACTION,
			Style = style,
			IsUnassignedDisplayed = false,
			AltBindingIndex = 1
		} );
	}

	#endregion // Public Functions

	#region Private Functions

	/// <summary>
	/// The callback for when the input system registers an input device change.
	/// </summary>
	/// <param name="user"> The current input user. </param>
	/// <param name="change"> The type of input system change event. </param>
	/// <param name="device"> The current input device. </param>
	private void OnInputDeviceChange ( InputUser user, InputUserChange change, InputDevice device )
	{
		// Check for device change
		if ( change == InputUserChange.DevicePaired )
		{
			// Store current device
			currentDevice = device;

			// Check debug setting
			if ( isDebugControlsOn )
			{
				DebugDevicePaired ( currentDevice );
			}
		}
		else if ( change == InputUserChange.DeviceUnpaired )
		{
			// Clear current device
			currentDevice = null;
		}
		else if ( change == InputUserChange.ControlSchemeChanged )
		{
			// Update the UI
			DebugControlSchemeChange ( user, currentDevice );
		}
	}

	/// <summary>
	/// Logs all of the available controls in the input system for a given device.
	/// </summary>
	/// <param name="device"> The currently paired device. </param>
	private void DebugDevicePaired ( InputDevice device )
	{
		// Log the device paired
		Debug.Log ( $"Paired: {device}" );

		// Log each control for the device
		for ( int i = 0; i < device.allControls.Count; i++ )
		{
			Debug.Log ( $"Control Name: {device.allControls [ i ].name} | Path: {device.allControls [ i ].path}" );
		}
	}

	/// <summary>
	/// Updates the debug UI elements for when the control scheme changes.
	/// </summary>
	/// <param name="user"> The current input user. </param>
	/// <param name="device"> The current device paired. </param>
	private void DebugControlSchemeChange ( InputUser user, InputDevice device )
	{
		// Set control scheme
		controlSchemeText.text = $"Control Scheme:<color=#FFC300> {user.controlScheme.Value.name}";

		// Set device name
		deviceNameText.text = $"Device Name:<color=#FFC300> {device.displayName}";

		// Set device path
		devicePathText.text = $"Device Path:<color=#FFC300> {device.path}";
	}

	#endregion // Private Functions
}
