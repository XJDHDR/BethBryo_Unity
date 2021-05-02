// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using BethBryo_for_Unity_Common;
using UnityEngine;

namespace BethBryo_for_Unity_Oblivion
{
	public class ExtractOblivionsResourceFiles : MonoBehaviour
	{
		public void ExtractOblivionTextureBsa()
		{
			string _pathToDataFiles = "D:/Games/Bethesda/Oblivion/Mod Organiser 2/mods/Original Base Oblivion Data Files/";
			BsaHeaderAndRecordExtraction.ExtractBsaContents(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", BSAContentType.Textures, SupportedGames.Oblivion);

			//BsaFileDataExtraction.ExtractBsaFileDataToFiles(_pathToDataFiles + "Oblivion - Textures - Compressed.bsa", BsaFileExtractParams[] PassedBsaFileExtractParams, out bool ErrorsOccurred)
		}
	}
}
