#define DEBUG
// #undef DEBUG
#define DEBUG2
// #undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
	public class SpawnSystem : MonoBehaviour {
        // Call Initialize in Awake() or Start() of the inheriting class.
        // Override NextItem to define which item gets spawned next.
        // Override OnItemReused to define what happens when an item is reused.
        // Override OnItemSpawned to define what happens when an item is spawned.
        [SerializeField] protected GameObject[] goPrototypes;
		[SerializeField] protected int poolCount = 10;
		[SerializeField] protected string seed;
		[SerializeField] protected string poolName;
		[SerializeField] protected Transform tHolder;

		protected System.Random prng;
		protected GameObject[] pool;

		protected GameObject this[int i] {
			get {
				i = Mathf.Clamp(i, 0, this.poolCount);
				return this.pool[i];
			}
		}

		protected void Initialize() {
			if (string.IsNullOrEmpty(this.poolName))
				this.poolName = gameObject.name;
			if (string.IsNullOrEmpty(this.seed)) 
				this.seed = this.poolName + "_" + DateTime.Now.ToString();
			this.prng = new System.Random(this.seed.GetHashCode());
			if (this.tHolder == null) {
				GameObject goHolder = new GameObject(this.poolName + "_PoolHolder");
				this.tHolder = goHolder.transform;
			}
			InitializePool();

		}

		private void InitializePool() {
			this.pool = new GameObject[this.poolCount];
			for (int i = 0; i < this.poolCount; i++) {
				GameObject goItem = Instantiate(this.goPrototypes[this.prng.Next(0, this.goPrototypes.Length)]);
				goItem.SetActive(false);
				goItem.transform.SetParent(this.tHolder);
				goItem.name = this.poolName + "_" + i;
				this.pool[i] = goItem;
			}
		}

		public void SpawnAll() {
			for (int i = 0; i < this.poolCount; i++)
				HandleSpawn(i);
		}
		public void SpawnNext(int n) {
			n = Mathf.Clamp(n, 0, this.poolCount);
			HashSet<int> indices = new HashSet<int>();
			while (indices.Count < n) {
                int i = NextItem();
				if (indices.Add(i))
					HandleSpawn(i);
            }
		}
		public void SpawnNext() {
			int i = NextItem();
			HandleSpawn(i);
		}
		private void HandleSpawn(int i) { 
			// If the item is already activated - Inform about it.
			if (this[i].activeInHierarchy)
				OnItemReused(i);
			else { 
				// Activate and Inform about it, in any case.
				this[i].SetActive(true);
				OnItemSpawned(i);
			}
		}
		public void DeactivateAll() { 
			foreach (GameObject goItem in this.pool)
				goItem.SetActive(false);
		}

		///////////////////////////////////////////////////////////////////////
		// Override to define which item gets spawned next.
		protected virtual int NextItem() {
			int nextIndex = this.prng.Next(0, this.poolCount);
			#if DEBUG2
			Debug.Log("SpawnSystem: [" + this.poolName + "]: item selected: " + nextIndex);
			#endif
			return nextIndex;
		}
		// Override to handle item reuse.
		protected virtual void OnItemReused(int i) {
			#if DEBUG2
			Debug.Log("SpawnSystem: [" + this.poolName + "]: item reused: " + i);
			#endif
		}
		// Override to handle item spawn.
		protected virtual void OnItemSpawned(int i) {
			#if DEBUG2
			Debug.Log("SpawnSystem: [" + this.poolName + "]: item activated: " + i);
			#endif
		}

    }
}