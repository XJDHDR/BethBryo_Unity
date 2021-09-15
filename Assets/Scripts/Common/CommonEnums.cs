// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

namespace BethBryo_for_Unity_Common
{
	public enum DebugLevel : byte
	{
		ErrorsAndWarningsOnly	= 0,
		Info					= 1,
		Debug					= 2
	}

	public enum SupportedGames : byte
	{
		NoGame		= 0,
		Morrowind	= 1,
		Oblivion	= 2,
		Fallout3	= 3,
		FalloutNV	= 4,
		Skyrim		= 5,
		SkyrimSE	= 6,
		Fallout4	= 7
	}
}
