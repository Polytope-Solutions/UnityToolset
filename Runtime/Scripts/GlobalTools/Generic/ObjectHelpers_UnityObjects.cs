using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

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
		public static GameObject TryFindOrAddByName(this GameObject goItem, string name, GameObject goPrefab=null, Type[] components=null) {
            GameObject goFound = null;
            Transform tFound = goItem.transform.Find(name);
            if (tFound == null){
                goFound = (goPrefab == null) ? new GameObject() : GameObject.Instantiate(goPrefab);
                goFound.name = name;
                goFound.transform.SetParent(goItem.transform);
            } else {
                goFound = tFound.gameObject;
            }
            if (components != null)
                foreach (Type component in components) 
                    goFound.TryGetOrAddComponent(component);
            return goFound;
        }
		public static GameObject TryFindOrAddByName(string name, GameObject goPrefab = null, Type[] components = null) {
            GameObject goFound = GameObject.Find(name);
            if (goFound == null){
                goFound = (goPrefab == null) ? new GameObject() : GameObject.Instantiate(goPrefab);
                goFound.name = name;
            }
            if (components != null)
                foreach (Type component in components)
                    goFound.TryGetOrAddComponent(component);
            return goFound;
        }
        public static T TryGetOrAddComponent<T>(this GameObject gItem) 
            where T : MonoBehaviour { 
            return (T)gItem.TryGetOrAddComponent(typeof(T));
        }
        public static Component TryGetOrAddComponent(this GameObject gItem, Type componentType){
            Component tFound = gItem.GetComponent(componentType);
            if (tFound == null) {
                tFound = gItem.AddComponent(componentType);
            }
            return tFound;
        }
        public static T CopyComponent<T>(this GameObject goTarget, T source) where T: Component {
            Type type = source.GetType();
            Component copy = goTarget.AddComponent(type);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields) {
                field.SetValue(copy, field.GetValue(source));
            }
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties) {
                if (property.CanWrite) {
                    property.SetValue(copy, property.GetValue(source, null), null);
                }
            }
            return (T)copy;
        }
        public static bool IsInLayerMask(this GameObject goItem, LayerMask layerMask) {
            return (layerMask & (1 << goItem.layer)) != 0;
        }

        public static IEnumerator InvokeNextFrame(Action callback) {
            yield return null;
            callback();
        }
        public static IEnumerator InvokeNextFrame<T>(Action<T> callback, T value) {
            yield return null;
            callback(value);
        }

        [HideInCallstack]
        public static void Log(Type source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.Log(ConditionMessage("STATIC", source.Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogWarning(Type source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.LogWarning(ConditionMessage("STATIC", source.Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogError(Type source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.LogError(ConditionMessage("STATIC", source.Name, methodName, messageContent, includeTimeStamp));
        }

        [HideInCallstack]
        public static void Log(this object source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            string objectName = (source is MonoBehaviour) ? ((MonoBehaviour)source).gameObject.name : "GENERIC";
            Debug.Log(ConditionMessage(objectName, source.GetType().Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogWarning(this object source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            string objectName = (source is MonoBehaviour) ? ((MonoBehaviour)source).gameObject.name : "GENERIC";
            Debug.LogWarning(ConditionMessage(objectName, source.GetType().Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogError(this object source, string messageContent,
                [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            string objectName = (source is MonoBehaviour) ? ((MonoBehaviour)source).gameObject.name : "GENERIC";
            Debug.LogError(ConditionMessage(objectName, source.GetType().Name, methodName, messageContent, includeTimeStamp));
        }

        [HideInCallstack]
        public static void Log(this GameObject goItem, string messageContent,
                Type type = null, [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.Log(ConditionMessage(goItem.name, type.Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogWarning(this GameObject goItem, string messageContent,
                Type type = null, [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.LogWarning(ConditionMessage(goItem.name, type.Name, methodName, messageContent, includeTimeStamp));
        }
        [HideInCallstack]
        public static void LogError(this GameObject goItem, string messageContent,
                Type type = null, [CallerMemberName] string methodName = null, bool includeTimeStamp = false) {
            Debug.LogError(ConditionMessage(goItem.name, type.Name, methodName, messageContent, includeTimeStamp));
        }

        private static string ConditionMessage(string sourceObjectName, string typeName, string methodName, 
                string messageContent, bool includeTimeStamp) {
            string message = (includeTimeStamp) ? $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] " : string.Empty;
            message += "[";
            message += string.Join(">", sourceObjectName, typeName, methodName);
            message += "]";
            message += $"\n\t{messageContent}\n";
            return message;
        }
    }
}