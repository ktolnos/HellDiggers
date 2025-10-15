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
using UnityEngine.Serialization;

namespace IPD
{
	/// <summary>
	/// The scriptable object stores the master data for the input prompt mappings for each supported control scheme.
	/// </summary>
	[CreateAssetMenu ( fileName = "Master Mappings", menuName = "Scriptable Objects/Input Prompt/Master Mappings" )]
	public class MasterMappingsScriptableObject : ScriptableObject
	{
		#region Mapping Data

		[Tooltip ( "The prompt to display for unassigned or unknown key bindings." )]
		[NonReorderable] // HACK: Ensure proper rendering in editor
		[SerializeField]
		[FormerlySerializedAs("unassignedIcon")]
		private PromptModel [ ] unassignedPrompt;

		[Tooltip ( "The input prompt mapping data for each control scheme." )]
		[SerializeField]
		private ControlSchemeMappingScriptableObject [ ] schemes;

		#endregion // Mapping Data

		#region Public Properties

		/// <summary>
		/// The prompt to display for unassigned or unknown key bindings.
		/// </summary>
		public PromptModel [ ] UnassignedPrompt
		{
			get
			{
				return unassignedPrompt;
			}
		}

		/// <summary>
		/// The input prompt mapping data for each control scheme.
		/// </summary>
		public ControlSchemeMappingScriptableObject [ ] Schemes
		{
			get
			{
				return schemes;
			}
		}

		#endregion // Public Properties
	}
}