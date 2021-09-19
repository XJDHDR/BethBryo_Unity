// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	internal struct NifHeaderData
	{
		internal uint NifVersionCombined;
		internal uint? UserVersion;
		internal uint? NumberOfBlocks;
		internal uint? BSVersion;
		internal ushort? BlockTypesQuantity;
		internal string[] BlockTypesList;
		internal uint[] BlockTypesListCRC32Hashes;
		internal uint[] BlockTypesHashes;
		internal ushort[] BlockTypesToObjectIndex;
		internal uint[] BlockSizes;
		internal uint? StringsQuantity;
		internal uint? StringsMaxLength;
		internal string[] StringsList;
		internal uint QtyGroups;
		internal uint[] Groups;
	}
}
