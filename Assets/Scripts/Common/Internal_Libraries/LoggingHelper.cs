// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//

using System.Collections.Concurrent;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Provides functionality to act as a middleman between scripts and Unity's logging system.
	/// This is intended for use by scripts which either don't run in the main thread or need to not be directly dependant on Unity code.
	/// If these don't apply, it's better to just use Unity's standard logging methods.
	/// </summary>
	public static class LoggingHelper
	{
		/// <summary>
		/// Holds all of the log messages that have been queued to be pushed into Unity's logging system.
		/// This is intended for use by scripts which either don't run in the main thread or need to not be directly dependant on Unity code.
		/// If these don't apply, it's better to just use Unity's standard logging methods.
		/// </summary>
		// Using a ConcurrentStack because it is the only thread-safe container that supports a Clear() method at the time of writing
		// (Unity currently has .NET Standard 2.0 compatibility).
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


		/// <summary>
		/// Called once every frame by the RootEngineManager to print any outstanding log messages.
		/// </summary>
		public static void PrintAllCurrentLogs()
		{
			int _logQueueCount = LogQueue.Count;
			if (_logQueueCount > 0)
			{
				// Prevent race conditions as much as possible.
				LoggingData[] _copiedQueue;
				lock (LogQueue)
				{
					_copiedQueue = LogQueue.ToArray();
					LogQueue.Clear();
				}

				// Iterate through the list backwards because a ConcurrentStack stores entries in a Last-In-First-Out manner
				for (int _i = _copiedQueue.Length - 1; _i >= 0 ; --_i)
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
			/// <summary> Indicates the severity of the logmessage. Is it an error, warning or just debug info? </summary>
			public LogSeverityValue LogSeverity;

			/// <summary> Use this to store the message that will be sent to Unity's logging system. </summary>
			public string LogMessage;
		}

		/// <summary> Indicates the severity of the log message. Is it an error, warning or just debug info? </summary>
		public enum LogSeverityValue : byte
		{
			Info	= 1,
			Warning = 2,
			Error	= 3
		}
	}
}
