using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static void DestroyChildren(this GameObject goItem){
			if (goItem == null) return;
			for(int i = goItem.transform.childCount-1; i>=0; i--) {
                if (Application.isPlaying)
                    GameObject.Destroy(goItem.transform.GetChild(i).gameObject);
                else
                    GameObject.DestroyImmediate(goItem.transform.GetChild(i).gameObject);
			}
		}
		public static void SetActiveChildren(this GameObject goItem, bool targetState){
			if (goItem == null) return;
			for(int i = goItem.transform.childCount-1; i>=0; i--) {
                goItem.transform.GetChild(i).gameObject.SetActive(targetState);
			}
		}
		public static GameObject TryFindOrAddByName(this GameObject goItem, string name) {
            GameObject goFound = null;
            Transform tFound = goItem.transform.Find(name);
            if (tFound == null){
                goFound = new GameObject();
                goFound.name = name;
                goFound.transform.SetParent(goItem.transform);
            } else {
                goFound = tFound.gameObject;
            }
            return goFound;
        }
		public static GameObject TryFindOrAddByName(string name) {
            GameObject goFound = GameObject.Find(name);
            if (goFound == null){
                goFound = new GameObject();
                goFound.name = name;
            }
            return goFound;
        }
        public static T TryGetOrAddComponent<T>(this GameObject gItem) 
            where T : MonoBehaviour { 
            T tFound = gItem.GetComponent<T>();
            if (tFound == null) {
                tFound = gItem.AddComponent<T>();
            }
            return tFound;
        }

        public static IEnumerator InvokeNextFrame(Action callback) {
            yield return null;
            callback();
        }
        public static IEnumerator InvokeNextFrame<T>(Action<T> callback, T value) {
            yield return null;
            callback(value);
        }
    }
}