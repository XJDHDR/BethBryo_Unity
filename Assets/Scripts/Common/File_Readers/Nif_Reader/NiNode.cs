// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Provides methods for reading NiNode data from an Nif file.
	/// </summary>
	internal static class NiNode
	{
		/// <summary>
		/// Used to read the binary data that makes up the header of an Nif file.
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="NifHeaderData">Struct holding all of the relevant header data that was read from the header.</param>
		/// <param name="BlockNumber">Indicates that this is the Nth block of data in the Nif file.</param>
		/// <param name="CurArrayPos">The location of the array reading pointer.</param>
		/// <returns>True if the method successfully extracted the NiNode data. False otherwise.</returns>
		internal static bool ReadNiNode(byte[] NifData, string NifLocation, FileNifStructs.NifHeaderData NifHeaderData, uint BlockNumber, ref int CurArrayPos)
		{
			// An NiNode's data is preceded by NiAVObject data
			if (NiAVObject.ReadNiAVObject(NifData, NifLocation, NifHeaderData, BlockNumber, ref CurArrayPos) == false)
				return false;


			return true;
		}
	}
}
