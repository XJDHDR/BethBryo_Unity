// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

namespace BethBryo_for_Unity_Common
{
	public static class CommonSettings
	{
		#if UNITY_EDITOR
			public static DebugLevel DebugLevel = DebugLevel.Info;
		#else
			public static DebugLevel DebugLevel = DebugLevel.ErrorsAndWarningsOnly;
		#endif
	}
}
