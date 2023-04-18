using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Scenes {
	public class PersistentObject : MonoBehaviour {
		private void Awake() { 
			DontDestroyOnLoad(gameObject);
		}
	}
}