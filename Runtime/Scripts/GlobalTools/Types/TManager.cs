using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
	public class TManager<TItem> : MonoBehaviour
		                         where TItem : MonoBehaviour {
		private static TItem _instance;
		public static TItem instance {
			get {
				if (_instance == null) {
					// Find It
					_instance = FindObjectOfType<TItem>();
				}
				return _instance;
			}
		}
	}
}