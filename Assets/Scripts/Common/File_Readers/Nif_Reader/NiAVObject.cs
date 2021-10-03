// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

using System;
using System.Numerics;

namespace BethBryo_for_Unity_Common
{
	internal static class NiAVObject
	{
		internal static bool ReadNiAVObject(byte[] NifData, string NifLocation, FileNifStructs.NifHeaderData NifHeaderData, uint BlockNumber, ref int CurArrayPos)
		{
			// An NiAVObject's data is preceded by NiObjectNET data
			if (NiObjectNET.ReadNiObjectNET(NifData, NifLocation, NifHeaderData, BlockNumber, ref CurArrayPos) == false)
				return false;

			// There are some flags present in the form of a UInt if BS Version is more than 26
			if (NifHeaderData.BSVersion > 26)
			{
				// Don't need this so skip.
				CurArrayPos += 4;
			}

			// Next, Nif versions from 3.0.0.0 up and BS Versions less than or equal to 26 will have some flags present as a UShort.
			if (NifHeaderData.NifVersionCombined >= 0x03000000)		// If greater than or equal to 3.0.0.0
			{
				if ((NifHeaderData.BSVersion == null) || (NifHeaderData.BSVersion <= 26))
				{
					// Don't need this so skip.
					CurArrayPos += 2;
				}
			}

			// Next piece of data are 3 Floats representing the block's translation. Present on all versions.
			float _xPos = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			float _yPos = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			float _zPos = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;
			Vector3 _translation = new Vector3(_xPos, _yPos, _zPos);

			// Next is the block's rotation matrix: 9 floats in a 3x3 matrix.
			float[,] _rotation = new float[3, 3];
			for (byte _i = 0; _i < 9; ++_i)
			{
				_rotation[(_i / 3), (_i % 3)] = BitConverter.ToSingle(NifData, CurArrayPos);
				CurArrayPos += 4;
			}

			// Next is the block's scale as a float.
			float _scale = BitConverter.ToSingle(NifData, CurArrayPos);
			CurArrayPos += 4;

			// Next, versions up to 4.2.2.0 will have a Vector3 named "Velocity" with an unknown function. Skip over this if present.
			if (NifHeaderData.NifVersionCombined <= 0x04020200)		// If less than or equal to 4.2.2.0
			{
				CurArrayPos += 12;
			}

			// Next, Nifs with a BS Version up to 34 will have a number of refs to rendering properties
			if ((NifHeaderData.BSVersion == null) || (NifHeaderData.BSVersion <= 34))
			{
				// First, we have a uint with the number of refs then the refs themselves. We don't need this so skip over it.
				CurArrayPos += BitConverter.ToUInt16(NifData, CurArrayPos) * 4 + 4;
			}

			// Next, Nif versions up to 2.3.0.0 will have a UInt then a byte with unknown purposes. Skip over them.
			if (NifHeaderData.NifVersionCombined <= 0x04020200)		// If less than or equal to 4.2.2.0
			{
				CurArrayPos += 5;
			}

			// Next, Nif versions between 3.0.0.0 and 4.2.2.0 will optionally have a Bounding Volume.
			if ((NifHeaderData.NifVersionCombined >= 0x03000000) && (NifHeaderData.NifVersionCombined <= 0x04020200))
			{
				if (FileNifCommonMethods.ReadBoolean(NifData, NifLocation, NifHeaderData.NifVersionCombined, ref CurArrayPos, out bool _isBoundingVolumePresent) == false)
				{
					return false;
				}

				if (_isBoundingVolumePresent)
				{
					FileNifCommonMethods.ReadBoundingVolume(NifData, NifLocation, NifHeaderData.NifVersionCombined, NifHeaderData.UserVersion, ref CurArrayPos, out uint BoundingVolume);
				}
			}

			// Finally, Nif versions of 10.0.1.0 or later will have a reference to an NiCollisionObject
			if (NifHeaderData.NifVersionCombined >= 0x0A000100)
			{
				FileNifCommonMethods.ReadNiCollisionObject(NifData, ref CurArrayPos, out uint _objectIndex);
			}

			return true;
		}
	}
}
