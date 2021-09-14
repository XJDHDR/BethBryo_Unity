using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	internal class BsaFileFilterAndPatching
	{
		internal BsaFileFilterAndPatching(SupportedGames SelectedGame, ref Dictionary<string, byte> PreExtractNames, ref Dictionary<string, ulong> PostExtractPatches)
		{
			switch (SelectedGame)
			{
				case SupportedGames.Oblivion:
					_preparePreExtractFilterOblivion(PreExtractNames);
					_preparePostExtractFilterOblivion(PostExtractPatches);
					break;
			}
		}

		internal bool PreExtractNameFilter(Dictionary<string, byte> PreExtractNames, string FileName, string BsaPath)
		{
			if (FileName.EndsWith(".txt"))
			{
				return true;		// None of the TXT files contained in Oblivion's BSAs have any relevant info.
			}

			if (BsaPath.EndsWith("/Oblivion - Textures - Compressed.bsa"))
			{
				if (PreExtractNames.ContainsKey(FileName))
				{
					return true;
				}
			}

			return false;
		}

		internal void PostExtractFilePatching(Dictionary<string, ulong> PostExtractPatches, string BsaPath, string FileName, ref byte[] FileData)
		{
			if (BsaPath.EndsWith("/Oblivion - Textures - Compressed.bsa"))
			{
				if (PostExtractPatches.TryGetValue(FileName, out ulong _indexNumber))
				{
					switch (_indexNumber)
					{
						case 0xFFFF000000000000:    // anvilwindow02_n.dds, anvilwindowhaunted01_n.dds
							Array.Resize(ref FileData, FileData.Length + 16);	// Need 16 extra bytes to store the new DXT5 mipmap
							FileData[28] = 0x0A;                                // Change the dwMipMapCount byte to reflect the new count
							FileData[FileData.Length - 16] = 0xFF;              // Add the new mipmaps
							FileData[FileData.Length - 15] = 0xFF;
							FileData[FileData.Length - 14] = 0x00;
							FileData[FileData.Length - 13] = 0x00;
							FileData[FileData.Length - 12] = 0x00;
							FileData[FileData.Length - 11] = 0x00;
							FileData[FileData.Length - 10] = 0x00;
							FileData[FileData.Length - 9]  = 0x00;
							FileData[FileData.Length - 8]  = 0x1F;
							FileData[FileData.Length - 7]  = 0x84;
							FileData[FileData.Length - 6]  = 0x1F;
							FileData[FileData.Length - 5]  = 0x84;
							FileData[FileData.Length - 4]  = 0x00;
							FileData[FileData.Length - 3]  = 0x00;
							FileData[FileData.Length - 2]  = 0x00;
							FileData[FileData.Length - 1]  = 0x00;
							break;

						case 0xF29C4F8C55005500:    // lcwood01.dds
							Array.Resize(ref FileData, FileData.Length + 16);   // Need 16 extra bytes to store the new DXT1 mipmaps
							FileData[28] = 0x0B;                                // Change the dwMipMapCount byte to reflect the new count
							FileData[FileData.Length - 16] = 0xF2;              // Add the new mipmaps
							FileData[FileData.Length - 15] = 0x9C;
							FileData[FileData.Length - 14] = 0x4F;
							FileData[FileData.Length - 13] = 0x8C;
							FileData[FileData.Length - 12] = 0x55;
							FileData[FileData.Length - 11] = 0x00;
							FileData[FileData.Length - 10] = 0x55;
							FileData[FileData.Length - 9]  = 0x00;
							FileData[FileData.Length - 8]  = 0x91;
							FileData[FileData.Length - 7]  = 0x94;
							FileData[FileData.Length - 6]  = 0x90;
							FileData[FileData.Length - 5]  = 0x8C;
							FileData[FileData.Length - 4]  = 0xAA;
							FileData[FileData.Length - 3]  = 0xAA;
							FileData[FileData.Length - 2]  = 0xAA;
							FileData[FileData.Length - 1]  = 0xAA;
							break;

						case 0x00001F84F5F5FFFF:    // lcwoodpost01_n.dds
							Array.Resize(ref FileData, FileData.Length + 16);   // Need 16 extra bytes to store the new DXT1 mipmaps
							FileData[28] = 0x0A;                                // Change the dwMipMapCount byte to reflect the new count
							FileData[FileData.Length - 16] = 0x00;              // Add the new mipmaps
							FileData[FileData.Length - 15] = 0x00;
							FileData[FileData.Length - 14] = 0x1F;
							FileData[FileData.Length - 13] = 0x84;
							FileData[FileData.Length - 12] = 0xF5;
							FileData[FileData.Length - 11] = 0xF5;
							FileData[FileData.Length - 10] = 0xFF;
							FileData[FileData.Length - 9]  = 0xFF;
							FileData[FileData.Length - 8]  = 0x00;
							FileData[FileData.Length - 7]  = 0x00;
							FileData[FileData.Length - 6]  = 0x1F;
							FileData[FileData.Length - 5]  = 0x84;
							FileData[FileData.Length - 4]  = 0xFD;
							FileData[FileData.Length - 3]  = 0xFF;
							FileData[FileData.Length - 2]  = 0xFF;
							FileData[FileData.Length - 1]  = 0xFF;
							break;

					}
				}
			}
		}

		private void _preparePreExtractFilterOblivion(Dictionary<string, byte> _preExtractNames)
		{
			// textures/architecture/anvil/
			_preExtractNames.Add("arcanesymbol01_g.dds",	0);     // File data encoded as a DDPF_LUMINANCE type, which Unity doesn't support.
			_preExtractNames.Add("arcanesymbol02_g.dds",	1);     // File data encoded as a DDPF_LUMINANCE type, which Unity doesn't support.
			_preExtractNames.Add("handmagic01_g.dds",		2);     // File data encoded as a DDPF_LUMINANCE type, which Unity doesn't support.
		}

		private void _preparePostExtractFilterOblivion(Dictionary<string, ulong> _postExtractPatches)
		{
			// textures/architecture/anvil/
			_postExtractPatches.Add("anvilwindow02_n.dds",			0xFFFF000000000000);    // Unity expects 10 mipmaps for this texture whereas it only has 9
			_postExtractPatches.Add("anvilwindowhaunted01_n.dds",	0xFFFF000000000000);    // Unity expects 10 mipmaps for this texture whereas it only has 9
			_postExtractPatches.Add("lcwood01.dds",					0xF29C4F8C55005500);    // Unity expects 11 mipmaps for this texture whereas it only has 9
			_postExtractPatches.Add("lcwoodpost01_n.dds",			0x00001F84F5F5FFFF);    // Unity expects 10 mipmaps for this texture whereas it only has 8
		}
		
		/*
		// textures/architecture/anvil/
		anvilwindow01.dds
		lcwoodpost01.dds
		coppertrim01.dds
		lorgenhand_g.dds
		anvilmcdoor01.dds
		anvilstonecarving01_n.dds
		anvilstonetrimuc01.dds
		tileroofnew_n.dds
		anvilwindowarch01.dds
		anvilstackedstone01_n.dds
		anvilwindow01_n.dds
		lorgenhand.dds
		anvilconcrete01.dds
		anvilstonecarving01_a.dds
		anvilstonetrim04_n.dds
		anvilstonetrimuc02_n.dds
		anvilconcrete01_a.dds
		vines02_n.dds
		lorgenhand_n.dds
		anvilconcrete01_n.dds
		anvilcastledoor01_n.dds
		anvilstackedstone01.dds
		anvilcastledoor01.dds
		tatteredrobe01.dds
		anvilstonecarving01.dds
		anvilstonetrim04.dds
		anvilstonetrim01.dds
		anvilstonetrimuc01_n.dds
		anvilstonetrim01_n.dds
		lcwood01_n.dds
		anvilstonetrimuc02.dds
		anvilstackedstonebarn01_n.dds
		tatteredrobe01_n.dds
		anvilmcdoor01_n.dds
		arcanesymbol02.dds
		anvilstonetrim02.dds
		anvilwindowhaunted01.dds
		coppertrim01_n.dds
		vines02.dds
		copperpost.dds
		anvilwindow02.dds
		arcanesymbol02_n.dds
		anvilstackedstonebarn01.dds
		anvilwindowarch01_n.dds
		anvilstonetrim02_n.dds
		copperpost_n.dds
		tileroofnew.dds

		// textures/architecture/anvil/anvilinterior/
		anvilwindow02interior_g.dds
		ucwoodfloortrim01_n.dds
		tilefloortrim01_n.dds
		anvilwindow01interiorhaunted_g.dds
		anvilwindow02interior_n.dds
		anvilwindow02interior.dds
		anvilwindow01interior.dds
		anvilwindow01interiorhaunted_n.dds
		ucwoodtrim01_n.dds
		anvilwindow01interior_n.dds
		ucwoodtrim01.dds
		anvilwindowarch01interior_n.dds
		anvilwindowarch01interior.dds
		anvilwindow01interiorhaunted.dds
		anvilwindowarch01interior_g.dds
		tilefloortrim01.dds
		ucwoodfloortrim01.dds
		anvilwindow01interior_g.dds
		*/
	}
}
