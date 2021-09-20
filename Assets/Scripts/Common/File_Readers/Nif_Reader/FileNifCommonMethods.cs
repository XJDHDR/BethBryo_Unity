// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;

namespace BethBryo_for_Unity_Common
{
	internal static class FileNifCommonMethods
	{
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
