// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System;
using Unity.Collections;
using Unity.Entities;

[Serializable]
public struct FormAndRecordIDs : IComponentData
{
	public ushort ModID;
	public uint FormID;
	public FixedString32 RecordID;
}
