// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using Force.Crc32;
using System;

namespace BethBryo_for_Unity_Common
{
	public static class NifHeader
	{
		internal static bool GetNifHeaderAndNodes(byte[] NifData, string NifLocation, SupportedGames CurrentGame, ref int CurArrayPos, out FileNifStructs.NifHeaderData NifHeaderData)
		{
			NifHeaderData = new FileNifStructs.NifHeaderData
			{
				NifVersionCombined = 0,
				UserVersion = null,
				NumberOfBlocks = null,
				BSVersion = null,
				BlockTypesQuantity = null,
				BlockTypesList = new string[0],
				BlockTypesListCRC32Hashes = new uint[0],
				BlockTypesHashes = new uint[0],
				BlockTypesToObjectIndex = new ushort[0],
				BlockSizes = new uint[0],
				StringsQuantity = null,
				StringsMaxLength = null,
				StringsList = new string[0],
				QtyGroups = 0,
				Groups = new uint[0]
			};

			// The first bytes are a newline terminated Header String. Thus, read bytes until the newline is encountered. Mark that position when it's found.
			// Also, the last space in this string is the position where the NIF version can be found, so mark that position as well.
			byte _lastSpacePosition = 0;
			ushort _loopIterations = 0;
			int _loopStartLocation = 0;
			while (_loopIterations < 252)
			{
				_loopIterations += 1;
				if (NifData[CurArrayPos] == 0x0A)			// Newline character
				{
					CurArrayPos += 1;      // Move to the position after the newline to get ready to read block of data after the Header String
					break;
				}
				else if (NifData[CurArrayPos] == 0x20)		// Space character
				{
					_lastSpacePosition = (byte)CurArrayPos;
				}
				CurArrayPos += 1;

				if (_loopIterations > 250)
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"Failed to find a newline character while reading it's Header String starting at byte " + _loopStartLocation + ".\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
			}

			// Now that we know the position of the newline and last space in that string, extract the NIF version used to build this model from the Header String.
			// The version string consists of 4 parts separated by fullstops. The names I'm giving them are as follows: <Major>.<Minor>.<Build>.<Private>
			char[] _stringChars = new char[CurArrayPos - _lastSpacePosition - 1];	// This formula is the length of the version substring. Newline Pos - Space Pos - Newline Char length
			for (byte _i = 0; _i < _stringChars.Length; ++_i)
			{
				_stringChars[_i] = Convert.ToChar(NifData[_lastSpacePosition + 1 + _i]);	// Start reading from position after the last space
			}
			string _versionSubString = _stringChars.ToString();
			string[] _versionStringArr = _versionSubString.Split('.');
			byte _nifVersionMajor = Convert.ToByte(_versionStringArr[0]);
			byte _nifVersionMinor = Convert.ToByte(_versionStringArr[1]);
			byte _nifVersionBuild = Convert.ToByte(_versionStringArr[2]);
			byte _nifVersionPrivate = Convert.ToByte(_versionStringArr[3]);
			NifHeaderData.NifVersionCombined = (uint)((_nifVersionMajor << 24) | (_nifVersionMinor << 16) | (_nifVersionBuild << 8) | (_nifVersionPrivate << 0));

			// Next, check if the rest of the Header String matches what we expect it to say.
			// For versions less than or equal to 10.0.1.2, the text must say "NetImmerse File Format, Version"
			// For versions greater than or equal to 10.1.0.0, the text must say "Gamebryo File Format, Version"
			_stringChars = new char[CurArrayPos - 1];
			for (byte _i = 0; _i < _stringChars.Length; ++_i)
			{
				_stringChars[_i] = Convert.ToChar(NifData[_i]);
			}
			string _headerTextSubString = _stringChars.ToString();
			if (NifHeaderData.NifVersionCombined < 0x0A010000)		// If less than 10.1.0.0
			{
				if (_headerTextSubString != "NetImmerse File Format, Version")
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"It's Header String is supposed to say \"NetImmerse File Format, Version\" but instead, it says: \"" + _headerTextSubString + "\".\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
			}
			else
			{
				if (_headerTextSubString != "Gamebryo File Format, Version")
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"It's Header String is supposed to say \"Gamebryo File Format, Version\" but instead, it says: \"" + _headerTextSubString + "\".\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
			}

			// Next, Nif versions up to 3.1.0.0 will have a copyright notice be the next block of data. This is a newline terminated string.
			// We don't need any of this data so if present, just skip ahead to the next block.
			if (NifHeaderData.NifVersionCombined <= 0x03010000)		// If less than or equal to 3.1.0.0
			{
				_loopIterations = 0;
				_loopStartLocation = CurArrayPos;
				while (_loopIterations < 65503)
				{
					if (NifData[CurArrayPos] == 0x0A)   // Newline character
					{
						CurArrayPos += 1;		// Move to the position after the newline to get ready to read block of data after the Header String
						break;
					}
					CurArrayPos += 1;

					if (CurArrayPos > 65500)
					{
						LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
						{
							LogSeverity = LoggingHelper.LogSeverityValue.Error,
							LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
							"Failed to find a newline character while reading it's Copyright string starting at byte " + _loopStartLocation + ".\n" +
							"This could indicate a corrupt file."
						});
						return false;
					}
				}
			}

