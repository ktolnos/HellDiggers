/* Input Prompt Display v.1.0.0
 * --------------------------------------------------------------------------------------------------------------------------------------------------
 * 
 * This file is part of Input Prompt Display, which is released under the Unity Asset Store End User License Agreement.
 * To view a copy of this license, visit https://unity3d.com/legal/as_terms
 * 
 * Input Prompt Display � 2023 by Flight Paper Studio LLC is licensed under CC BY 4.0. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IPD
{
	/// <summary>
	/// This class controls loading and storing the scriptable objects for the input prompt system.
	/// </summary>
	public static class InputPromptUtility
	{
		#region Input Prompt Data Constants

		private const string INPUT_PROMPT_LABEL = "Input Prompt";

		private const string WINDOWS_PLATFORM_KEYWORD = "WINDOWS";
		private const string MAC_PLATFORM_KEYWORD = "MAC";
		private const string XBOX_ONE_PLATFORM_KEYWORD = "XBOX ONE";
		private const string XBOX_SERIES_PLATFORM_KEYWORD = "XBOX SERIES";
		private const string PS4_PLATFORM_KEYWORD = "PS4";
		private const string PS5_PLATFORM_KEYWORD = "PS5";
		private const string SWITCH_PLATFORM_KEYWORD = "SWITCH";

		#endregion // Input Prompt Data Constants

		#region Input Prompt Enums

		/// <summary>
		/// The listed states for loading the input prompt data in the utility.
		/// </summary>
		public enum LoadingState
		{
			UNLOADED,
			LOADING,
			LOADED
		}

		#endregion // Input Prompt Enums

		#region Input Prompt Data

		private static MasterMappingsScriptableObject masterMappings;
		private static readonly Dictionary<string, ControlSchemeMappingScriptableObject> schemeDictionary = new Dictionary<string, ControlSchemeMappingScriptableObject> ( );
		private static LoadingState state = LoadingState.UNLOADED;
		private static System.Action onLoadComplete;

		#endregion // Input Prompt Data

		#region Public Properties

		/// <summary>
		/// The state of loading the addressables for the utility.
		/// </summary>
		public static LoadingState State
		{
			get
			{
				return state;
			}
		}

		#endregion // Public Properties

		#region Public Functions

		/// <summary>
		/// Asynchrously loads the master mappings scriptable object from addressables.
		/// </summary>
		/// <returns> Whether or not the addressables were succesfully loaded. </returns>
		public static async Task<bool> Load ( )
		{
			// Clear previous data
			state = LoadingState.LOADING;
			bool isLoaded = false;

			// Load master mappings from addressables
			AsyncOperationHandle<MasterMappingsScriptableObject> handler = Addressables.LoadAssetAsync<MasterMappingsScriptableObject> ( INPUT_PROMPT_LABEL );
			await handler.Task;

			// Check for successful load
			if ( handler.Status == AsyncOperationStatus.Succeeded )
			{
				// Mark successful load
				isLoaded = true;

				// Store master mappings
				masterMappings = handler.Result;
				for ( int i = 0; i < masterMappings.Schemes.Length; i++ )
				{
					schemeDictionary.Add ( masterMappings.Schemes [ i ].Scheme, masterMappings.Schemes [ i ] );
				}
			}

			// Update whether or not the mappings were loaded
			state = isLoaded ? LoadingState.LOADED : LoadingState.UNLOADED;
			if ( isLoaded )
			{
				// Trigger callback
				if ( onLoadComplete != null )
				{
					onLoadComplete ( );
					onLoadComplete = null;
				}
			}

			// Return whether or not the mappings were successfully loaded
			state = isLoaded ? LoadingState.LOADED : LoadingState.UNLOADED;
			return isLoaded;
		}

		/// <summary>
		/// Adds a callback to the On Load Complete event.
		/// The event clears all callbacks after triggering.
		/// </summary>
		/// <param name="onComplete"> The callback to be added. </param>
		public static void AddOnLoadCompleteCallback ( System.Action onComplete )
		{
			onLoadComplete += onComplete;
		}

		/// <summary>
		/// Removes a callback from the On Load Complete event.
		/// The event clears all callbacks after triggering.
		/// </summary>
		/// <param name="onComplete"> The callback to be removed. </param>
		public static void RemoveOnLoadCompleteCallback ( System.Action onComplete )
		{
			if ( onComplete != null )
			{
				onLoadComplete -= onComplete;
			}
		}

		/// <summary>
		/// Returns the prompt model for a given control scheme, control, and style. 
		/// </summary>
		/// <param name="scheme"> The current control scheme. </param>
		/// <param name="control"> The input control for the action. </param>
		/// <param name="style"> The desired style for the prompt. </param>
		/// <returns> The matching prompt model. </returns>
		public static PromptModel GetPrompt ( string scheme, InputControl control, string style )
		{
			// Check if loaded
			if ( state != LoadingState.LOADED )
			{
				// Log an error for unloaded mapping
				Debug.LogError ( "The input prompt mappings have not been loaded" );
				return null;
			}

			// Get the control scheme mappings
			ControlSchemeMappingScriptableObject schemeMapping = GetControlSchemeMapping ( scheme );

			// Check for matching control scheme
			if ( schemeMapping == null )
			{
				// Log a warning for a missing control scheme
				Debug.Log ( $"Control scheme '{scheme}' was not found" );
				return null;
			}

			// Get the key icon mapping
			InputModel mapping = null;
			for ( int i = 0; i < schemeMapping.InputPrompts.Length; i++ )
			{
				// Check for matching path
				// NOTE: Both Match() and MatchPrefix() are used to avoid false positives when accounting for parent and child controls
				// Using Match() on /XInputControllerWindows/dpad returns true for both <Gamepad>/dpad and <Gamepad>/dpad/up
				// Using MatchPrefix() on /XInputControllerWindows/dpad/up returns true for both <Gamepad>/dpad and <Gamepad>/dpad/up
				if ( InputControlPath.Matches ( schemeMapping.InputPrompts [ i ].Path, control ) && InputControlPath.MatchesPrefix ( schemeMapping.InputPrompts [ i ].Path, control ) )
				{
					mapping = schemeMapping.InputPrompts [ i ];
					break;
				}
			}

			// Check for matching mapping
			if ( mapping == null )
			{
				// Log a warning for a missing key mapping
				Debug.Log ( $"Key mapping '{control}' was not found for control scheme '{scheme}'" );
				return null;
			}

			// Get matching style for the key icon
			return GetPromptForStyle ( mapping.Prompts, style );
		}

		/// <summary>
		/// Gets the data for the unassigned prompt.
		/// </summary>
		/// <param name="style"> The desired style for the unassigned prompt. </param>
		/// <returns> The default unassigned prompt. </returns>
		public static PromptModel GetUnassignedPrompt ( string style )
		{
			// Check if utility is loaded
			if ( state != LoadingState.LOADED )
			{
				// Log an error for unloaded mapping
				Debug.LogError ( "The input prompt mappings have not been loaded" );
				return null;
			}

			// Check for unassigned prompt
			if ( masterMappings == null || masterMappings.UnassignedPrompt.Length < 1 )
			{
				// Log a warning that no unassigned prompt was found
				Debug.Log ( "No unassigned prompt was found" );
				return null;
			}

			// Return the prompt
			return GetPromptForStyle ( masterMappings.UnassignedPrompt, style );
		}

		/// <summary>
		/// Gets the data for a control scheme by its name.
		/// </summary>
		/// <param name="scheme"> The name of the control scheme. </param>
		/// <returns> The control scheme mapping scriptable object. </returns>
		public static ControlSchemeMappingScriptableObject GetControlSchemeMapping ( string scheme )
		{
			// Return the control scheme
			if ( state == LoadingState.LOADED && schemeDictionary.ContainsKey ( scheme ) )
			{
				return schemeDictionary [ scheme ];
			}

			// Return that no control scheme was found
			return null;
		}

		#endregion // Public Functions

		#region Private Functions

		/// <summary>
		/// Returns the prompt model with a matching style for a given set of prompts.
		/// </summary>
		/// <param name="prompts"> The given set of prompts. </param>
		/// <param name="style"> The desired style for the prompt. </param>
		/// <returns> The matching key icon model. </returns>
		private static PromptModel GetPromptForStyle ( PromptModel [ ] prompts, string style )
		{
			// Check for valid prompts
			if ( prompts == null || prompts.Length < 1 )
			{
				return null;
			}

			// Store prompt
			PromptModel model = null;

			// Store platform specific style
			string platformStyle = GetPlatformStyle ( style );

			// Search the prompts for matching style
			for ( int i = 0; i < prompts.Length; i++ )
			{
				// Check for matching platform specific style
				if ( !string.IsNullOrEmpty ( platformStyle ) && prompts [ i ].Style == platformStyle )
				{
					// Return the platform specific styled prompt
					return prompts [ i ];
				}

				// Check for style
				if ( prompts [ i ].Style == style )
				{
					// Store the styled key icon
					// Continue seaching in case there is a platform specific styled prompt to override
					model = prompts [ i ];
				}
			}

			// Check if no matching style was found
			if ( model == null )
			{
				// Log a warning for a missing style
				Debug.Log ( $"Style '{style}' was not found" );

				// Set the first prompt as the default
				model = prompts [ 0 ];
			}

			// Return the styled prompt
			return model;
		}

		/// <summary>
		/// Returns the formated style of a the current platform.
		/// </summary>
		/// <param name="style"> The current style. </param>
		/// <returns> The platform specific style. </returns>
		private static string GetPlatformStyle ( string style )
		{
			// Check for Windows
			if ( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor )
			{
				return WINDOWS_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for Mac
			else if ( Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
			{
				return MAC_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for Xbox One
			else if ( Application.platform == RuntimePlatform.XboxOne || Application.platform == RuntimePlatform.GameCoreXboxOne )
			{
				return XBOX_ONE_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for Xbox Series S|X
			else if ( Application.platform == RuntimePlatform.GameCoreXboxSeries )
			{
				return XBOX_SERIES_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for PS4
			else if ( Application.platform == RuntimePlatform.PS4 )
			{
				return PS4_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for PS5
			else if ( Application.platform == RuntimePlatform.PS5 )
			{
				return PS5_PLATFORM_KEYWORD + "/" + style;
			}
			// Check for Nintendo Switch
			else if ( Application.platform == RuntimePlatform.Switch )
			{
				return SWITCH_PLATFORM_KEYWORD + "/" + style;
			}

			// Return that the specifc platform was not found
			return string.Empty;
		}

		#endregion // Private Functions
	}
}