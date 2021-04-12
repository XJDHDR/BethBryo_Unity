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
