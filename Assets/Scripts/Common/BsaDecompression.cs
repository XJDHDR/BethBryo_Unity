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
		public static void ExtractBsaContents(string _pathToBsa, string _gameName)
		{
			if (File.Exists(_pathToBsa))
			{
				using (FileStream _bsaFileStream = new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader _bsaToRead = new BinaryReader(_bsaFileStream))
					{
						if (_readBsaHeader(_bsaToRead, _gameName, out uint _totalFolderCount, out bool _bsaIsCompressed))
						{
							_readBsaFolderRecords(_bsaToRead, _totalFolderCount, out FolderRecords[] _folderRecords);
							_readBsaFolderNamesAndFileRecords(_bsaFileStream, _bsaToRead, _totalFolderCount, _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords);
						}
					}
				}
			}
		}

		internal static bool _readBsaHeader(BinaryReader _openedBsa, string _gameName, out uint _totalFolderCount, out bool _bsaIsCompressed)
		{
			// Set some initial default values for the output variables
			_totalFolderCount = 0;
			_bsaIsCompressed = false;

			// Read first 4 bytes of file and determine if those bytes are valid (it is "BSA " for every BethBryo BSA).
			string _characterCode = new string(_openedBsa.ReadChars(4));
			if (! Equals(_characterCode, "BSA "))
			{
				Debug.LogErrorFormat("BSA {_openedBsa} does not begin with the correct character code. It should be \"BSA \" but \"{_characterCode}\" was read instead. This could indicate a corrupted file.");
				_totalFolderCount = 0;
				return false;
			}

			// Read next 4 bytes to find BSA's version and determine if it is valid for the current game.
			uint _bsaVersion = _openedBsa.ReadUInt32();
			switch (_bsaVersion)
			{
				case 103 when _gameName == "Oblivion":
				//case 103 when _gameName == "Fallout3":
				//case 103 when _gameName == "FalloutNV":
				//case 104 when _gameName == "Skyrim":
				//case 105 when _gameName == "Fallout4":
				//case 105 when _gameName == "SkyrimSE":
					// BSA is valid so continue executing this method.
					break;

				default:
					Debug.LogErrorFormat("BSA {_openedBsa} has a version number of {_bsaVersion}, which does not match that used by {_gameName} BSAs.");
					return false;
			}

			// Read next 4 bytes to find BSA's Archive Flags. Then determine if some flags are configured correctly and extract value of "Compressed" flag if so.
			uint _folderRecordsOffset = _openedBsa.ReadUInt32();
			if (_folderRecordsOffset != 36)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has an incorrect FolderRecord offset. It should be \"36\" but \"{_folderRecordsOffset}\" was read instead. This could indicate a corrupted file.");
				return false;
			}

			uint _archiveFlags = _openedBsa.ReadUInt32();
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
			_totalFolderCount = _openedBsa.ReadUInt32();
			if (_totalFolderCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Folders counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			uint _totalFileCount = _openedBsa.ReadUInt32();
			if (_totalFileCount < 1)
			{
				Debug.LogErrorFormat("BSA {_openedBsa} has it's Total Files counter set to less than 1. This could indicate a corrupted or empty file.");
				return false;
			}

			uint _totalLengthAllFolderNames = _openedBsa.ReadUInt32();
			uint _totalLengthAllFileNames = _openedBsa.ReadUInt32();
			uint _bsaContentType = _openedBsa.ReadUInt32();
			return true;
		}

		internal static void _readBsaFolderRecords(BinaryReader _openedBsa, uint _totalFolderCount, out FolderRecords[] _folderRecords)
		{
			_folderRecords = new FolderRecords[_totalFolderCount];
			for (uint i = 0; i < _totalFolderCount; i += 1)
			{
				_folderRecords[i]._folderNameHash = _openedBsa.ReadUInt64();
				_folderRecords[i]._foldersFileCount = _openedBsa.ReadUInt32();
				_folderRecords[i]._nameAndFileRecordsOffset = _openedBsa.ReadUInt32();
			}
		}

		internal static void _readBsaFolderNamesAndFileRecords(FileStream _bsaFileStream, BinaryReader _openedBsa, uint _totalFolderCount, 
																FolderRecords[] _folderRecords, out FolderNameAndFileRecords[] _folderNameAndFileRecords)
		{
			_folderNameAndFileRecords = new FolderNameAndFileRecords[_totalFolderCount];
			int _folderNameStringLength;

			for (uint i = 0; i < _totalFolderCount; i += 1)
			{
				// Get the 
				_folderNameStringLength = _openedBsa.ReadChar();        // First byte is the length of the folder name's string.
				_folderNameAndFileRecords[i]._folderName = new string(_openedBsa.ReadChars(_folderNameStringLength - 1));
				_bsaFileStream.Position += 1;      // Last character in string is a null terminator so move past it.
			}
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
