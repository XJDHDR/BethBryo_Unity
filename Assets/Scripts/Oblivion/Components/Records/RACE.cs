using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct RACE : IComponentData
	{
		// Header
		private ushort _modID;
		public uint FormID;
		public FixedString32 RecordID;

		public FixedString32 Name;
		public FixedString512 Description;

		public FixedList32<SPEL> DefaultSpells;
		public FixedList32<SkillBoosts> SkillBoosts;

		public float MaleHeight;
		public float MaleWeight;
		public float FemaleHeight;
		public float FemaleWeight;

		public bool IsPlayable;

		public HAIR DefaultHair;
		public EYES DefaultEyes;
		public byte DefaultHairColour;      // Probably should decode this single value into RGB values.

		public FixedList32<NpcAttribute> MaleBaseAttributes;
		public FixedList32<NpcAttribute> FemaleBaseAttributes;
		
		// Face data
		// public RenderMesh FemaleHeadModel;
		// public RenderMesh FemaleEarsModel;
		// public RenderMesh FemaleMouthModel;
		// public RenderMesh FemaleTeethUpperModel;
		// public RenderMesh FemaleTeethLowerModel;
		// public RenderMesh FemaleTongueModel;
		// public RenderMesh FemaleEyeLeftModel;
		// public RenderMesh FemaleEyeRightModel;
		// public RenderMesh MaleHeadModel;
		// public RenderMesh MaleEarsModel;
		// public RenderMesh MaleMouthModel;
		// public RenderMesh MaleTeethUpperModel;
		// public RenderMesh MaleTeethLowerModel;
		// public RenderMesh MaleTongueModel;
		// public RenderMesh MaleEyeLeftModel;
		// public RenderMesh MaleEyeRightModel;

		// public Material FemaleHeadTexture;
		// public Material FemaleEarsTexture;
		// public Material FemaleMouthTexture;
		// public Material FemaleTeethUpperTexture;
		// public Material FemaleTeethLowerTexture;
		// public Material FemaleTongueTexture;
		// public Material FemaleEyeLeftTexture;
		// public Material FemaleEyeRightTexture;
		// public Material MaleHeadTexture;
		// public Material MaleEarsTexture;
		// public Material MaleMouthTexture;
		// public Material MaleTeethUpperTexture;
		// public Material MaleTeethLowerTexture;
		// public Material MaleTongueTexture;
		// public Material MaleEyeLeftTexture;
		// public Material MaleEyeRightTexture;

		// Body data
		// public RenderMesh FemaleUpperBodyModel;
		// public RenderMesh FemaleLowerBodyModel;
		// public RenderMesh FemaleHandModel;
		// public RenderMesh FemaleFootModel;
		// public RenderMesh FemaleTailModel;
		// public RenderMesh MaleUpperBodyModel;
		// public RenderMesh MaleLowerBodyModel;
		// public RenderMesh MaleHandModel;
		// public RenderMesh MaleFootModel;
		// public RenderMesh MaleTailModel;

		// public Material FemaleUpperBodyTexture;
		// public Material FemaleLowerBodyTexture;
		// public Material FemaleHandTexture;
		// public Material FemaleFootTexture;
		// public Material FemaleTailTexture;
		// public Material MaleUpperBodyTexture;
		// public Material MaleLowerBodyTexture;
		// public Material MaleHandTexture;
		// public Material MaleFootTexture;
		// public Material MaleTailTexture;

		public FixedList64<HAIR> Hairs;
		public FixedList32<EYES> Eyes;

		// Facegen Data
		public FixedBytes510 DefaultFaceGenGeomSymmetric;       // Should decode these bytes into individual vars
		public FixedBytes126 DefaultFaceGenGeomAsymmetric;
		public FixedBytes510 DefaultFaceGenTexture;

		public ushort ModID { get => _modID; }
	}

	[Serializable]
	public struct SkillBoosts
	{
		public SKIL Skill;
		public byte Amount;
	}
}
