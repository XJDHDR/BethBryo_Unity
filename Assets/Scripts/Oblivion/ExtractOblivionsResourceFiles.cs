// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using BethBryo_for_Unity_Common;
using UnityEngine;

namespace BethBryo_for_Unity_Oblivion
{
	public class ExtractOblivionsResourceFiles : MonoBehaviour
	{
		public void ExtractOblivionTextureBsa()
		{
			string _pathToDataFiles = "/";

			//bool _noErrorOccurred = BsaHeaderAndRecordExtraction.ExtractBsaContents(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", BSAContentType.Textures, SupportedGames.Oblivion,
			//	out BsaHeader _bsaHeader, out FolderRecords[] _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords);

			Debug.Log ("Finished extracting BSA header");

			if (BsaHeaderAndRecordExtraction.ExtractBsaContents(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", BSAContentType.Textures, SupportedGames.Oblivion,
				out BsaHeader _bsaHeader, out FolderRecords[] _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords))
			{
				int _currentFile = 0;
				string _modFilesPath = Application.streamingAssetsPath + "/Mod Data/Oblivion/";
				BsaFileExtractParams[] _bsaFileExtractParams = new BsaFileExtractParams[_bsaHeader.TotalFileCount];
				for (int _i = 0; _i < _bsaHeader.TotalFolderCount; ++_i)
				{
					for (int _j = 0; _j < _folderNameAndFileRecords[_i].FileRecords.Length; ++_j)
					{
						_bsaFileExtractParams[_currentFile].ExtractToFile = true;
						_bsaFileExtractParams[_currentFile].FileDataOffset = _folderNameAndFileRecords[_i].FileRecords[_j].FileDataOffset;

						if ((_bsaHeader.BsaIsCompressed == true && _folderNameAndFileRecords[_i].FileRecords[_j].DefaultCompressionInverted == false) ||
							(_bsaHeader.BsaIsCompressed == false && _folderNameAndFileRecords[_i].FileRecords[_j].DefaultCompressionInverted == true))
						{
							_bsaFileExtractParams[_currentFile].IsDataCompressed = true;
						}
						else
						{
							_bsaFileExtractParams[_currentFile].IsDataCompressed = false;
						}

						_bsaFileExtractParams[_currentFile].FileDataSize = _folderNameAndFileRecords[_i].FileRecords[_j].FileSize;
						_bsaFileExtractParams[_currentFile].FileFullPath = _modFilesPath + _folderNameAndFileRecords[_i].FolderName + _folderNameAndFileRecords[_i].FileRecords[_j].FileName;

						++_currentFile;
					}
				}

				BsaFileDataExtraction.ExtractBsaFileDataToFiles(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", _bsaFileExtractParams, out bool _errorsOccurred);
			}
			else
			{

			}
		}
	}
}
