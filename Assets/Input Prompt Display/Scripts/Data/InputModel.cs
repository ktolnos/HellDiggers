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
	/// This class stores the data of the prompts for a single input of a device.
	/// </summary>
	[System.Serializable]
	public class InputModel
	{
		#region Input Data

		/// <summary>
		/// The control path of the input on the device.
		/// </summary>
		[Tooltip ( "The control path of the input on the device." )]
		public string Path;

		/// <summary>
		/// The prompts for the input.
		/// </summary>
		[Tooltip ( "The prompts for the input." )]
		[NonReorderable] // HACK: This allows the list to render properly in the inspector
		public PromptModel [ ] Prompts;

		#endregion // Input Data
	}
}