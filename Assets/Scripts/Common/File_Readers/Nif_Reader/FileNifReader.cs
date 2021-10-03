// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// This is the base class that is used to read all data from an Nif file.
	/// </summary>
	public static class FileNifReader
	{
		/// <summary>
		/// Used to read the binary data in an Nif file and translate it into a form that is easier to read.
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="NifLocation">Full path indicating the location of the Nif file.</param>
		/// <returns>True if the method successfully extracted usable data from the Nif binary data. False otherwise.</returns>
		public static bool ReadAllNifObjects(byte[] NifData, string NifLocation)
		{
			// Read the Nif file's header.
			int _curArrayPos = 0;
			if (NifHeader.GetNifHeaderAndNodes(NifData, NifLocation, ref _curArrayPos, out FileNifStructs.NifHeaderData _nifHeaderData) == false)
				return false;

			// Next, read each block present in the file. 
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
							if (NiNode.ReadNiNode(NifData, NifLocation, _nifHeaderData, _i, ref _curArrayPos) == false)
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
