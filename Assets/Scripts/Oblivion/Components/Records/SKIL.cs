using System;
using Unity.Collections;
using Unity.Entities;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct SKIL : IComponentData
	{
		// Header
		public ushort ModID;
		public uint FormID;
		public FixedString32 RecordID;

		// Menu Data
		public FixedString32 SkillName;
		public FixedString32 Description;
		public FixedString32 IconTexturePath;

		// Skill Data
		public ulong TrainingAction;
		public NpcAttribute AssociatedAttribute;
		public ClassSpecialization AssociatedSpecialization;

		public float FirstActionIncrementAmount;
		public float SecondActionIncrementAmount;

		// Levelup flavour text
		public FixedString32 ApprenticeText;
		public FixedString32 JourneymanText;
		public FixedString32 ExpertText;
		public FixedString32 MasterText;
	}
}
