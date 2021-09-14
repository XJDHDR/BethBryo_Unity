// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System;
using Unity.Entities;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct OblivionRecordHeader : IComponentData
	{
		public ushort ModID;
		public ulong FormID;
		public RecordType RecordType;
	}
}
