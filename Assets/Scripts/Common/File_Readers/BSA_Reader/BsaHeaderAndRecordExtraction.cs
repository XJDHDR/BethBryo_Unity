using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BethBryo_for_Unity
{
	internal struct BsaHeader
	{
		internal bool BsaIsCompressed;
		internal uint TotalFolderCount;
		internal uint TotalFileCount;
		internal uint TotalLengthAllFolderNames;
		internal uint TotalLengthAllFileNames;
	}

	internal struct FolderRecords
	{
		internal ulong FolderNameHash;
		internal uint FoldersFileCount;
		internal uint NameAndFileRecordsOffset;
	}

	internal struct FolderNameAndFileRecords
	{
		internal string FolderName;
		internal FileRecords[] FileRecords;
	}

	internal struct FileRecords
	{
		internal ulong FileNameHash;
		internal bool DefaultCompressionInverted;
		internal uint FileSize;
		internal uint FileDataOffset;       // This offset is from the start of the BSA, not from any record.
		internal string FileName;
	}

	internal static class BsaHeaderAndRecordExtraction
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
		internal static void ExtractBsaContents(string PathToBsa, string GameName)
		{
			if (File.Exists(PathToBsa))
			{
				using (FileStream _bsaFileStream = new FileStream(PathToBsa, FileMode.Open, FileAccess.Read))
				{
					// Initialise some variables for the FileStream caching as well as the cache itself
					BytesParams _bytesParams = new BytesParams();
					_bytesParams.FileStream = _bsaFileStream;
					_bytesParams.StreamCache = new byte[4096];
					_bytesParams.CacheCurrentPos = 4096;
					BytesToTypes.RefillBytesArray(ref _bytesParams);
					
					// Read the BSA header and records
					BsaHeader _bsaHeader = new BsaHeader();
					if (_readBsaHeader(GameName, ref _bytesParams, ref _bsaHeader))
					{
						_readBsaFolderRecords(_bsaHeader, ref _bytesParams, out FolderRecords[] _folderRecords);
						_readBsaFolderNamesAndFileRecords(_bsaHeader, _folderRecords, ref _bytesParams, out FolderNameAndFileRecords[] _folderNameAndFileRecords);
					}
				}
			}
		}

		private static bool _readBsaHeader(string _gameName, ref BytesParams _bytesParams, ref BsaHeader _bsaHeader)
		{
			// Read first 4 bytes of file and determine if those bytes are valid (it is "BSA " for every BethBryo BSA).
			char[] _bsaStringArr = new char[4];
			for (ushort _i = 0; _i < 4; _i += 1)
			{
				_bsaStringArr[_i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _));
			}
			string _characterCode = new string(_bsaStringArr);
			if (! Equals(_characterCode, "BSA "))
			{
				Debug.LogErrorFormat("{_openedBsa} does not begin with the correct character code. It should be \"BSA \" but \"{_characterCode}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			// Read next 4 bytes to find BSA's version and determine if it is valid for the current game.
			uint _bsaVersion = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
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
					Debug.LogErrorFormat("{_openedBsa} has a version number of {_bsaVersion}, which does not match that used by {_gameName} BSAs.");
					return false;
			}

			// BitConverter.ToBoolean(Byte[], Int32)
			// Read next 4 bytes to find BSA's Folder Record Offset and then determine if it is equal to 36.
			uint _folderRecordsOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_folderRecordsOffset != 36)
			{
				Debug.LogErrorFormat("{_openedBsa} has an incorrect FolderRecord offset. It should be \"36\" but \"{_folderRecordsOffset}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Archive Flags. Then determine if some flags are configured correctly.
			// The "Compressed" flag (bit 3) is the only one relevant for extracting files.
			// Bits 1, 2 and 7 are only relevant for checking if the BSA is good. Everything else is either unknown or irrelevant for Unity.
			uint _archiveFlags = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if ((_archiveFlags & 1) != 1)	// Is bit 1 set? - Any number ANDed with 1 is equal to 1 if bit 1 set.
			{
				Debug.LogErrorFormat("{_openedBsa} has it's \"Names for Directories\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & 2) != 2)    // Is bit 2 set? - Any number ANDed with 2 is equal to 2 if bit 2 set.
			{
				Debug.LogErrorFormat("{_openedBsa} has it's \"Names for Files\" Archive Flag unset when it should be. This could indicate a corrupted file.");
				return false;
			}
			else if ((_archiveFlags & 128) != 128)    // Is bit 7 set? - Any number ANDed with 128 is equal to 128 if bit 7 set.
			{
				Debug.LogErrorFormat("{_openedBsa} has it's \"Big-Endian\" Archive Flag set when it shouldn't be. This could indicate either a corrupted file or that you are trying to use a console version's BSA, which isn't supported.");
				return false;
			}
			else
			{
				if ((_archiveFlags & 4) == 4)    // Is bit 3 set? - Any number ANDed with 4 is equal to 4 if bit 3 set.
					_bsaHeader.BsaIsCompressed = true;
				else
					_bsaHeader.BsaIsCompressed = false;
			}

			// Read next 4 bytes to find BSA's Total Folder Count and check if it's value is not zero. 
			_bsaHeader.TotalFolderCount = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalFolderCount < 1)
			{
				Debug.LogErrorFormat("{_openedBsa} has it's Total Folders counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			// Read next 4 bytes to find BSA's Total File Count and check if it's value is not zero. 
			_bsaHeader.TotalFileCount = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalFileCount < 1)
			{
				Debug.LogErrorFormat("{_openedBsa} has it's Total Files counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			// Read next 4 bytes to find the total length of all folder names in this BSA and check if it's value is not zero. 
			_bsaHeader.TotalLengthAllFolderNames = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalLengthAllFolderNames < 1)
			{
				Debug.LogErrorFormat("{_openedBsa} has it's Total Folder Name length set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			// Read next 4 bytes to find the total length of all file names in this BSA and check if it's value is not zero. 
			_bsaHeader.TotalLengthAllFileNames = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalLengthAllFileNames < 1)
			{
				Debug.LogErrorFormat("{_openedBsa} has it's Total File Name length set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			uint _bsaContentType = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);

			long _currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
			if (_currentPointerPosition != _folderRecordsOffset)
			{
				Debug.LogErrorFormat("The pointer for reading data from {_openedBsa} currently has it's pointer at byte {_currentPointerPosition} " +
					"after it finished reading the BSA Header. It is supposed to be at byte {_folderRecordsOffset}. This could indicate a corrupted file.");
				return false;
			}

			return true;
		}

		private static bool _readBsaFolderRecords(BsaHeader _bsaHeader, ref BytesParams _bytesParams, out FolderRecords[] _folderRecords)
		{
			_folderRecords = new FolderRecords[_bsaHeader.TotalFolderCount];
			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; _i += 1)
			{
				// Read next 8 bytes to find the folder name hash for this folder. 
				_folderRecords[_i].FolderNameHash = BytesToTypes.BytesToULong64(ref _bytesParams, out _);

				// Read next 4 bytes to find this folder's file count and check if it's value is not zero. 
				_folderRecords[_i].FoldersFileCount = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
				if (_folderRecords[_i].FoldersFileCount < 1)
				{
					Debug.LogErrorFormat("{_openedBsa} has it's Total File Name length set to less than 1. This could indicate a corrupted record.");
					return false;
				}

				// Read next 4 bytes to find the offset to this folder's record for folder name and files details combined with total file name length.
				// Then subtract total file name length because we only need the offset. Then check if the result is not zero.
				_folderRecords[_i].NameAndFileRecordsOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _) - _bsaHeader.TotalLengthAllFileNames;
				if (_folderRecords[_i].NameAndFileRecordsOffset < 1)
				{
					Debug.LogErrorFormat("{_openedBsa} has one of it's Folder Name And Files Record offsets set to less than 1. This could indicate a corrupted record.");
					return false;
				}
			}
			return true;
		}

		private static bool _readBsaFolderNamesAndFileRecords(BsaHeader _bsaHeader, FolderRecords[] _folderRecords, ref BytesParams _bytesParams,
																out FolderNameAndFileRecords[] _folderNameAndFileRecords)
		{
			long _currentPointerPosition = 0;
			uint _countedLengthOfFolderNames = 0;
			_folderNameAndFileRecords = new FolderNameAndFileRecords[_bsaHeader.TotalFolderCount];

			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; _i += 1)
			{
				_currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
				if (_currentPointerPosition != _folderRecords[_i].NameAndFileRecordsOffset)
				{
					Debug.LogErrorFormat("The pointer for reading data from {_openedBsa} currently has it's pointer at byte {_currentPointerPosition} while it was reading the " +
						"Folder Names And File Record #{i+1}. It is supposed to be at byte {_folderRecords[i]._nameAndFileRecordsOffset}. This could indicate a corrupted file.");
					return false;
				}

				// Get the name of the current folder in the folder name and files record, which is stored as a zero-terminated string prefixed with it's length.
				// First byte is the length of the folder name's string. The rest is the string itself.
				byte _folderNameStringLength = BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _);
				_countedLengthOfFolderNames += _folderNameStringLength;
				char[] _bsaStringArr = new char[_folderNameStringLength];
				for (ushort _j = 0; _j < (_folderNameStringLength - 1); _j += 1)
				{
					_bsaStringArr[_i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _));
				}
				_folderNameAndFileRecords[_i].FolderName = new string(_bsaStringArr);
				// Last byte in folder's name must be a zero value
				if (BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _) != 0)
				{
					Debug.LogErrorFormat("{_openedBsa}'s folder name \"{_folderNameAndFileRecords[i]._folderName}\" did not end with a zero value. This could indicate a corrupted file.");
					return false;
				}

				_folderNameAndFileRecords[_i].FileRecords = new FileRecords[_folderRecords[_i].FoldersFileCount];
				for (uint _j = 0; _j < _folderRecords[_i].FoldersFileCount; _j += 1)
				{
					_folderNameAndFileRecords[_i].FileRecords[_j].FileNameHash = BytesToTypes.BytesToULong64(ref _bytesParams, out _);
					uint _tempFileSize = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);

					// Bit 31 in the "FileSize" record is used to indicate that this file's compression status is inverted from the global flag's status. Doesn't indicate file's size.
					if ((_tempFileSize & 1073741824) == 1073741824)   // Is bit 31 set? - Any number ANDed with 1073741824 is equal to 1073741824 if bit 31 set.
					{
						_folderNameAndFileRecords[_i].FileRecords[_j].DefaultCompressionInverted = true;
						_tempFileSize -= 1073741824;
					}
					else
						_folderNameAndFileRecords[_i].FileRecords[_j].DefaultCompressionInverted = false;

					// Bit 32 in the "FileSize" record is used to indicate that this archive record has been checked.
					// Not relevant for Unity but must be zeroed if present because it doesn't indicate file size.
					if ((_tempFileSize & 2147483648) == 2147483648)   // Is bit 32 set? - Any number ANDed with 2147483648 is equal to 2147483648 if bit 32 set.
					{
						_tempFileSize -= 2147483648;
					}

					_folderNameAndFileRecords[_i].FileRecords[_j].FileSize = _tempFileSize;

					// Offset to the file's raw data. This offset is from the start of the BSA, not from any record.
					_folderNameAndFileRecords[_i].FileRecords[_j].FileDataOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
				}
			}

			if (_countedLengthOfFolderNames != _bsaHeader.TotalLengthAllFolderNames)
			{
				Debug.LogErrorFormat("{_openedBsa}'s Total Folder Name Length given in header ({_bsaHeader._totalLengthAllFolderNames}) does not match up with the lengths " +
					"counted for all folder names in the Folder Names And File Records ({_countedLengthOfFolderNames}). This could indicate a corrupted file.");
				return false;
			}

			uint _countedLengthOfFileNames = 0;
			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; _i += 1)
			{
				for (uint _j = 0; _j < _folderRecords[_i].FoldersFileCount; _j += 1)
				{
					// Get the name of the current file in the File Name block, which is stored as a zero-terminated string.
					// Since there is no length indicated, we have to read byte-by-byte until we reach a zero.
					List<char> _bsaStringAList = new List<char>();
					ushort _loopLimiter = 0;
					while (_loopLimiter < 65503)
					{
						_loopLimiter += 1;
						_countedLengthOfFileNames += 1;

						byte _readFilenameByte = BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _);
						if (_readFilenameByte == 0)
						{
							break;
						}
						else
						{
							_bsaStringAList.Add(System.Convert.ToChar(_readFilenameByte));
						}

						if ((_loopLimiter >= 65500) || (_bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos >= _bytesParams.FileStream.Length))
						{
							string _errorStartText;
							if (_loopLimiter >= 65500)
							{
								_errorStartText = "FileReader for {_openedBsa} failed to encounter a zero value after 65000 characters while reading the ";
							}
							else
							{
								_errorStartText = "FileReader for {_openedBsa} failed to encounter a zero value before reaching the end of the file while reading the ";
							}

							string _errorEndText;
							if (_i == 0)
							{
								if (_j == 0)
								{
									_errorEndText = "first filename. This could indicate a corrupted file.";
								}
								else
								{
									_errorEndText = "filename after {_folderNameAndFileRecords[i]._fileRecords[j-1]._fileName}. This could indicate a corrupted file.";
								}
							}
							else
							{
								if (_j == 0)
								{
									_errorEndText = "filename after {_folderNameAndFileRecords[i - 1]._fileRecords[_folderNameAndFileRecords[i - 1]._fileRecords.Length - 1]._fileName}. " +
										"This could indicate a corrupted file.";
								}
								else
								{
									_errorEndText = "filename after {_folderNameAndFileRecords[i]._fileRecords[j-1]._fileName}. This could indicate a corrupted file.";
								}
							}
							Debug.LogErrorFormat(_errorStartText + _errorEndText);
							return false;
						}
					}
					_folderNameAndFileRecords[_i].FileRecords[_j].FileName = string.Join("", _bsaStringAList);
				}
			}

			if (_countedLengthOfFileNames != _bsaHeader.TotalLengthAllFileNames)
			{
				Debug.LogErrorFormat("{_openedBsa}'s Total Folder Name Length given in header ({_bsaHeader._totalLengthAllFolderNames}) does not match up with the lengths " +
					"counted for all folder names in the Folder Names And File Records ({_countedLengthOfFolderNames}). This could indicate a corrupted file.");
				return false;
			}

			_currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
			if (_currentPointerPosition != _folderNameAndFileRecords[0].FileRecords[0].FileDataOffset)
			{
				Debug.LogErrorFormat("The pointer for reading data from {_openedBsa} currently has it's pointer at byte {_currentPointerPosition} after it finished reading the " +
					"File Name Block. It is supposed to be at byte {_folderNameAndFileRecords[0]._fileRecords[0]._fileDataOffset}. This could indicate a corrupted file.");
				return false;
			}

			return true;
		}
	}
}
