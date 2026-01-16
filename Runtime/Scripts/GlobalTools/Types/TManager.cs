using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
	public abstract class TManager<TItem> : MonoBehaviour
											where TItem : MonoBehaviour {
		// NB! if overriding Awake - call base.Awake() in derived classes.
		private static TItem _instance;
		public static TItem Instance {
			get {
				if (_instance == null) {
					// Find It
					_instance = FindFirstObjectByType<TItem>();
				}
				return _instance;
			}
		}
		protected virtual void Awake() {
			if (!Instance)
                this.LogWarning("No instance of found in scene.");
        }
        protected virtual void OnDestroy() {
			if (!Instance)
				_instance = null;
        }
    }
}