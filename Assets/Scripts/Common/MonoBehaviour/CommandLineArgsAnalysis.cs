// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using Force.Crc32;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Provides methods for reading the command line arguments passed to Unity.
	/// </summary>
	internal static class CommandLineArgsAnalysis
	{
		/// <summary>
		/// Provides methods for reading the command line arguments passed to Unity.
		/// </summary>
		/// <returns> True if the game the user was trying to launch was successfully found. False if this was not the case. </returns>
		internal static bool InterpretCommandLineArguments(out SupportedGames RunningGame)
		{
			RunningGame = SupportedGames.NoGame;

			// First, create a CRC32 hash of almost every argument. Skip the first because this is just the EXE name.
			string[] _commandLineArgs = Environment.GetCommandLineArgs();
			uint[] _commandLineArgsHashes = new uint[_commandLineArgs.Length - 1];
			for (byte _i = 0; _i < _commandLineArgsHashes.Length; _i++)
			{
				_commandLineArgsHashes[_i] = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(_commandLineArgs[_i + 1].ToLower(CultureInfo.InvariantCulture)));
			}

			// First, check if the command line shows which game the user is trying to launch.
			bool _gameFound = false;
			for (byte _i = 0; _i < _commandLineArgsHashes.Length; _i++)
			{
				switch (_commandLineArgsHashes[_i])
				{
					// "+LaunchOblivion"
					case 0x28445053:
						RunningGame = SupportedGames.Oblivion;
						_gameFound = true;
						break;

					default:
						break;
				}

				if (_gameFound)
					break;
			}

			// Since the game wasn't found in the command line arguments, see if there is a LaunchGame.txt in the Persistent Data Path folder which has a name.
			if (!_gameFound)
			{
				if (File.Exists($"{Application.persistentDataPath}/LaunchGame.txt"))
				{
					uint _launchGameContentsHash = Crc32Algorithm.Compute(File.ReadAllBytes($"{Application.persistentDataPath}/LaunchGame.txt"));
					switch (_launchGameContentsHash)
					{
						// "Oblivion"
						case 0x239fbc21:
							RunningGame = SupportedGames.Oblivion;
							_gameFound = true;
							break;

						default:
							break;
					}
				}
			}

			// Game could not be found so raise an error and pass this failure to the RootEngineManager. Otherwise, proceed with the remaining tasks.
			if (!_gameFound)
			{
				LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
				{
					LogSeverity = LoggingHelper.LogSeverityValue.Error,
					LogMessage = "Error while detecting which game was launched.\n" +
						"Could not find an identifier for which game the user is trying to play in either the command line arguments or LaunchGame.txt.\n" +
						"The program will now exit."
				});
				return false;
			}
			else
			{
				// Check if the user has specified any of the command line arguments Unity recognises.
				for (byte _i = 0; _i < _commandLineArgsHashes.Length; _i++)
				{
					switch (_commandLineArgsHashes[_i])
					{
						// "-batchmode"
						case 0xbe76c4e6:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Batch mode."
							});
							break;

						// "-disable-gpu-skinning"
						case 0xef660e2d:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has force disabled GPU skinning."
							});
							break;

						// "-force-d3d11"
						case 0xd13ac4e8:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the DirectX 11 API."
							});
							break;

						// "-force-d3d11-singlethreaded"
						case 0x22d0099e:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the DirectX 11 API in singlethreading mode."
							});
							break;

						// "-force-d3d12"
						case 0x48339552:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the DirectX 12 API."
							});
							break;

						// "-force-device-index"
						case 0x1b17a1a6:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Unity to use a specific GPU."
							});
							break;

						// "-force-metal"
						case 0x810b258a:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the Metal API."
							});
							break;

						// "-force-glcore"
						case 0x5dac27a5:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL API."
							});
							break;

						// "-force-glcore32"
						case 0x0b1f8624:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v3.2 API."
							});
							break;

						// "-force-glcore33"
						case 0x7c18b6b2:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v3.3 API."
							});
							break;

						// "-force-glcore40"
						case 0xaa5071cf:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.0 API."
							});
							break;

						// "-force-glcore41"
						case 0xdd574159:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.1 API."
							});
							break;

						// "-force-glcore42"
						case 0x445e10e3:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.2 API."
							});
							break;

						// "-force-glcore43"
						case 0x33592075:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.3 API."
							});
							break;

						// "-force-glcore44"
						case 0xad3db5d6:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.4 API."
							});
							break;

						// "-force-glcore45"
						case 0xda3a8540:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the OpenGL v4.5 API."
							});
							break;

						// "-force-vulkan"
						case 0x518123f8:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced the use of the Vulkan API."
							});
							break;

						// "-force-clamped"
						case 0x193eda1a:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced a block on checks for OpenGL extensions."
							});
							break;

						// "-force-low-power-device"
						case 0xe8e5afe3:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Unity to assume a low powered device."
							});
							break;

						// "-force-wayland"
						case 0x200ddab3:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Unity to assume a low powered device."
							});
							break;

						// "-nographics"
						case 0xcf906abb:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Unity to not initialise a graphics device."
							});
							break;

						// "-nolog"
						case 0x3fc928e5:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced Unity to not write an output_log.txt."
							});
							break;

						// "-no-stereo-rendering"
						case 0xe1fe3057:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has force disabled stereo rendering."
							});
							break;

						// "-screen-height"
						case 0x5b279162:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced a specific screen height."
							});
							break;

						// "-screen-width"
						case 0x370462c7:
							LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
							{
								LogSeverity = LoggingHelper.LogSeverityValue.Warning,
								LogMessage = "User has forced a specific screen width."
							});
							break;

						default:
							break;
					}
				}
			}

			return true;
		}
	}
}
