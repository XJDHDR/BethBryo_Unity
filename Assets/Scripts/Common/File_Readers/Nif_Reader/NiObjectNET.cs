// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;

namespace BethBryo_for_Unity_Common
{
	internal class NiObjectNET
	{
		internal static bool ReadNiObjectNET(byte[] NifData, NifHeaderData NifHeaderData, uint BlockNumber, ref int CurArrayPos)
		{
			// The first thing that might be contained in a NiObjectNET block is a BSLightingShaderType value. 
			// This is only the case if the parent block is a BSLightingShaderProperty, Nif version is 20.2.0.7 and the BS Version is between 83 and 130.
			if (NifHeaderData.BlockTypesListCRC32Hashes[NifHeaderData.BlockTypesToObjectIndex[BlockNumber]] == 0xc689f98c)  // BSLightingShaderProperty
			{
				if (NifHeaderData.NifVersionCombined == 0x14020007)     // If equal to 20.2.0.7
				{
					if ((NifHeaderData.BSVersion >= 83) && (NifHeaderData.BSVersion <= 130))
					{
						BSLightingShaderType _bSLightingShaderType = (BSLightingShaderType)BitConverter.ToUInt32(NifData, CurArrayPos);
						CurArrayPos += 4;
					}
				}
			}

			// Next, we have the name of the block being read. This will be either a string if 20.0.0.5 or less, or an index in the string array if 20.1.0.3 or greater.
			if (NifHeaderData.NifVersionCombined <= 0x14000005)			// If less than or equal to 20.0.0.5
			{
				FileNifCommonMethods.ReadSizedString(NifData, ref CurArrayPos, out string _blockName);
			}
			else if (NifHeaderData.NifVersionCombined >= 0x14010003)    // If greater than or equal to 20.1.0.3
			{
				uint _stringLength = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
			}

			// Next, there is a block of Legacy Extra Data if the version is up to 2.3.0.0
			if (NifHeaderData.NifVersionCombined <= 0x02030000)         // If less than or equal to 2.3.0.0
			{
				// First is a boolean that indicates if there is any other extra data present.
				// For these Nif versions, it is a 32-bit number that is equal to either 0 or 1.
				uint _isThereExtraDataPresent = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;

				if (_isThereExtraDataPresent == 1)
				{
					FileNifCommonMethods.ReadSizedString(NifData, ref CurArrayPos, out string _extraPropName);

				}
			}


			return true;
		}
	}
}
