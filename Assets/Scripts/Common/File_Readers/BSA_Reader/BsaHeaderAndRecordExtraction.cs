// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BethBryo_for_Unity_Common
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
		internal uint FileDataOffset;
		internal string FileName;
	}

	internal enum BSAContentType : uint
	{
		Nothing		= 0b_0000_0000_0000,  // 0
		Meshes		= 0b_0000_0000_0001,  // 1
		Textures	= 0b_0000_0000_0010,  // 2
		Menus		= 0b_0000_0000_0100,  // 4
		Sounds		= 0b_0000_0000_1000,  // 8
		Voices		= 0b_0000_0001_0000,  // 16
		Shaders		= 0b_0000_0010_0000,  // 32
		Trees		= 0b_0000_0100_0000,  // 64
		Fonts		= 0b_0000_1000_0000,  // 128
		Misc		= 0b_0001_0000_0000,  // 256
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
		internal static void ExtractBsaContents(string PathToBsa, BSAContentType BSAType, SupportedGames CurrentGame)
		{
			if (File.Exists(PathToBsa))
			{
				using (FileStream _bsaFileStream = new FileStream(PathToBsa, FileMode.Open, FileAccess.Read))
				{
					// Initialise some variables for the FileStream caching as well as the cache itself
					BytesParams _bytesParams = new BytesParams
					{
						FileStream = _bsaFileStream,
						StreamCache = new byte[4096],
						CacheCurrentPos = 4096
					};
					BytesToTypes.RefillBytesArray(ref _bytesParams);
					
					// Read the BSA header and records
					BsaHeader _bsaHeader = new BsaHeader();
					if (_readBsaHeader(CurrentGame, BSAType, ref _bytesParams, ref _bsaHeader))
					{
						_readBsaFolderRecords(_bsaHeader, ref _bytesParams, out FolderRecords[] _folderRecords);
						_readBsaFolderNamesAndFileRecords(_bsaHeader, _folderRecords, ref _bytesParams, out FolderNameAndFileRecords[] _folderNameAndFileRecords);
					}
				}
			}
		}

		private static bool _readBsaHeader(SupportedGames _currentGame, BSAContentType _bSAType, ref BytesParams _bytesParams, ref BsaHeader _bsaHeader)
		{
			// Read first 4 bytes of file and determine if those bytes are valid (it is "BSA " for every BethBryo BSA).
			char[] _bsaStringArr = new char[4];
			for (ushort _i = 0; _i < 4; ++_i)
			{
				_bsaStringArr[_i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _));
			}
			string _characterCode = new string(_bsaStringArr);
			if (! Equals(_characterCode, "BSA "))
			{
				Debug.LogErrorFormat("{1} does not begin with the correct character code. It should be \"BSA \" but \"{2}\" was read instead. " +
					"This could indicate a corrupted file.", _bytesParams.FileStream.Name, _characterCode);
				return false;
			}

			// Read next 4 bytes to find BSA's version and determine if it is valid for the current game.
			uint _bsaVersion = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			switch (_bsaVersion)
			{
				case 103 when _currentGame == SupportedGames.Oblivion:
				case 103 when _currentGame == SupportedGames.Fallout3:
				case 103 when _currentGame == SupportedGames.FalloutNV:
				case 104 when _currentGame == SupportedGames.Skyrim:
				case 105 when _currentGame == SupportedGames.SkyrimSE:
				case 105 when _currentGame == SupportedGames.Fallout4:
					// BSA is valid so continue executing this method.
					break;

				default:
					Debug.LogErrorFormat("{1} has a version number of {2}, which does not match that used by {3} BSAs.", _bytesParams.FileStream.Name, _bsaVersion, _currentGame);
					return false;
			}

			// BitConverter.ToBoolean(Byte[], Int32)
			// Read next 4 bytes to find BSA's Folder Record Offset and then determine if it is equal to 36.
			uint _folderRecordsOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_folderRecordsOffset != 36)
			{
				Debug.LogErrorFormat("{1} has an incorrect FolderRecord offset. It should be \"36\" but \"{2}\" was read instead. " +
					"This could indicate a corrupted file.", _bytesParams.FileStream.Name, _folderRecordsOffset);
				return false;
			}

			// Read next 4 bytes to find BSA's Archive Flags. Then determine if some flags are configured correctly.
			// The "Compressed" flag (bit 3) is the only one relevant for extracting files.
			// Bits 1, 2 and 7 are only relevant for checking if the BSA is good. Everything else is either unknown or irrelevant for Unity.
			uint _archiveFlags = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if ((_archiveFlags & 1) != 1)	// Is bit 1 set? - Any number ANDed with 1 is equal to 1 if bit 1 set.
			{
				Debug.LogErrorFormat("{1} has it's \"Names for Directories\" Archive Flag unset when it should be. This could indicate a corrupted file.", _bytesParams.FileStream.Name);
				return false;
			}
			else if ((_archiveFlags & 2) != 2)    // Is bit 2 set? - Any number ANDed with 2 is equal to 2 if bit 2 set.
			{
				Debug.LogErrorFormat("{1} has it's \"Names for Files\" Archive Flag unset when it should be. This could indicate a corrupted file.", _bytesParams.FileStream.Name);
				return false;
			}
			else if ((_archiveFlags & 128) != 128)    // Is bit 7 set? - Any number ANDed with 128 is equal to 128 if bit 7 set.
			{
				Debug.LogErrorFormat("{1} has it's \"Big-Endian\" Archive Flag set when it shouldn't be. This could indicate either a corrupted file " +
					"or that you are trying to use a console version's BSA, which isn't supported.", _bytesParams.FileStream.Name);
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
				Debug.LogErrorFormat("{1} has it's Total Folders counter set to less than 1. This could indicate a corrupted or empty file.", _bytesParams.FileStream.Name);
				return false;
			}

			// Read next 4 bytes to find BSA's Total File Count and check if it's value is not zero. 
			_bsaHeader.TotalFileCount = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalFileCount < 1)
			{
				Debug.LogErrorFormat("{1} has it's Total Files counter set to less than 1. This could indicate a corrupted or empty file.", _bytesParams.FileStream.Name);
				return false;
			}

			// Read next 4 bytes to find the total length of all folder names in this BSA and check if it's value is not zero. 
			_bsaHeader.TotalLengthAllFolderNames = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalLengthAllFolderNames < 1)
			{
				Debug.LogErrorFormat("{1} has it's Total Folder Name length set to less than 1. This could indicate a corrupted or empty file.", _bytesParams.FileStream.Name);
				return false;
			}

			// Read next 4 bytes to find the total length of all file names in this BSA and check if it's value is not zero. 
			_bsaHeader.TotalLengthAllFileNames = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaHeader.TotalLengthAllFileNames < 1)
			{
				Debug.LogErrorFormat("{1} has it's Total File Name length set to less than 1. This could indicate a corrupted or empty file.", _bytesParams.FileStream.Name);
				return false;
			}

			// Read next 4 bytes to find the BSA's set Content Type Flags and check if they are what was expected. 
			uint _bsaContentType = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
			if (_bsaContentType != (uint)_bSAType)
			{
				Debug.LogErrorFormat("{1} has it's Content Type Flags set to {2}, which is not what was expected ({3}). " +
					"This could indicate a corrupted or empty file.", _bytesParams.FileStream.Name, _bsaContentType, (uint)_bSAType);
				return false;
			}

			// The FileStream's pointer should be at the start of the Folder Records. Check if it's position is currently at the offset indicated in the Folder Record Offset uint. 
			long _currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
			if (_currentPointerPosition != _folderRecordsOffset)
			{
				Debug.LogErrorFormat("The pointer for reading data from {1} currently has it's pointer at byte {2} after it finished reading the BSA Header. " +
					"It is supposed to be at byte {3}. This could indicate a corrupted file.", _bytesParams.FileStream.Name, _currentPointerPosition, _folderRecordsOffset);
				return false;
			}

			return true;
		}

		private static bool _readBsaFolderRecords(BsaHeader _bsaHeader, ref BytesParams _bytesParams, out FolderRecords[] _folderRecords)
		{
			_folderRecords = new FolderRecords[_bsaHeader.TotalFolderCount];
			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; ++_i)
			{
				// Read next 8 bytes to find the folder name hash for this folder. 
				_folderRecords[_i].FolderNameHash = BytesToTypes.BytesToULong64(ref _bytesParams, out _);

				// Read next 4 bytes to find this folder's file count and check if it's value is not zero. 
				_folderRecords[_i].FoldersFileCount = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
				if (_folderRecords[_i].FoldersFileCount < 1)
				{
					Debug.LogErrorFormat("{1} has it's Total File Name length set to less than 1. This could indicate a corrupted record.", _bytesParams.FileStream.Name);
					return false;
				}

				// Read next 4 bytes to find the offset to this folder's record for folder name and files details combined with total file name length.
				// Then subtract total file name length because we only need the offset. Then check if the result is not zero.
				_folderRecords[_i].NameAndFileRecordsOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _) - _bsaHeader.TotalLengthAllFileNames;
				if (_folderRecords[_i].NameAndFileRecordsOffset < 1)
				{
					Debug.LogErrorFormat("{1} has one of it's Folder Name And Files Record offsets set to less than 1. This could indicate a corrupted record.", _bytesParams.FileStream.Name);
					return false;
				}
			}
			return true;
		}

		private static bool _readBsaFolderNamesAndFileRecords(BsaHeader _bsaHeader, FolderRecords[] _folderRecords, ref BytesParams _bytesParams,
																out FolderNameAndFileRecords[] _folderNameAndFileRecords)
		{
			uint _countedLengthOfFolderNames = 0;
			_folderNameAndFileRecords = new FolderNameAndFileRecords[_bsaHeader.TotalFolderCount];

			long _currentPointerPosition;
			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; ++_i)
			{
				_currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
				if (_currentPointerPosition != _folderRecords[_i].NameAndFileRecordsOffset)
				{
					Debug.LogErrorFormat("The pointer for reading data from {1} currently has it's pointer at byte {2} while it was reading the Folder Names And File Record #{3}. " +
						"It is supposed to be at byte {4}. This could indicate a corrupted file.", _bytesParams.FileStream.Name, _currentPointerPosition, _i + 1, _folderRecords[_i].NameAndFileRecordsOffset);
					return false;
				}

				// Get the name of the current folder in the folder name and files record, which is stored as a zero-terminated string prefixed with it's length.
				// First byte is the length of the folder name's string. The rest is the string itself.
				byte _folderNameStringLength = BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _);
				_countedLengthOfFolderNames += _folderNameStringLength;
				char[] _bsaStringArr = new char[_folderNameStringLength];
				for (ushort _j = 0; _j < (_folderNameStringLength - 1); ++_j)
				{
					_bsaStringArr[_i] = System.Convert.ToChar(BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _));
				}
				_folderNameAndFileRecords[_i].FolderName = new string(_bsaStringArr);
				// Last byte in folder's name must be a zero value
				if (BytesToTypes.BytesToSingleByte8(ref _bytesParams, out _) != 0)
				{
					Debug.LogErrorFormat("{1}'s folder name \"{2}\" did not end with a zero value. This could indicate a corrupted file.",
						_bytesParams.FileStream.Name, _folderNameAndFileRecords[_i].FolderName);
					return false;
				}

				_folderNameAndFileRecords[_i].FileRecords = new FileRecords[_folderRecords[_i].FoldersFileCount];
				for (uint _j = 0; _j < _folderRecords[_i].FoldersFileCount; ++_j)
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

					// Finally, get the size of the File Block. This size is the size of the raw file data as well as the uncompressed size UInt32 bytes.
					_folderNameAndFileRecords[_i].FileRecords[_j].FileSize = _tempFileSize;

					// Offset to the file's raw data from the start of the BSA.
					_folderNameAndFileRecords[_i].FileRecords[_j].FileDataOffset = BytesToTypes.BytesToUInt32(ref _bytesParams, out _);
				}
			}

			if (_countedLengthOfFolderNames != _bsaHeader.TotalLengthAllFolderNames)
			{
				Debug.LogErrorFormat("{1}'s Total Folder Name Length given in header ({2}) does not match up with the lengths counted for all folder names in the Folder Names And File " +
					"Records ({3}). This could indicate a corrupted file.", _bytesParams.FileStream.Name, _bsaHeader.TotalLengthAllFolderNames, _countedLengthOfFolderNames);
				return false;
			}

			uint _countedLengthOfFileNames = 0;
			for (uint _i = 0; _i < _bsaHeader.TotalFolderCount; ++_i)
			{
				for (uint _j = 0; _j < _folderRecords[_i].FoldersFileCount; ++_j)
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
								_errorStartText = "FileReader for " + _bytesParams.FileStream.Name + " failed to encounter a zero value after 65000 characters while reading the ";
							}
							else
							{
								_errorStartText = "FileReader for " + _bytesParams.FileStream.Name + " failed to encounter a zero value before reaching the end of the file while reading the ";
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
									_errorEndText = "filename after " + _folderNameAndFileRecords[_i].FileRecords[_j-1].FileName + ". This could indicate a corrupted file.";
								}
							}
							else
							{
								if (_j == 0)
								{
									_errorEndText = "filename after " + _folderNameAndFileRecords[_i - 1].FileRecords[_folderNameAndFileRecords[_i - 1].FileRecords.Length - 1].FileName + ". " +
										"This could indicate a corrupted file.";
								}
								else
								{
									_errorEndText = "filename after " + _folderNameAndFileRecords[_i].FileRecords[_j-1].FileName + ". This could indicate a corrupted file.";
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
				Debug.LogErrorFormat("{1}'s Total Folder Name Length given in header ({2}) does not match up with the lengths counted for all folder names in the Folder Names And File " +
					"Records ({3}). This could indicate a corrupted file.", _bytesParams.FileStream.Name, _bsaHeader.TotalLengthAllFolderNames, _countedLengthOfFolderNames);
				return false;
			}

			_currentPointerPosition = _bytesParams.FileStream.Position - 4096 + _bytesParams.CacheCurrentPos;
			if (_currentPointerPosition != _folderNameAndFileRecords[0].FileRecords[0].FileDataOffset)
			{
				Debug.LogErrorFormat("The pointer for reading data from {1} currently has it's pointer at byte {2} after it finished reading the File Name Block. It is supposed to be at " +
					"byte {3}. This could indicate a corrupted file.", _bytesParams.FileStream.Name, _currentPointerPosition, _folderNameAndFileRecords[0].FileRecords[0].FileDataOffset);
				return false;
			}

			return true;
		}
	}
}
