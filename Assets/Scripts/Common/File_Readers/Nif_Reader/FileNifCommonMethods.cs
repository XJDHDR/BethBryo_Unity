// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;
using System.Numerics;

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Provides a number of methods used to read common data arrangements in Nif files.
	/// </summary>
	internal static class FileNifCommonMethods
	{
		/// <summary>
		/// Reads a Boolean from the Nif data array.
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="NifLocation">The path to the Nif data being read.</param>
		/// <param name="NifVersion">The Combined Version number read from the Nif's header.</param>
		/// <param name="CurArrayPos">The location of the array reading pointer.</param>
		/// <param name="Boolean">The boolean that was read from the Nif data.</param>
		internal static bool ReadBoolean(byte[] NifData, string NifLocation, uint NifVersion, ref int CurArrayPos, out bool Boolean)
		{
			uint _readBytes;
			int _startingPosition = CurArrayPos;
			if (NifVersion >= 0x04010001)       // If greter than or equal to version 4.1.0.1
			{
				// In this case, the bool is stored as a 32-bit int.
				_readBytes = BitConverter.ToUInt32(NifData, CurArrayPos);
				CurArrayPos += 4;
			}
			else
			{
				// In this case, the bool is stored as an 8-bit int.
				_readBytes = NifData[CurArrayPos];
				CurArrayPos += 1;
			}

			switch (_readBytes)
			{
				case 0:
					Boolean = false;
					break;

				case 1:
					Boolean = true;
					break;

				default:
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


		internal static bool ReadBoundingVolume(byte[] NifData, string NifLocation, uint NifVersion, uint? UserVersion, ref int CurArrayPos, out uint BoundingVolume)
		{
			BoundingVolume = BitConverter.ToUInt16(NifData, CurArrayPos);
			CurArrayPos += 4;

			switch (BoundingVolume)
			{
				case 0:     // Sphere
					ReadSphereBound(NifData, NifVersion, UserVersion, ref CurArrayPos, out Vector3 _sphereCentre, out float _sphereRadius);
					break;

				case 1:     // Box
					Vector3 _boxCentre = new Vector3();
					_boxCentre.X = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_boxCentre.Y = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_boxCentre.Z = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					Vector3[] _boxAxis = new Vector3[3];
					for (byte _i = 0; _i < _boxAxis.Length; _i++)
					{
						_boxAxis[_i].X = BitConverter.ToSingle(NifData, CurArrayPos);
						CurArrayPos += 4;
						_boxAxis[_i].Y = BitConverter.ToSingle(NifData, CurArrayPos);
						CurArrayPos += 4;
						_boxAxis[_i].Z = BitConverter.ToSingle(NifData, CurArrayPos);
						CurArrayPos += 4;
					}

					Vector3 _boxExtents = new Vector3();
					_boxExtents.X = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_boxExtents.Y = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_boxExtents.Z = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					break;

				case 2:     // Capsule
					Vector3 _capsuleCentre = new Vector3();
					_capsuleCentre.X = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_capsuleCentre.Y = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_capsuleCentre.Z = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					Vector3 _capsuleOrigin = new Vector3();
					_capsuleOrigin.X = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_capsuleOrigin.Y = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_capsuleOrigin.Z = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					float _capsuleExtent = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					float _capsuleRadius = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					break;

				// This is commented out in the Nif XML. If this ever gets uncommented, a struct of all the essential data
				// from this Bounding Volume data fetching method is required.
				/*
				case 4:		// Union
					uint _numBoundingVolumes = BitConverter.ToUInt32(NifData, CurArrayPos);
					for (uint _i = 0; _i < _numBoundingVolumes; _i++)
					{
						ReadBoundingVolume(NifData, NifVersion, UserVersion, ref CurArrayPos, out BoundingVolume);
					}
					break;
				*/

				case 5:     // Half space
					ReadNiPlane(NifData, ref CurArrayPos, out Vector3 _halfSpaceNormal, out float _halfSpaceConstant);

					Vector3 _halfSpaceCentre = new Vector3();
					_halfSpaceCentre.X = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_halfSpaceCentre.Y = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;
					_halfSpaceCentre.Z = BitConverter.ToSingle(NifData, CurArrayPos);
					CurArrayPos += 4;

					break;

				case 0xffffffff:	// Nothing
					break;

				default:
					LoggingHelper.LogQueue.Push(new LoggingHelper.LoggingData
					{
						LogSeverity = LoggingHelper.LogSeverityValue.Error,
						LogMessage = $"Error while reading Nif file: {NifLocation}\n" +
							$"While reading a Bounding Volume at {CurArrayPos}, the value read ({BoundingVolume}) was not a valid BV ID number.\n" +
							"This could indicate a corrupt file."
					});
					return false;
			}

			return true;
		}

		internal static void ReadNiCollisionObject(byte[] NifData, ref int CurArrayPos, out uint ObjectIndex)
		{
			ObjectIndex = BitConverter.ToUInt32(NifData, CurArrayPos);
			CurArrayPos += 4;
		}

		internal static void ReadNiPlane(byte[] NifData, ref int CurArrayPos, out Vector3 Normal, out float Constant)
		{
			// Read the coordinates of the plane's center
			Normal.X = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			Normal.Y = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			Normal.Z = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;

			// Read the circle's radius
			Constant = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
		}

		/// <summary>
		/// Reads the info for a Bounding Sphere.
		/// </summary>
		/// <param name="NifData">Byte array which holds the contents of the Nif file's bytes.</param>
		/// <param name="NifVersion">The Combined Version number read from the Nif's header.</param>
		/// <param name="CurArrayPos">The location of the array reading pointer.</param>
		/// <param name="Centre">Circle's centre point that was read from the Nif data.</param>
		/// <param name="Radius">Radius of the circle that was read from the Nif data.</param>
		// Called an "NiBound" in the Nif xml
		internal static void ReadSphereBound(byte[] NifData, uint NifVersion, uint? UserVersion, ref int CurArrayPos, out Vector3 Centre, out float Radius)
		{
			// Read the coordinates of the circle's center
			Centre.X = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			Centre.Y = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			Centre.Z = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;

			// Read the circle's radius
			Radius = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;

			// Finally, a Divinity 2 mesh has some extra data that must be skipped over if present.
			if (NifVersion == 0x14030009)       // If equal to 20.3.0.9
			{
				if ((UserVersion == 0x20000) || (UserVersion == 0x30000))
				{
					// First, a UShort with the number of corners must be skipped.
					CurArrayPos += 2;

					// Then there are two arrays of Vector3s indicating the two corners that must also be skipped.
					CurArrayPos += 24;
				}
			}
		}
	}
}
