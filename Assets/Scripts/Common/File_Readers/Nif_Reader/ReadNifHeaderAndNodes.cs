// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System;
using System.Text;

namespace BethBryo_for_Unity_Common
{
	public class ReadNifHeaderAndNodes
	{
		public static bool GetNifHeaderAndNodes(byte[] NifData, SupportedGames CurrentGame)
		{
			int _curArrayPos = 0;

			// The first bytes are a newline terminated string so read bytes until a newline is encountered.
			ushort _loopLimiter = 0;
			StringBuilder _headerStringBuild = new();
			while (_loopLimiter < 65503)
			{
				_loopLimiter += 1;

				byte _currentByte = NifData[_curArrayPos];
				_curArrayPos += 1;
				if (_currentByte == 0x0A)   // Newline character
				{
					break;
				}
				else
				{
					_headerStringBuild.Append(Convert.ToChar(_currentByte));
				}

			}

			return true;
		}
	}
}
