// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Provides a number of methods used to read common data arrangements in Nif files.
	/// </summary>
	internal static class FileNifCommonMethods
	{
		internal static bool ReadBoolean(byte[] NifData, string NifLocation, uint NifVersion, ref int CurArrayPos, out bool Boolean)
		{
			uint _readBytes;
			int _startingPosition = CurArrayPos;
			if (NifVersion >= 0x04010001)       // If greter than or equal to version 4.1.0.1
			{
				_readBytes = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
			}
			else if (NifVersion >= 0x04000002)  // If greter than or equal to version 4.0.0.2
			{
				_readBytes = NifData[CurArrayPos];
				CurArrayPos += 1;
			}
			else
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
					"While reading a boolean at position " + _startingPosition + ", the Nif version passed (" + NifVersion + ") was less than 4.0.0.2, which is not allowed.\n" +
					"This could indicate a corrupt file."
				});
				Boolean = false;
				return false;
			}

			if (_readBytes == 1)
			{
				Boolean = true;
			}
			else if (_readBytes == 0)
			{
				Boolean = false;
			}
			else
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while reading Nif file: " + NifLocation + "\n" +
					"While reading a boolean at position " + _startingPosition + ", the value read (" + _readBytes + ") was not 0 or 1.\n" +
					"This could indicate a corrupt file."
				});
				Boolean = false;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads a Sized String from the Nif data array. This is a string that is prefixed with a 32-bit int stating the string's length.
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="CurArrayPos">The location of the array reading pointer.</param>
		/// <param name="SizedString">The string that was read from the Nif data.</param>
		internal static void ReadSizedString(byte[] NifData, ref int CurArrayPos, out string SizedString)
		{
			uint _stringLength = BitConverter.ToUInt32(NifData, CurArrayPos);
			CurArrayPos += 4;

			char[] _stringChars = new char[_stringLength];
			for (uint _i = 0; _i < _stringLength; ++_i)
			{
				_stringChars[_i] = Convert.ToChar(NifData[CurArrayPos]);
				CurArrayPos += 1;
			}
			SizedString = _stringChars.ToString();
		}
	}
}