			// Next, Nif versions from 3.1.0.1 and up will have the version number repeated in hexadecimal format (Endianness being respected, I think).
			// We can check these against what was extracted from the Header String
			if (NifHeaderData.NifVersionCombined >= 0x03010001)		// If greater than or equal to 3.1.0.1
			{
				if (NifData[CurArrayPos] != _nifVersionPrivate)
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The Private Version read from the header string at byte " + CurArrayPos + " does not match the Private Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionPrivate + "\" but the Version block says it's \"" + NifData[CurArrayPos] + "\"\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
				CurArrayPos += 1;
				if (NifData[CurArrayPos] != _nifVersionBuild)
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The Build Version read from the header string at byte " + CurArrayPos + " does not match the Build Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionBuild + "\" but the Version block says it's \"" + NifData[CurArrayPos] + "\"\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
				CurArrayPos += 1;
				if (NifData[CurArrayPos] != _nifVersionMinor)
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The Minor Version read from the header string at byte " + CurArrayPos + " does not match the Minor Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionMinor + "\" but the Version block says it's \"" + NifData[CurArrayPos] + "\"\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
				CurArrayPos += 1;
				if (NifData[CurArrayPos] != _nifVersionMajor)
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The Major Version read from the header string at byte " + CurArrayPos + " does not match the Major Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionMajor + "\" but the Version block says it's \"" + NifData[CurArrayPos] + "\"\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
				CurArrayPos += 1;
			}

