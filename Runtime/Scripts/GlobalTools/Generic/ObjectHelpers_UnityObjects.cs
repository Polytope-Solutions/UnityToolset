using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static void DestroyChildren(this GameObject goItem){
			if (goItem == null) return;
			for(int i = goItem.transform.childCount-1; i>=0; i--) {
				#if UNITY_EDITOR
				GameObject.DestroyImmediate(goItem.transform.GetChild(i).gameObject);
				#else
				GameObject.Destroy(goItem.transform.GetChild(i).gameObject);
				#endif
			}
		}
    }
}