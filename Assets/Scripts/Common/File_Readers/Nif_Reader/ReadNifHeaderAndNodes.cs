// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;
using System.Text;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	public class ReadNifHeaderAndNodes
	{
		public static bool GetNifHeaderAndNodes(byte[] NifData, string NifLocation, SupportedGames CurrentGame)
		{
			int _curArrayPos = 0;

			// The first bytes are a newline terminated Header String so read bytes until a newline is encountered. Mark that position when it's found.
			// Also, the last space in this string is the position where the NIF version can be found, so mark that position as well.
			byte _lastSpacePosition = 0;
			while (_curArrayPos < 252)
			{
				if (NifData[_curArrayPos] == 0x0A)   // Newline character
				{
					_curArrayPos += 1;      // Move to the position after the newline to get ready to read block of data after the Header String
					break;
				}
				else if (NifData[_curArrayPos] == 0x20)		// Space character
				{
					_lastSpacePosition = (byte)_curArrayPos;
				}
				_curArrayPos += 1;

				if (_curArrayPos > 250)
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"Failed to find a newline character while reading it's Header String.\n" +
						"This could indicate a corrupt file.");
					return false;
				}
			}

			// Now that we know the position of the newline and last space in that string, extract the NIF version used to build this model from the Header String.
			// The version string consists of 4 parts separated by fullstops. The names I'm giving them are as follows: <Major>.<Minor>.<Build>.<Private>
			char[] _stringChars = new char[_curArrayPos - _lastSpacePosition - 1];	// This formula is the length of the version substring. Newline Pos - Space Pos - Newline Char length
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

			// Next, check if the rest of the Header String matches what we expect it to say.
			// For versions less than or equal to 10.0.1.0, the text must say "NetImmerse File Format, Version"
			// For versions greater than or equal to 10.1.0.0, the text must say "Gamebryo File Format, Version"
			_stringChars = new char[_curArrayPos - 1];
			for (byte _i = 0; _i < _stringChars.Length; ++_i)
			{
				_stringChars[_i] = Convert.ToChar(NifData[_i]);
			}
			string _headerTextSubString = _stringChars.ToString();
			if ((_nifVersionMajor < 10) || (_nifVersionMajor == 10 && _nifVersionMinor == 0 && _nifVersionBuild <= 1))
			{
				if (_headerTextSubString != "NetImmerse File Format, Version")
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"It's Header String is supposed to say \"NetImmerse File Format, Version\" but instead, it says: \"" + _headerTextSubString + "\".\n" +
						"This could indicate a corrupt file.");
					return false;
				}
			}
			else if ((_nifVersionMajor > 10) || (_nifVersionMajor == 10 && _nifVersionMinor >= 1))
			{
				if (_headerTextSubString != "Gamebryo File Format, Version")
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"It's Header String is supposed to say \"Gamebryo File Format, Version\" but instead, it says: \"" + _headerTextSubString + "\".\n" +
						"This could indicate a corrupt file.");
					return false;
				}
			}

			// Next, Nif versions up to 3.1.0.0 will have a copyright notice be the next block of data. This is a newline terminated string.
			// We don't need any of this data so if present, just skip ahead to the next block.
			if ((_nifVersionMajor < 3) || (_nifVersionMajor == 3 && _nifVersionMinor < 1) || 
				(_nifVersionMajor == 3 && _nifVersionMinor == 1 && _nifVersionBuild == 0 && _nifVersionPrivate == 0))
			{
				ushort _loopIterations = 0;
				while (_loopIterations < 65503)
				{
					if (NifData[_curArrayPos] == 0x0A)   // Newline character
					{
						_curArrayPos += 1;      // Move to the position after the newline to get ready to read block of data after the Header String
						break;
					}
					_curArrayPos += 1;

					if (_curArrayPos > 65500)
					{
						Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
							"Failed to find a newline character while reading it's Copyright string.\n" +
						"This could indicate a corrupt file.");
						return false;
					}
				}
			}

			// Next, Nif versions from 3.1.0.1 and up will have the version number repeated in hexadecimal format (Endianness being respected, I think).
			// We can check these against what was extracted from the Header String
			if ((_nifVersionMajor > 3) || (_nifVersionMajor == 3 && _nifVersionMinor > 1) || (_nifVersionMajor == 3 && _nifVersionMinor == 1 && _nifVersionBuild > 0) || 
				(_nifVersionMajor == 3 && _nifVersionMinor == 1 && _nifVersionBuild == 0 && _nifVersionPrivate >= 1))
			{
				if (NifData[_curArrayPos] != _nifVersionPrivate)
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"The Private Version read from the header string does not match the Private Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionPrivate + "\" but the Version block says it's \"" + NifData[_curArrayPos] + "\"\n" +
						"This could indicate a corrupt file.");
					return false;
				}
				_curArrayPos += 1;
				if (NifData[_curArrayPos] != _nifVersionBuild)
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"The Build Version read from the header string does not match the Build Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionBuild + "\" but the Version block says it's \"" + NifData[_curArrayPos] + "\"\n" +
						"This could indicate a corrupt file.");
					return false;
				}
				_curArrayPos += 1;
				if (NifData[_curArrayPos] != _nifVersionMinor)
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"The Minor Version read from the header string does not match the Minor Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionMinor + "\" but the Version block says it's \"" + NifData[_curArrayPos] + "\"\n" +
						"This could indicate a corrupt file.");
					return false;
				}
				_curArrayPos += 1;
				if (NifData[_curArrayPos] != _nifVersionMajor)
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"The Major Version read from the header string does not match the Major Version read from the Version hex block.\n" +
						"The Header says it's \"" + _nifVersionMajor + "\" but the Version block says it's \"" + NifData[_curArrayPos] + "\"\n" +
						"This could indicate a corrupt file.");
					return false;
				}
				_curArrayPos += 1;
			}

			// Next, Nif versions from 20.0.0.3 up will have an endianess bit be the next block of data. This is a single byte with one of two values.
			// Test if this is a little endian model, as big endian models (used in the Xbox 360 and PS3 versions) are unsupported.
			if ((_nifVersionMajor > 20) || (_nifVersionMajor == 20 && _nifVersionMinor > 0) || (_nifVersionMajor == 20 && _nifVersionMinor == 0 && _nifVersionBuild > 0) ||
				(_nifVersionMajor == 20 && _nifVersionMinor == 0 && _nifVersionBuild == 0 && _nifVersionPrivate >= 3))
			{
				if (NifData[_curArrayPos] == 0x00)		// 0x01 = Little endian. 0x00 = Big endian.
				{
					Debug.LogErrorFormat("Error while reading Nif file: " + NifLocation + "\n" +
						"The endianess bit is set to 0.\n" +
						"This could indicate either a corrupt file or that this model was extracted from the Xbox 360 or PS3 version of the game, neither of which are supported.");
					return false;
				}
				_curArrayPos += 1;
			}

			uint? _userVersion = null;
			// Next, Nif versions from 10.0.1.8 up will have a User Version number be the next block of data. This is a 32-bit integer.
			// A few features require that we know this number so hold onto it.
			if ((_nifVersionMajor > 10) || (_nifVersionMajor == 10 && _nifVersionMinor > 0) || (_nifVersionMajor == 10 && _nifVersionMinor == 0 && _nifVersionBuild > 1) ||
				(_nifVersionMajor == 10 && _nifVersionMinor == 0 && _nifVersionBuild == 1 && _nifVersionPrivate >= 8))
			{
				_userVersion = BitConverter.ToUInt32(NifData, _curArrayPos);
				_curArrayPos += 4;
			}



			return true;
		}
	}
}