			// Next, Nif versions from 20.0.0.3 up will have an endianess bit be the next block of data. This is a single byte with one of two values.
			// Test if this is a little endian model, as big endian models (used in the Xbox 360 and PS3 versions) aren't guaranteed to be read properly.
			if (NifHeaderData.NifVersionCombined >= 0x14000003)		// If greater than or equal to 20.0.0.3
			{
				if (NifData[CurArrayPos] == 0x00)		// 0x00 = Big endian.
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The endianess bit at byte " + CurArrayPos + " is set to 0.\n" +
						"This could indicate either a corrupt file or that this model was extracted from the Xbox 360 or PS3 version of the game, neither of which are currently supported."
					});
					return false;
				}
				else if (NifData[CurArrayPos] != 0x01)	// 0x01 = Little endian.
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
						"The endianess bit at byte " + CurArrayPos + " is not set to either 0 or 1. This could indicate a corrupt file."
					});
					return false;
				}
				CurArrayPos += 1;
			}

			// Next, Nif versions from 10.0.1.8 up will have a User Version number be the next block of data. This is a 32-bit integer.
			// A few features require that we know this number so hold onto it.
			if (NifHeaderData.NifVersionCombined >= 0x0A000108)		// If greater than or equal to 10.0.1.8
			{
				NifHeaderData.UserVersion = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
			}

			// Next, Nif versions from 3.1.0.1 and up will have the number of file objects as a 32-bit integer.
			if (NifHeaderData.NifVersionCombined >= 0x03010001)		// If greater than or equal to 3.1.0.1
			{
				NifHeaderData.NumberOfBlocks = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
			}

			// Next, Nif versions labelled as BSStreamHeader versions will have a bunch of info about how the file was exported.
			// We don't need most of the info here (with one exception) so just skip past all of it
			if (_doBSStreamHeaderConditionCheck(NifHeaderData.NifVersionCombined, NifHeaderData.UserVersion))
			{
				_doSkipOverBSStreamHeader(NifData, NifLocation, ref CurArrayPos, out NifHeaderData.BSVersion);
			}

			// Next, Nif versions from 30.0.0.0 up will have some Metadata be the next block of data. This is a byte array.
			// We don't need this so just skip past all of it
			if (NifHeaderData.NifVersionCombined >= 0x1E000000)		// If greater than or equal to 30.0.0.0
			{
				CurArrayPos += NifData[CurArrayPos];
			}

			// Next, Nif versions from 5.0.0.1 up will have the number of block types used in this NIF,
			// a list of them in string or, in one specific version, hash form and a map of those types to their index in the file.
			if (NifHeaderData.NifVersionCombined >= 0x05000001)		// If greater than or equal to 5.0.0.1
			{
				//First two bytes is a 16-bit integer indicating the number of block types listed.
				NifHeaderData.BlockTypesQuantity = BitConverter.ToUInt16(NifData, CurArrayPos);
				CurArrayPos += 2;

				if (NifHeaderData.NifVersionCombined != 0x14030102)   // If not equal to 20.3.1.2
				{
					uint _stringLength;
					NifHeaderData.BlockTypesListCRC32Hashes = new uint[(int)NifHeaderData.BlockTypesQuantity];
					NifHeaderData.BlockTypesList = new string[(int)NifHeaderData.BlockTypesQuantity];
					for (ushort _i = 0; _i < NifHeaderData.BlockTypesQuantity; ++_i)
					{
						// After that, each block type is an unterminated string prefixed by a 32-bit integer (why?) indicating it's length.
						_stringLength = BitConverter.ToUInt32(NifData, CurArrayPos);
						CurArrayPos += 4;

						_loopStartLocation = CurArrayPos;
						_stringChars = new char[_stringLength];
						for (uint _j = 0; _j < _stringLength; ++_j)
						{
							_stringChars[_j] = Convert.ToChar(NifData[CurArrayPos]);
							CurArrayPos += 1;
						}
						NifHeaderData.BlockTypesList[_i] = _stringChars.ToString();
						NifHeaderData.BlockTypesListCRC32Hashes[_i] = Crc32Algorithm.Compute(NifData, _loopStartLocation, CurArrayPos);
					}
				}
				else
				{
					NifHeaderData.BlockTypesHashes = new uint[(int)NifHeaderData.BlockTypesQuantity];
					for (ushort _i = 0; _i < NifHeaderData.BlockTypesQuantity; ++_i)
					{
						// One specific Nif version stores hashes of the block types instead. 
						NifHeaderData.BlockTypesHashes[_i] = BitConverter.ToUInt32(NifData, CurArrayPos);
						CurArrayPos += 4;
					}
				}

				// Finally, we need to create an index array that maps these block types to the objects defined in the Nif file.
				NifHeaderData.BlockTypesToObjectIndex = new ushort[(int)NifHeaderData.NumberOfBlocks];
				for (uint _i = 0; _i < NifHeaderData.NumberOfBlocks; ++_i)
				{
					NifHeaderData.BlockTypesToObjectIndex[_i] = BitConverter.ToUInt16(NifData, CurArrayPos);
					if (NifHeaderData.BlockTypesToObjectIndex[_i] >= 32768)	// The last bit appears to be an unknown flag used by the PhysX engine. Don't need it so remove it if set.
					{
						NifHeaderData.BlockTypesToObjectIndex[_i] -= 32768;
					}
					CurArrayPos += 2;
				}
			}

			// Next, Nif versions from 20.2.0.5 up will have an array of the sizes for each object block.
			if (NifHeaderData.NifVersionCombined >= 0x14020005)      // If greater than or equal to 20.2.0.5
			{
				NifHeaderData.BlockSizes = new uint[(int)NifHeaderData.NumberOfBlocks];
				for (uint _i = 0; _i < NifHeaderData.NumberOfBlocks; ++_i)
				{
					NifHeaderData.BlockSizes[_i] = BitConverter.ToUInt32(NifData, CurArrayPos);
					CurArrayPos += 4;
				}
			}

			// Next, Nif versions from 20.1.0.1 up will have an array of strings used in this Nif file.
			if (NifHeaderData.NifVersionCombined >= 0x14010001)      // If greater than or equal to 20.1.0.1
			{
				NifHeaderData.StringsQuantity = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
				NifHeaderData.StringsMaxLength = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;

				uint _stringLength;
				NifHeaderData.StringsList = new string[(int)NifHeaderData.StringsQuantity];
				for (uint _i = 0; _i < NifHeaderData.StringsQuantity; ++_i)
				{
					_loopStartLocation = CurArrayPos;

					// After that, each string is an unterminated string prefixed by a 32-bit integer indicating it's length.
					_stringLength = BitConverter.ToUInt32(NifData, CurArrayPos);
					CurArrayPos += 4;

					if (_stringLength > NifHeaderData.StringsMaxLength)
					{
						LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
						{
							LogSeverity = LoggingHelper.LogSeverityValue.Error,
							LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
							"While reading the Strings array, the string length at " + _loopStartLocation + " (" + _stringLength + ") is greater than the read " +
							"Max String Length value (" + NifHeaderData.StringsMaxLength + ").\n" + "This could indicate a corrupt file."
						});
						return false;
					}

					_stringChars = new char[_stringLength];
					for (uint _j = 0; _j < _stringLength; ++_j)
					{
						_stringChars[_j] = Convert.ToChar(NifData[CurArrayPos]);
						CurArrayPos += 1;
					}
					NifHeaderData.StringsList[_i] = _stringChars.ToString();
				}
			}

			// Next, Nif versions from 5.0.0.6 up will have an array of groups identified by 32-bit integers.
			if (NifHeaderData.NifVersionCombined >= 0x05000006)
			{
				NifHeaderData.QtyGroups = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;

				if (NifHeaderData.QtyGroups > 0)
				{
					NifHeaderData.Groups = new uint[NifHeaderData.QtyGroups];
					for (uint _i = 0; _i < NifHeaderData.QtyGroups; ++_i)
					{
						NifHeaderData.Groups[_i] = BitConverter.ToUInt32(NifData, CurArrayPos);
						CurArrayPos += 4;
					}
				}
			}

			// And with that, the header has been completely parsed. Next, the data in each individual block needs to be parsed.
			return true;
		}

		private static bool _doBSStreamHeaderConditionCheck(uint _nifVersionCombined, uint? _userVersion)
		{
			switch (_nifVersionCombined)
			{
				case 167772418:		// If equal to 10.0.1.2
					return true;

				case 335544325:     // If equal to 20.0.0.5
					return true;

				case 335675399:     // If equal to 20.2.0.7
					return true;

				default:
					if ((_nifVersionCombined >= 167837696) && (_nifVersionCombined <= 335544324) && (_userVersion >= 3) && (_userVersion <= 11))
					{	//		If version >= 10.1.0.0		AND			version  <= 20.0.0.4
						return true;
					}
					else
					{
						return false;
					}
			}
		}

		private static bool _doSkipOverBSStreamHeader(byte[] _nifData, string _nifLocation, ref int _curArrayPos, out uint? _bSVersion)
		{
			// First piece of data is the BS Version as a 32-bit integer.
			// Read this because it is the one value we need here and elsewhere.
			_bSVersion = BitConverter.ToUInt32(_nifData, _curArrayPos);
			_curArrayPos += 4;

			// Next is the author in a null-terminated string prefixed by it's length.
			// Skip ahead by the number of bytes given in the prefix but also check if we are on track.
			_curArrayPos += _nifData[_curArrayPos];
			if (_nifData[_curArrayPos] == 0x00)
			{
				_curArrayPos += 1;
			}
			else
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while reading Nif file: " + _nifLocation + "\n" +
					"While skipping over the author's name, a null was expected at byte " + _curArrayPos + " but " + _nifData[_curArrayPos] + " was read instead.\n" +
					"This could indicate a corrupt file."
				});
				return false;
			}

			// Next is an unknown 32-bit integer that is only present if BS Version is greater than 130. Skip over it if present.
			if (_bSVersion > 130)
			{
				_curArrayPos += 4;
			}

			// Next is the Processing Script name in a null-terminated string prefixed by it's length.
			// Skip ahead by the number of bytes given in the prefix but also check if we are on track.
			_curArrayPos += _nifData[_curArrayPos];
			if (_nifData[_curArrayPos] == 0x00)
			{
				_curArrayPos += 1;
			}
			else
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while reading Nif file: " + _nifLocation + "\n" +
					"While skipping over the Processing Script's name, a null was expected at byte " + _curArrayPos + " but " + _nifData[_curArrayPos] + " was read instead.\n" +
					"This could indicate a corrupt file."
				});
				return false;
			}

			// Next is the Export Script name in a null-terminated string prefixed by it's length.
			// Skip ahead by the number of bytes given in the prefix but also check if we are on track.
			_curArrayPos += _nifData[_curArrayPos];
			if (_nifData[_curArrayPos] == 0x00)
			{
				_curArrayPos += 1;
			}
			else
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while reading Nif file: " + _nifLocation + "\n" +
					"While skipping over the Export Script's name, a null was expected at byte " + _curArrayPos + " but " + _nifData[_curArrayPos] + " was read instead.\n" +
					"This could indicate a corrupt file."
				});
				return false;
			}

			// Next is the Max Filepath in a null-terminated string prefixed by it's length. Only present if BS Version is equal to 130.
			// Skip ahead by the number of bytes given in the prefix if present but also check if we are on track.
			if (_bSVersion == 130)
			{
				_curArrayPos += _nifData[_curArrayPos];
				if (_nifData[_curArrayPos] == 0x00)
				{
					_curArrayPos += 1;
				}
				else
				{
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = "Error while reading Nif file: " + _nifLocation + "\n" +
						"While skipping over the Max Filepath, a null was expected at byte " + _curArrayPos + " but " + _nifData[_curArrayPos] + " was read instead.\n" +
						"This could indicate a corrupt file."
					});
					return false;
				}
			}

			return true;
		}
	}
}
