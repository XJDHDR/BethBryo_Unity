// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//

using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	public static class LoggingHelper
	{
		public static ConcurrentStack<LoggingData> LogQueue = new ConcurrentStack<LoggingData>();

#pragma warning disable IDE0051 // Remove unused private members
		// Domain reloading is disabled: https://docs.unity3d.com/Manual/DomainReloading.html
		// Thus, we need this to reset the above static variables in editor mode.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void _resetStaticVars()
		{
			LogQueue.Clear();
		}
#pragma warning restore IDE0051 // Remove unused private members


		public static void PrintAllCurrentLogs()
		{
			int _logQueueCount = LogQueue.Count;
			if (_logQueueCount > 0)
			{
				List<LoggingData> _copiedQueue;
				lock (LogQueue)
				{
					_copiedQueue = new List<LoggingData>(LogQueue);
					LogQueue.Clear();
				}

				_logQueueCount = _copiedQueue.Count;
				for (int _i = _logQueueCount; _i > 0 ; --_i)
				{
					switch ((byte)_copiedQueue[_i].LogSeverity)
					{
						case 1:
							Debug.LogFormat(_copiedQueue[_i].LogMessage);
							break;

						case 2:
							Debug.LogWarningFormat(_copiedQueue[_i].LogMessage);
							break;

						case 3:
							Debug.LogErrorFormat(_copiedQueue[_i].LogMessage);
							break;

						default:
							Debug.LogErrorFormat("The following log message did not have a correct severity value assigned " +
								"(must be 1, 2 or 3 but it was \"" + _copiedQueue[_i].LogSeverity + "\" instead):\n" +
								_copiedQueue[_i].LogMessage);
							break;
					}
				}
			}
		}

		public struct LoggingData
		{
			public LogSeverityValue LogSeverity;
			public string LogMessage;
		}

		public enum LogSeverityValue : byte
		{
			Info	= 1,
			Warning = 2,
			Error	= 3
		}
	}
}
