using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static T GetItem<T>(this List<T> list, int index){
			index = Mathf.Clamp(index,
			                    0, list.Count);
			return list[index];
		}
    }
}