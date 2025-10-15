/* Input Prompt Display v.1.0.0
 * --------------------------------------------------------------------------------------------------------------------------------------------------
 * 
 * This file is part of Input Prompt Display, which is released under the Unity Asset Store End User License Agreement.
 * To view a copy of this license, visit https://unity3d.com/legal/as_terms
 * 
 * Input Prompt Display © 2023 by Flight Paper Studio LLC is licensed under CC BY 4.0. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 */

using UnityEngine;

namespace IPD
{
	/// <summary>
	/// This class stores the settings for displaying an input prompt.
	/// </summary>
	[System.Serializable]
	public class DisplaySettingsModel
	{
		#region Setting Data

		[Header ( "Settings Data" )]

		/// <summary>
		/// The action being displayed.
		/// </summary>
		[Tooltip ( "The action being displayed." )]
		public string Action;

		/// <summary>
		/// The name of the composite part binding for the action.
		/// Leave empty for non-composite bindings for an action.
		/// </summary>
		[Tooltip ( "The name of the composite part binding for the action. Leave empty for non-composite bindings for an action." )]
		public string Composite;

		/// <summary>
		/// The style for the prompt.
		/// </summary>
		[Tooltip ( "The style for the prompt." )]
		[HideInInspector] public string Style = "xelu";

		#endregion // Setting Data

		#region Optional Data

		[Header ( "Optional Data" )]

		/// <summary>
		/// Whether or not the unassigned prompt should be displayed if a binding is not found.  
		/// Set to true as default. Set to false to hide the element instead.
		/// </summary>
		[Tooltip ( "Whether or not the unassigned prompt should be displayed if a binding is not found. Set to true as default. Set to false to hide the element instead." )]
		public bool IsUnassignedDisplayed = true;

		/// <summary>
		/// The index of the player bindings to display.
		/// Set to 0 as default. Increase to display bindings for an action of an additional player.
		/// </summary>
		[Min ( 0 )]
		[Tooltip ( "The index of the player bindings to display. Set to 0 as default. Increase to display bindings for an action of an additional player." )]
		public int PlayerIndex = 0;

		/// <summary>
		/// The index of the binding to display.
		/// Set to 0 as default. Increase to display alternative bindings for an action.
		/// </summary>
		[Min ( 0 )]
		[Tooltip ( "The index of the binding to display. Set to 0 as default. Increase in case of displaying alternative bindings for an action." )]
		public int AltBindingIndex = 0;

		/// <summary>
		/// The list of control schemes that this input prompt should be hidden for.
		/// This is useful for hiding input prompts for mouse or touch inputs.
		/// </summary>
		[Tooltip ( "The list of control schemes that this input prompt should be hidden for. This is useful for hiding input prompts for mouse or touch inputs." )]
		public string [ ] HideForControlSchemes;

		#endregion // Optional Data
	}
}