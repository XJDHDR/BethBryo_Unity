// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System;
using Unity.Entities;

namespace BethBryo_for_Unity_Oblivion
{
	[Serializable]
	public struct TES4Header : IComponentData
	{
		// Add fields to your component here. Remember that:
		//
		// * A component itself is for storing data and doesn't 'do' anything.
		//
		// * To act on the data, you will need a System.
		//
		// * Data in a component must be blittable, which means a component can
		//   only contain fields which are primitive types or other blittable
		//   structs; they cannot contain references to classes.
		//
		// * You should focus on the data structure that makes the most sense
		//   for runtime use here. Authoring Components will be used for 
		//   authoring the data in the Editor.


	}
}
