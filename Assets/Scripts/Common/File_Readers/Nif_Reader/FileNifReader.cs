// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	internal static class FileNifReader
	{
		/// <summary>
		/// Used to initialise the FileStream cache and re-fill it periodically.
		/// You should call this function once to initialise the class before calling any of the other functions for the first time.
		/// Initialisation is done by calling this function like so: RefillBytesArray({FileStream}, ref {byte[]}, ref {int} = 4096)
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="NifLocation">Full path indicating the location of the Nif file.</param>
		/// <returns>True if the method successfully extracted usable data from the Nif binary data. False otherwise.</returns>
		public static bool ReadAllNifObjects(byte[] NifData, string NifLocation)
		{
			int _curArrayPos = 0;
			if (NifHeader.GetNifHeaderAndNodes(NifData, NifLocation, ref _curArrayPos, out FileNifStructs.NifHeaderData _nifHeaderData) == false)
				return false;

			if (_nifHeaderData.NumberOfBlocks != null)
			{
				uint _i = 0;
				while (_i < _nifHeaderData.NumberOfBlocks)
				{
					// Do a comparison on the CRC32 hash of the current block's type.
					uint _blockTypeForCurrentBlock = _nifHeaderData.BlockTypesListCRC32Hashes[_nifHeaderData.BlockTypesToObjectIndex[_i]];
					switch (_blockTypeForCurrentBlock)
					{
						case 0x14bb47e4:	// NiNode
							if (NiNode.ReadNiNode(NifData, _nifHeaderData, _i, ref _curArrayPos) == false)
								return false;
							break;





						case 0xc689f98c:	// BSLightingShaderProperty
							break;

						default:
							break;
					}

					++_i;
				}
			}
			else
			{

			}

			return true;
		}
	}
}
