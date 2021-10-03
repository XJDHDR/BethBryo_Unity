// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	/// <summary>
	/// Class used to hold methods for reading node types that don't have any info other than what is inherited from another node type.
	/// </summary>
	internal static class FileNifStubMethods
	{
		/// <summary>
		/// A shape node that refers to singular triangle data.
		/// </summary>
		internal static bool ReadNiTriBasedGeom()
		{
			// ReadNiGeometry()
			return true;
		}


		/// <summary>
		/// A shape node that refers to singular triangle data.
		/// </summary>
		internal static bool ReadNiTriShape()
		{
			ReadNiTriBasedGeom();
			return true;
		}

		/// <summary>
		/// A shape node that refers to data organized into strips of triangles.
		/// </summary>
		internal static bool ReadNiTriStrips()
		{
			ReadNiTriBasedGeom();
			return true;
		}

	}
}
