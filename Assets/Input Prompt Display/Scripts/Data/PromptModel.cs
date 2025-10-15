/* Input Prompt Display v.1.0.0
 * --------------------------------------------------------------------------------------------------------------------------------------------------
 * 
 * This file is part of Input Prompt Display, which is released under the Unity Asset Store End User License Agreement.
 * To view a copy of this license, visit https://unity3d.com/legal/as_terms
 * 
 * Input Prompt Display © 2023 by Flight Paper Studio LLC is licensed under CC BY 4.0. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 */

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IPD
{
	/// <summary>
	/// This class stores the data of a single prompt for an input.
	/// </summary>
	[System.Serializable]
	public class PromptModel
	{
		#region Prompt Data

		/// <summary>
		/// The visual style for the prompt.
		/// Can include platform specific styling (e.g. WINDOWS/filled/small).
		/// </summary>
		[Tooltip ( "The visual style for the prompt. Can include platform specific styling (e.g. WINDOWS/filled/small)." )]
		public string Style;

		/// <summary>
		/// The directly loaded sprite for the prompt.
		/// Use this when not using the addressable.
		/// </summary>
		[Tooltip ( "The directly loaded sprite for the prompt. Use this when not using the addressable." )]
		public Sprite Sprite;

		/// <summary>
		/// The indirectly loaded addressable for the prompt.
		/// Use this when not using the directly loaded sprite.
		/// </summary>
		[Tooltip ( "The indirectly loaded addressable for the prompt. Use this when not using the directly loaded sprite." )]
		public AssetReference Addressable;

		private Sprite loadedAddressable;

		#endregion // Prompt Data

		#region Public Functions

		/// <summary>
		/// Gets whether or not this model contains valid data.
		/// </summary>
		/// <returns> Whether or not this model data is valid. </returns>
		public bool IsValid ( )
		{
			return Sprite != null || ( Addressable != null && Addressable.RuntimeKeyIsValid ( ) );
		}

		/// <summary>
		/// Gets the sprite value for this prompt.
		/// </summary>
		/// <returns> The sprite for this prompt. </returns>
		public Sprite LoadSprite ( )
		{
			// Check for valid data
			if ( !IsValid ( ) )
			{
				return null;
			}

			// Check for directly loaded sprite
			if ( Sprite != null )
			{
				return Sprite;
			}

			// Check if the sprite addressable is already loaded
			if ( loadedAddressable != null )
			{
				return loadedAddressable;
			}

			// Return that the sprite has not loaded yet
			return null;
		}

		/// <summary>
		/// Gets the sprite value for this prompt asynchronously.
		/// </summary>
		/// <returns> The sprite for this prompt. </returns>
		public async Task<Sprite> LoadSpriteAsync ( )
		{
			// Check for valid data
			if ( !IsValid ( ) )
			{
				return null;
			}

			// Check for directly loaded sprite
			if ( Sprite != null )
			{
				return Sprite;
			}

			// Check if the sprite addressable is already loaded
			if ( loadedAddressable != null )
			{
				return loadedAddressable;
			}

			// Check for addressable
			AssetReference spriteAddressable = Addressable;
			if ( spriteAddressable != null && spriteAddressable.RuntimeKeyIsValid ( ) )
			{
				// Check if addressable is loaded internally
				if ( spriteAddressable.IsValid ( ) )
				{
					// Wait for the sprite to load
					AsyncOperationHandle<Sprite> handler = spriteAddressable.OperationHandle.Convert<Sprite> ( );
					await handler.Task;

					// Store the loaded sprite
					loadedAddressable = handler.Result;
				}
				else
				{
					// Wait for the sprite to load
					AsyncOperationHandle<Sprite> handler = spriteAddressable.LoadAssetAsync<Sprite> ( );
					await handler.Task;

					// Check for successful load
					if ( handler.Status == AsyncOperationStatus.Succeeded )
					{
						// Store the loaded sprite
						loadedAddressable = handler.Result;
					}
				}
			}

			// Return the loaded sprite
			return loadedAddressable;
		}

		#endregion // Public Functions
	}
}