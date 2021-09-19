// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was written mainly according to the specifications provided by the Nif XML project:
// https://github.com/niftools/nifxml/

namespace BethBryo_for_Unity_Common
{
	internal enum BSLightingShaderType : uint
	{
		Default					=  0,
		Environment_Map			=  1,	// Enables EnvMap Mask(TS6), EnvMap Scale
		Glow_Shader				=  2,	// Enables Glow(TS3)
		Parallax				=  3,	// Enables Height(TS4)
		Face_Tint				=  4,	// Enables Detail(TS4), Tint(TS7)
		Skin_Tint				=  5,	// Enables Skin Tint Color
		Hair_Tint				=  6,	// Enables Hair Tint Color
		Parallax_Occ			=  7,	// Enables Height(TS4), Max Passes, Scale. Unimplemented.
		Multitex_Landscape		=  8,
		LOD_Landscape			=  9,
		Snow					= 10,
		MultiLayer_Parallax		= 11,	// Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Para Refraction Scale, Para Inner Layer U & V Scale, EnvMap Scale
		Tree_Anim				= 12,
		LOD_Objects				= 13,
		Sparkle_Snow			= 14,   // Enables SparkleParams
		LOD_Objects_HD			= 15,
		Eye_Envmap				= 16,   // Enables EnvMap Mask(TS6), Eye EnvMap Scale
		Cloud					= 17,
		LOD_Landscape_Noise		= 18,
		Multitex_Land_LOD_Blend	= 19,
		FO4_Dismemberment		= 20
	}
}
