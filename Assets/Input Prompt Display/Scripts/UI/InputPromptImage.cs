/* Input Prompt Display v.1.0.0
 * --------------------------------------------------------------------------------------------------------------------------------------------------
 * 
 * This file is part of Input Prompt Display, which is released under the Unity Asset Store End User License Agreement.
 * To view a copy of this license, visit https://unity3d.com/legal/as_terms
 * 
 * Input Prompt Display � 2023 by Flight Paper Studio LLC is licensed under CC BY 4.0. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 */

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace IPD
{
	/// <summary>
	/// This class controls displaying an input prompt.
	/// </summary>
	[RequireComponent ( typeof ( Image ) )]
	public class InputPromptImage : MonoBehaviour
	{
		#region UI Elements

		[SerializeField]
		private Image image;

		#endregion // UI Elements

		#region Key Icon Data

		[SerializeField]
		private DisplaySettingsModel settings;
		public DisplaySettingsModel CurrentSettings => settings;

		#endregion // Key Icon Data

		#region MonoBehaviour Functions

		private void Awake ( )
		{
			// Check for image
			if ( image == null )
			{
				image = GetComponent<Image> ( );
			}
			
			// Add subscriptions
			InputUser.onChange += OnInputDeviceChange;
		}

		private void OnDestroy ( )
		{
			// Remove subscriptions
			InputUser.onChange -= OnInputDeviceChange;
		}

		private async void Start ( )
		{
			// Check if data is loaded
			if ( InputPromptUtility.State != InputPromptUtility.LoadingState.LOADED )
			{
				// Check if currently loading
				if ( InputPromptUtility.State == InputPromptUtility.LoadingState.LOADING )
				{
					// Wait for loading to complete and then set prompt
					InputPromptUtility.AddOnLoadCompleteCallback ( ( ) =>
					{
						// Set prompt
						Initialize ( settings );
					} );

					// Hide the prompt until loading is complete
					image.gameObject.SetActive ( false );
					return;
				}
				else
				{
					// Wait for load
					await InputPromptUtility.Load ( );
				}
			}

			// Set the prompt
			Initialize ( settings );
		}

		#endregion // MonoBehaviour Functions

		#region Public Functions

		/// <summary>
		/// Sets and displays the input prompt.
		/// </summary>
		/// <param name="newSettings"> The settings data for displaying the input prompt. </param>
		public async void Initialize ( DisplaySettingsModel newSettings )
		{
			// Display the prompt by default
			image.gameObject.SetActive ( true );

			// Check for valid settings
			if ( settings == null || string.IsNullOrEmpty ( settings.Action ) )
			{
				// Log a warning for invalid settings
				LogIssue ( $"Invalid display settings provided", false );
				return;
			}

			// Store new settings
			settings = newSettings;

			// Get player
			if ( PlayerInput.all.Count < 1 )
			{
				// Log an error for missing player input
				LogIssue ( "There is no player input present", false, true );
				return;
			}
			else if ( settings.PlayerIndex >= PlayerInput.all.Count )
			{
				// Log a warning for missing player input
				LogIssue ( $"There is only {PlayerInput.all.Count} player input(s) present", false );
				return;
			}
			settings.PlayerIndex = Mathf.Clamp ( settings.PlayerIndex, 0, PlayerInput.all.Count - 1 );
			PlayerInput player = PlayerInput.GetPlayerByIndex ( settings.PlayerIndex );

			// Check control scheme
			if ( settings.HideForControlSchemes != null && settings.HideForControlSchemes.Length > 0 )
			{
				// Check if the current control scheme should hide the prompt
				for ( int i = 0; i < settings.HideForControlSchemes.Length; i++ )
				{
					if ( settings.HideForControlSchemes [ i ] == player.currentControlScheme )
					{
						// Hide the prompt
						image.gameObject.SetActive ( false );
						return;
					}
				}
			}

			// Get action
			InputAction action = player.currentActionMap.FindAction ( settings.Action );
			if ( action == null )
			{
				// Log a warning for a missing action
				LogIssue ( $"The action '{settings.Action}' was not found in the current action map '{player.currentActionMap.name}'", false );
				return;
			}

			// Get control path
			InputControl control = null;
			if ( action.controls.Count < 1 )
			{
				// Log a warning for missing controls
				LogIssue ( $"The action '{settings.Action}' has no controls bound for the control scheme '{player.currentControlScheme}'", settings.IsUnassignedDisplayed );
				return;
			}
			// Check for composite binding
			else if ( !string.IsNullOrEmpty ( settings.Composite ) )
			{
				// Track alternative bindings
				int altBindingTracker = 0;

				// Check each control for the correct composite binding
				for ( int i = 0; i < action.controls.Count; i++ )
				{
					// Get binding
					InputBinding binding = (InputBinding)action.GetBindingForControl ( action.controls [ i ] );

					// Check for matching composite
					if ( binding != null && settings.Composite == binding.name )
					{
						// Check for alternative bindings
						if ( settings.AltBindingIndex == altBindingTracker )
						{
							// Store composite binding
							control = action.controls [ i ];
							break;
						}
						else
						{
							// Continue search for alternative binding
							altBindingTracker++;
						}
					}
				}

				// Check for control
				if ( control == null )
				{
					// Check if any composite was found
					if ( altBindingTracker > 0 )
					{
						// Log a warning for missing alternative composite binding
						LogIssue ( $"The action '{settings.Action}' only has {altBindingTracker} composite '{settings.Composite}' binding(s) for the control scheme '{player.currentControlScheme}'", settings.IsUnassignedDisplayed );
					}
					else
					{
						// Log a warning for missing composite binding
						LogIssue ( $"The action '{settings.Action}' does not contain the composite '{settings.Composite}' binding for the control scheme '{player.currentControlScheme}'", settings.IsUnassignedDisplayed );
					}
					return;
				}
			}
			else
			{
				// Check for alternative bindings
				if ( settings.AltBindingIndex >= action.controls.Count )
				{
					// Log a warning for missing alternate binding
					LogIssue ( $"The action '{settings.Action}' only has {action.controls.Count} control(s) for the control scheme '{player.currentControlScheme}'", settings.IsUnassignedDisplayed );
					return;
				}

				// Store non-composite control
				control = action.controls [ settings.AltBindingIndex ];
			}

			// Get the prompt
			PromptModel prompt = InputPromptUtility.GetPrompt ( player.currentControlScheme, control, settings.Style );
			if ( prompt == null )
			{
				// Log a warning for a missing prompt
				LogIssue ( "No prompt was found", settings.IsUnassignedDisplayed );
			}
			else
			{
				// Display the prompt
				await SetImage ( prompt );
			}
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
			if ( change == InputUserChange.ControlSchemeChanged )
			{
				// Update the prompt for the new control scheme
				Initialize ( settings );
			}
		}

		/// <summary>
		/// Logs an issue with data and hides the image.
		/// </summary>
		/// <param name="message"> The message to log. </param>
		/// <param name="displayUnassignedPrompt"> Whether the unassigned prompt should be displayed (true) or element should be hidden (false). </param>
		/// <param name="isError"> Whether or not the issue should be logged as an error (true) or as a warning (false). </param>
		private async void LogIssue ( string message, bool displayUnassignedPrompt, bool isError = false )
		{
			// Check for error
			if ( isError )
			{
				// Log an error
				Debug.LogError ( message );
			}
			else
			{
				// Log a warning
				Debug.Log ( message );
			}

			// Check if the unassigned prompt should be displayed
			if ( displayUnassignedPrompt )
			{
				// Get and display unassigned prompt
				await SetImage ( InputPromptUtility.GetUnassignedPrompt ( settings.Style ) );
			}
			else
			{
				// Hide the prompt
				image.gameObject.SetActive ( false );
			}
		}

		/// <summary>
		/// Set the image element to display from a prompt model.
		/// </summary>
		/// <param name="model"> The model for the prompt. </param>
		private async Task SetImage ( PromptModel model )
		{
			// Check for valid model
			if ( model == null || !model.IsValid ( ) )
			{
				// Hide element
				image.gameObject.SetActive ( false );
				return;
			}

			// Get the sprite
			Sprite sprite = await model.LoadSpriteAsync ( );

			// Check for sprite
			if ( sprite == null )
			{
				// Hide element
				image.gameObject.SetActive ( false );
				return;
			}

			// Set sprite
			image.sprite = sprite;
		}

		#endregion // Private Functions
	}
}