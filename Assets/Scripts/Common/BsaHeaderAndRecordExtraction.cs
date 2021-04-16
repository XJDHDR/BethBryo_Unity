using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace BethBryo_for_Unity
{
	internal struct ComMethParams
	{
		internal FileStream _bsaFileStream;
		internal byte[] _streamCache;
		internal int _cacheCurrentPos;
	}

	internal struct BsaHeader
	{
		internal bool _bsaIsCompressed;
		internal uint _totalFolderCount;
	}

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
		internal bool _defaultCompressionInverted;
		internal uint _fileSize;
		internal uint _fileDataOffset;       // This offset is from the start of the BSA, not from any record.
	}

	public static class BsaHeaderAndRecordExtraction
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
				ComMethParams _comMethParams = new ComMethParams();
				using (_comMethParams._bsaFileStream = new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read))
				{
					// Initialise some variables for the FileStream caching as well as the cache itself
					_comMethParams._streamCache = new byte[4096];
					_comMethParams._cacheCurrentPos = 4096;
					BytesToTypes.RefillBytesArray(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos);

					// Read the BSA header and records
					BsaHeader _bsaHeader = new BsaHeader();
					if (_readBsaHeader(_gameName, ref _comMethParams, ref _bsaHeader))
					{
						_readBsaFolderRecords(_bsaHeader, ref _comMethParams, out FolderRecords[] _folderRecords);
						_readBsaFolderNamesAndFileRecords(_bsaHeader, _folderRecords, ref _comMethParams, out FolderNameAndFileRecords[] _folderNameAndFileRecords);
					}
				}
			}
		}

		internal static bool _readBsaHeader(string _gameName, ref ComMethParams _comMethParams, ref BsaHeader _bsaHeader)
		{
			// Read first 4 bytes of file and determine if those bytes are valid (it is "BSA " for every BethBryo BSA).
			char[] _bsaStringArr = new char[4];
			for (byte i = 0; i < 4; i += 1)
			{
				_bsaStringArr[i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _));
			}
			string _characterCode = new string(_bsaStringArr);
			if (! Equals(_characterCode, "BSA "))
			{
				Debug.LogErrorFormat("BSA {_openedBsa} does not begin with the correct character code. It should be \"BSA \" but \"{_characterCode}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			// Read next 4 bytes to find BSA's version and determine if it is valid for the current game.
			uint _bsaVersion = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
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
			uint _folderRecordsOffset = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			if (_folderRecordsOffset != 36)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has an incorrect FolderRecord offset. It should be \"36\" but \"{_folderRecordsOffset}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Archive Flags. Then determine if some flags are configured correctly.
			// The "Compressed" flag (bit 3) is the only one relevant for extracting files.
			// Bits 1, 2 and 7 are only relevant for checking if the BSA is good. Everything else is either unknown or irrelevant for Unity.
			uint _archiveFlags = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			if ((_archiveFlags & 1) != 1)	// Is bit 1 set? - Any number ANDed with 1 is equal to 1 if bit 1 set.
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Names for Directories\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & 2) != 2)    // Is bit 2 set? - Any number ANDed with 2 is equal to 2 if bit 2 set.
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Names for Files\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & 128) != 128)    // Is bit 7 set? - Any number ANDed with 128 is equal to 128 if bit 7 set.
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's \"Big-Endian\" Archive Flag set when it shouldn't be. This could indicate either a corrupted file or that you are trying to use a console version's BSA, which isn't supported.");
				return false;
			}
			else
			{
				if ((_archiveFlags & 4) == 4)    // Is bit 3 set? - Any number ANDed with 4 is equal to 4 if bit 3 set.
					_bsaHeader._bsaIsCompressed = true;
				else
					_bsaHeader._bsaIsCompressed = false;
			}

			// Read next 4 bytes to find BSA's Total Folder Count and check if it's value is valid. 
			_bsaHeader._totalFolderCount = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			if (_bsaHeader._totalFolderCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Folders counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Total File Count and check if it's value is valid. 
			uint _totalFileCount = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			if (_totalFileCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Files counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			uint _totalLengthAllFolderNames = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			uint _totalLengthAllFileNames = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			uint _bsaContentType = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			return true;
		}

		internal static void _readBsaFolderRecords(BsaHeader _bsaHeader, ref ComMethParams _comMethParams, out FolderRecords[] _folderRecords)
		{
			_folderRecords = new FolderRecords[_bsaHeader._totalFolderCount];
			for (uint i = 0; i < _bsaHeader._totalFolderCount; i += 1)
			{
				_folderRecords[i]._folderNameHash = BytesToTypes.BytesToULong64(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
				_folderRecords[i]._foldersFileCount = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
				_folderRecords[i]._nameAndFileRecordsOffset = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
			}
		}

		internal static bool _readBsaFolderNamesAndFileRecords(BsaHeader _bsaHeader, FolderRecords[] _folderRecords, ref ComMethParams _comMethParams,
																out FolderNameAndFileRecords[] _folderNameAndFileRecords)
		{
			_folderNameAndFileRecords = new FolderNameAndFileRecords[_bsaHeader._totalFolderCount];

			for (uint i = 0; i < _bsaHeader._totalFolderCount; i += 1)
			{
				// Get the name of the current folder in the folder name and files record.
				// First byte is the length of the folder name's string. The rest is the string itself.
				byte _folderNameStringLength = BytesToTypes.BytesToSingleByte8(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
				char[] _bsaStringArr = new char[_folderNameStringLength];
				for (byte j = 0; j < (_folderNameStringLength - 1); j += 1)
				{
					_bsaStringArr[i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _));
				}
				_folderNameAndFileRecords[i]._folderName = new string(_bsaStringArr);
				// Last byte in folder's name must be a zero value
				if (BytesToTypes.BytesToSingleByte8(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _) != 0)
				{
					Debug.LogErrorFormat("A BSA's folder name \"{_folderNameAndFileRecords[i]._folderName}\" did not end with a zero value. This could indicate a corrupted file.");
					return false;
				}

				_folderNameAndFileRecords[i]._fileRecords = new FileRecords[_folderRecords[i]._foldersFileCount];
				for (uint j = 0; j < _folderRecords[i]._foldersFileCount; j += 1)
				{
					_folderNameAndFileRecords[i]._fileRecords[j]._fileNameHash = BytesToTypes.BytesToULong64(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
					uint _tempFileSize = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);

					// Bit 31 in the "FileSize" record is used to indicate that this file's compression status is inverted from the global flag's status. Doesn't indicate file's size.
					if ((_tempFileSize & 1073741824) == 1073741824)   // Is bit 31 set? - Any number ANDed with 1073741824 is equal to 1073741824 if bit 31 set.
					{
						_folderNameAndFileRecords[i]._fileRecords[j]._defaultCompressionInverted = true;
						_tempFileSize -= 1073741824;
					}
					else
						_folderNameAndFileRecords[i]._fileRecords[j]._defaultCompressionInverted = false;

					// Bit 32 in the "FileSize" record is used to indicate that this archive record has been checked.
					// Not relevant for Unity but must be zeroed if present because it doesn't indicate file size.
					if ((_tempFileSize & 2147483648) == 2147483648)   // Is bit 32 set? - Any number ANDed with 2147483648 is equal to 2147483648 if bit 32 set.
					{
						_tempFileSize -= 2147483648;
					}

					_folderNameAndFileRecords[i]._fileRecords[j]._fileSize = _tempFileSize;

					// Offset to the file's raw data. This offset is from the start of the BSA, not from any record.
					_folderNameAndFileRecords[i]._fileRecords[j]._fileDataOffset = BytesToTypes.BytesToUInt32(_comMethParams._bsaFileStream, ref _comMethParams._streamCache, ref _comMethParams._cacheCurrentPos, out _);
				}
			}
			return true;
		}

		internal static void _readBsaFileNameRecords()
		{

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
