//using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace BethBryo_for_Unity
{
	internal struct FolderRecords
	{
		internal ulong _folderNameHash;
		internal uint _foldersFileCount;
		internal uint _nameAndFileRecordsOffset;
	}

	internal struct FolderNameAndFileRecords
	{
		internal string _folderName;
		internal FileRecords[] _fileRecords;
	}

	internal struct FileRecords
	{
		internal ulong _fileNameHash;
		internal uint _fileSize;
		internal bool _defaultCompressionInverted;
		internal uint _fileDataOffset;       // This offset is from the start of the BSA, not from any record.
	}

	public static class BsaExtraction
	{
		/// <summary>
		/// Used to initialise the FileStream cache and re-fill it periodically.
		/// You should call this function once to initialise the class before calling any of the other functions for the first time.
		/// Initialisation is done by calling this function like so: RefillBytesArray({FileStream}, ref {byte[]}, ref {int} = 4096)
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <returns>Returns a signed integer with the number of bytes that were read from the FileStream.</returns>
		public static void ExtractBsaContents(string _pathToBsa, string _gameName)
		{
			if (File.Exists(_pathToBsa))
			{
				using (FileStream _bsaFileStream = new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read))
				{
					// Initialise some variables for the FileStream caching as well as the cache itself
					int _byteArrayCacheCurrentPos = 4096;
					byte[] _bsaFileStreamCache = new byte[4096];
					BytesToTypes.RefillBytesArray(_bsaFileStream, ref _bsaFileStreamCache, ref _byteArrayCacheCurrentPos);

					// Read the BSA header and records
					if (_readBsaHeader(_bsaFileStream, _gameName, ref _bsaFileStreamCache, ref _byteArrayCacheCurrentPos, out uint _totalFolderCount, out bool _bsaIsCompressed))
					{
						_readBsaFolderRecords(_bsaFileStream, _totalFolderCount, ref _bsaFileStreamCache, ref _byteArrayCacheCurrentPos, out FolderRecords[] _folderRecords);
						_readBsaFolderNamesAndFileRecords(_bsaFileStream, _totalFolderCount, _folderRecords, ref _bsaFileStreamCache, 
															ref _byteArrayCacheCurrentPos, out FolderNameAndFileRecords[] _folderNameAndFileRecords);
					}
				}
			}
		}

		internal static bool _readBsaHeader(FileStream _bsaFileStream, string _gameName, ref byte[] _bsaStreamCache, ref int _cacheCurrentPos, out uint _totalFolderCount, out bool _bsaIsCompressed)
		{
			// Set some initial default values for the output variables
			_totalFolderCount = 0;
			_bsaIsCompressed = false;

			// Read first 4 bytes of file and determine if those bytes are valid (it is "BSA " for every BethBryo BSA).
			char[] _bsaStringArr = new char[4];
			for (byte i = 0; i < 4; i += 1)
			{
				_bsaStringArr[i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _));
			}
			string _characterCode = new string(_bsaStringArr);
			if (! Equals(_characterCode, "BSA "))
			{
				Debug.LogErrorFormat("BSA {_openedBsa} does not begin with the correct character code. It should be \"BSA \" but \"{_characterCode}\" was read instead. This could indicate a corrupted file.");
				_totalFolderCount = 0;
				return false;
			}

			// Read next 4 bytes to find BSA's version and determine if it is valid for the current game.
			uint _bsaVersion = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			switch (_bsaVersion)
			{
				case 103 when _gameName == "Oblivion":
				//case ?103? when _gameName == "Fallout3":
				//case ?103? when _gameName == "FalloutNV":
				//case 104 when _gameName == "Skyrim":
				//case ?105? when _gameName == "Fallout4":
				//case 105 when _gameName == "SkyrimSE":
					// BSA is valid so continue executing this method.
					break;

				default:
					Debug.LogErrorFormat("BSA {_openedBsa} has a version number of {_bsaVersion}, which does not match that used by {_gameName} BSAs.");
					return false;
			}

			// BitConverter.ToBoolean(Byte[], Int32)
			// Read next 4 bytes to find BSA's Folder Record Offset and then determine if it is equal to 36.
			uint _folderRecordsOffset = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			if (_folderRecordsOffset != 36)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has an incorrect FolderRecord offset. It should be \"36\" but \"{_folderRecordsOffset}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Archive Flags. Then determine if some flags are configured correctly and extract value of "Compressed" flag if so.
			uint _archiveFlags = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			if ((_archiveFlags & (1 << 1)) == 0)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Names for Directories\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & (1 << 2)) == 0)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Names for Files\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & (1 << 7)) != 0)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Big-Endian\" Archive Flag set when it shouldn't be. This could indicate either a corrupted file or that you are trying to use a BSA extracted from a console, which isn't supported.");
				return false;
			}
			else
			{
				if ((_archiveFlags & (1 << 3)) == 0)
					_bsaIsCompressed = false;
				else
					_bsaIsCompressed = true;
			}

			// Read next 4 bytes to find BSA's Total Folder Count and check if it's value is valid. 
			_totalFolderCount = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			if (_totalFolderCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Folders counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Total File Count and check if it's value is valid. 
			uint _totalFileCount = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			if (_totalFileCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Files counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			uint _totalLengthAllFolderNames = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			uint _totalLengthAllFileNames = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			uint _bsaContentType = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			return true;
		}

		internal static void _readBsaFolderRecords(FileStream _bsaFileStream, uint _totalFolderCount, ref byte[] _bsaStreamCache, ref int _cacheCurrentPos, out FolderRecords[] _folderRecords)
		{
			_folderRecords = new FolderRecords[_totalFolderCount];
			for (uint i = 0; i < _totalFolderCount; i += 1)
			{
				_folderRecords[i]._folderNameHash = BytesToTypes.BytesToULong64(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
				_folderRecords[i]._foldersFileCount = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
				_folderRecords[i]._nameAndFileRecordsOffset = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
			}
		}

		internal static bool _readBsaFolderNamesAndFileRecords(FileStream _bsaFileStream, uint _totalFolderCount, FolderRecords[] _folderRecords,
																ref byte[] _bsaStreamCache, ref int _cacheCurrentPos, out FolderNameAndFileRecords[] _folderNameAndFileRecords)
		{
			_folderNameAndFileRecords = new FolderNameAndFileRecords[_totalFolderCount];

			for (uint i = 0; i < _totalFolderCount; i += 1)
			{
				// Get the name of the current folder in the folder name and files record.
				byte _folderNameStringLength = BytesToTypes.BytesToSingleByte8(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);  // First byte is the length of the folder name's string.
				char[] _bsaStringArr = new char[_folderNameStringLength];
				for (byte j = 0; j < (_folderNameStringLength - 1); j += 1)
				{
					_bsaStringArr[i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _));
				}
				_folderNameAndFileRecords[i]._folderName = new string(_bsaStringArr);
				if (BytesToTypes.BytesToSingleByte8(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _) != 0)     // Last byte in folder's name must be a zero value
				{
					Debug.LogErrorFormat("A BSA's folder name \"{_folderNameAndFileRecords[i]._folderName}\" did not end with a zero value. This could indicate a corrupted file.");
					return false;
				}

				_folderNameAndFileRecords[i]._fileRecords = new FileRecords[_folderRecords[i]._foldersFileCount];
				for (uint j = 0; j < _folderRecords[i]._foldersFileCount; j += 1)
				{
					_folderNameAndFileRecords[i]._fileRecords[j]._fileNameHash = BytesToTypes.BytesToULong64(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
					_folderNameAndFileRecords[i]._fileRecords[j]._fileSize = BytesToTypes.BytesToUInt32(_bsaFileStream, ref _bsaStreamCache, ref _cacheCurrentPos, out _);
					_folderNameAndFileRecords[i]._fileRecords[j]._defaultCompressionInverted = false;
					_folderNameAndFileRecords[i]._fileRecords[j]._fileDataOffset = 0;

				}

			}
			return true;
		}

		internal static void DecompressZlibData()
		{
	//		using (FileStream originalFileStream = fileToDecompress.OpenRead())	// Might not need this
			{
	//			string currentFileName = fileToDecompress.FullName;
	//			string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

	//			using (FileStream decompressedFileStream = File.Create(newFileName))
				{
	//				using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
					{
	//					decompressionStream.CopyTo(decompressedFileStream);
	//					Debug.Log("Decompressed: {0}", fileToDecompress.Name);
					}
				}
			}
		}
	}
}
