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
	/// This scriptable object stores the data for mapping the input prompts of a control scheme.
	/// </summary>
	[CreateAssetMenu ( fileName = "Control Scheme Mapping", menuName = "Scriptable Objects/Input Prompt/Control Scheme Mapping" )]
	public class ControlSchemeMappingScriptableObject : ScriptableObject
	{
		#region Mapping Data

		[Tooltip ( "The name of the control scheme." )]
		[SerializeField]
		private string scheme;

		[Tooltip ( "The data for the input prompts of the control scheme." )]
		[NonReorderable] // HACK: Ensure proper rendering in editor
		[SerializeField]
		private InputModel [ ] inputPrompts;

		#endregion // Mapping Data

		#region Public Properties

		/// <summary>
		/// The name of the control scheme.
		/// </summary>
		public string Scheme
		{
			get
			{
				return scheme;
			}
		}

		/// <summary>
		/// The data for the input prompts of the control scheme.
		/// </summary>
		public InputModel [ ] InputPrompts
		{
			get
			{
				return inputPrompts;
			}
		}

		#endregion // Public Properties
	}
}