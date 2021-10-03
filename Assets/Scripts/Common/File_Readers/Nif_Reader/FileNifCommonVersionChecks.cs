// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Used to hold a number of common version checks used by various types of data in the file.
	/// </summary>
	internal static class FileNifCommonVersionChecks
	{
		/// <summary>
		/// Checks if the Nif BS version is one associated with a non-Bethesda game.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiStream(uint? BsVersion)
		{
			if ((BsVersion == null) || (BsVersion == 0))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with a Bethesda game.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsStream(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion > 0))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is less than or equal to 16.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLte16(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion <= 16))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with games earlier than Fallout 3.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLtFO3(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion < 34))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 3 or earlier.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLteFO3(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion <= 34))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with games earlier than Skyrim: Special Edition.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLtSSE(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion < 100))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with games earlier than Fallout 4.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLtFO4(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion < 130))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 4 or earlier.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsNiBsLteFO4(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion <= 130))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with games after Fallout 3.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsGtFO3(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion > 34))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 3 or later.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsGteFO3(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion >= 34))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Skyrim (original version) or later.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsGteSky(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion >= 83))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Skyrim: Special Edition or later.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsGteSSE(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion >= 100))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Skyrim: Special Edition.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsSSE(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion == 100))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 4.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsFO4(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion == 130))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 4 or later.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsGteFO4(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion >= 130))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif BS version is one associated with Fallout 76.
		/// </summary>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBsFO76(uint? BsVersion)
		{
			if ((BsVersion != null) && (BsVersion == 155))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif Version is equal to 20.2.0.7 and if the BS Version is greater than 0.
		/// </summary>
		/// <param name="NifVersion">The Combined Nif Version read from the Nif file's header.</param>
		/// <param name="BsVersion">The BS Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsBs202(uint NifVersion, uint? BsVersion)
		{
			if ((NifVersion == 0x14020007) && (BsVersion != null) && (BsVersion > 0))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if the Nif user version is one associated with Divinity 2.
		/// </summary>
		/// <param name="UserVersion">The User Version read from the Nif file's header.</param>
		/// <returns>True if the Nif is associated. False otherwise.</returns>
		internal static bool IsDivinity2(uint? UserVersion)
		{
			if ((UserVersion != null) && ((UserVersion == 0x20000) || (UserVersion == 0x30000)))
				return true;
			else
				return false;
		}
	}
}
