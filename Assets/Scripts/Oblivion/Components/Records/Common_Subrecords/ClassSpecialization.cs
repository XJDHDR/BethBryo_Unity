// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

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
