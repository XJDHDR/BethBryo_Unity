// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System;
using Unity.Collections;
using Unity.Entities;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct NpcAttribute : IComponentData
	{
		public FixedString32 Name;
		public byte Value;
	}
}
