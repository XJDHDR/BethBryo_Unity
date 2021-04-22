using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct ClassSpecialization : IComponentData
	{
		public FixedString32 Name;
	}
}
