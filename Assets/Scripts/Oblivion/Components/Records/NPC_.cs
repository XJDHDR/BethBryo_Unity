using System;
using Unity.Collections;
using Unity.Entities;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct NPC_ : IComponentData
	{
		// Header
		public ushort ModID;
		public uint FormID;
		public FixedString32 RecordID;

		// Basics
		public FixedString32 FullName;
		public FixedString32 SkeletonPath;
		public float ModelBoundRadius;

		// Config data
		public bool IsFemale;
		public bool IsEsential;
		public bool Respawns;
		public bool AutoCalcStats;
		public bool LevelIsPcOffset;
		public bool NoLowLevelProcessing;
		public bool NoRumors;
		public bool Sumonable;
		public bool NoPersuasion;
		public bool CanCorpseCheck;
		public ushort BaseSpellPoints;
		public ushort BaseFatigue;
		public ushort BarterGold;
		public short LevelOrOffset;
		public ushort MinLevel;
		public ushort MaxLevel;

		// Faction data
		public NpcFactionData FactionData;

		// Death Item
		public ushort DeathItemModID;
		public uint DeathItemFormID;

		// Race and attachments
		public RACE NpcRace;
		public FixedList128<SPEL> KnownSpells;
		public FixedList32<SCPT> Scripts;

		// Inventory
		public FixedList512<InventoryContents> InventoryContents;
		public ulong Money;

		// AI Data
		public byte Aggression;
		public byte Confidence;
		public byte Energylevel;
		public byte Responsibility;

		// Services
		public bool IsWeaponMerchant;
		public bool IsArmorMerchant;
		public bool IsClothingMerchant;
		public bool IsBookMerchant;
		public bool IsIngredientMerchant;
		public bool IsLightsMerchant;
		public bool IsApparatusMerchant;
		public bool IsMiscMerchant;
		public bool IsSpellMerchant;
		public bool IsMagicItemMerchant;
		public bool IsPotionMerchant;
		public bool OffersTraining;
		public bool OffersRecharge;
		public bool OffersRepairs;
		public TrainingData TrainingData;

		public FixedList32<PACK> AIPackages;

		public CLAS Class;

		// Stats
		public FixedList32<SKIL> Skills;
		public ushort Health;
		public FixedList32<NpcAttribute> Attributes;

		public HairData HairData;
		public EYES Eyes;

		public CSTY CombatStyle;

		// Facegen Data
		public FixedBytes510 FaceGenGeomSymmetric;		// Should decode these bytes into individual vars
		public FixedBytes126 FaceGenGeomAsymmetric;
		public FixedBytes510 FaceGenTexture;
	}

	[Serializable]
	public struct NpcFactionData
	{
		public FACT AssignedFaction;
		public byte Rank;
	}

	[Serializable]
	public struct InventoryContents
	{
		public InventoryItemData ItemData;
		public int Count;
	}

	[Serializable]
	public struct TrainingData
	{
		public SKIL SkillTrained;
		public byte Level;
	}

	[Serializable]
	public struct HairData
	{
		public HAIR Hair;
		public float Length;
		public byte LightR;
		public byte LightG;
		public byte LightB;
	}
}
