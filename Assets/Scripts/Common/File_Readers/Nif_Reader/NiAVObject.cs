// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	internal class NiAVObject
	{
		internal static bool ReadNiAVObject(byte[] NifData, NifHeaderData NifHeaderData, uint BlockNumber, ref int CurArrayPos)
		{
			// An NiAVObject's data is preceded by NiObjectNET data
			if (NiObjectNET.ReadNiObjectNET(NifData, NifHeaderData, BlockNumber, ref CurArrayPos) == false)
				return false;



			return true;
		}
	}
}
