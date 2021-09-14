// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using BethBryo_for_Unity_Common;
using System.Threading;
using UnityEngine;

namespace BethBryo_for_Unity_Oblivion
{
	public class ExtractOblivionsResourceFiles : MonoBehaviour
	{
		private bool _threadRunning = false;
		private Thread _thread;

		public void Update()
		{
			if (_threadRunning == true)
			{
				if (_thread.IsAlive == false)
				{
					_threadRunning = false;
				}
			}
		}

		public void ExtractOblivionTextureBsa()
		{
			if (_threadRunning == false)
			{
				_threadRunning = true;
				_thread = new Thread(_extractOblivionTextureBsaInThread);
				_thread.Start();
				Debug.Log("Created extraction thread.");
			}
		}

		private void _extractOblivionTextureBsaInThread()
		{
			string _pathToDataFiles = "D:/Games/Bethesda/Oblivion/Mod Organiser 2/mods/Original Base Oblivion Data Files/";

			//bool _noErrorOccurred = BsaHeaderAndRecordExtraction.ExtractBsaContents(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", BSAContentType.Textures, SupportedGames.Oblivion,
			//	out BsaHeader _bsaHeader, out FolderRecords[] _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords);

			Debug.Log("Finished extracting BSA header");

			string[] _bsasExtractList = new string[2] { "Oblivion - Textures - Compressed.bsa", "Oblivion - Meshes.bsa" };
			for (byte _h = 0; _h < _bsasExtractList.Length; ++_h)
			{
				BSAContentType _bsaContentType = BSAContentType.Nothing;
				if (_h == 0)
				{
					_bsaContentType = BSAContentType.Textures;
				}
				else if (_h == 1)
				{
					_bsaContentType = (uint)BSAContentType.Meshes + BSAContentType.Trees;
				}

				if (BsaHeaderAndRecordExtraction.ExtractBsaContents(_pathToDataFiles + _bsasExtractList[_h], _bsaContentType, SupportedGames.Oblivion,
					out BsaHeader _bsaHeader, out FolderRecords[] _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords))
				{
					int _currentFile = 0;
					//string _modFilesPath = Application.streamingAssetsPath + "/Mod Data/Oblivion/";
					string _modFilesPath = "F:/Repos/BethBryo_for_Unity/zzTempFileExtraction/";
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
							_bsaFileExtractParams[_currentFile].FileDirectoryPath = _modFilesPath + _folderNameAndFileRecords[_i].FolderName;
							_bsaFileExtractParams[_currentFile].FileNameAndExtension = _folderNameAndFileRecords[_i].FileRecords[_j].FileName;

							++_currentFile;
						}
					}

					BsaFileDataExtraction.ExtractBsaFileDataToFiles(_pathToDataFiles + _bsasExtractList[_h], _bsaFileExtractParams, out bool _errorsOccurred);
				}
				else
				{

				}
			}
		}
	}
}
