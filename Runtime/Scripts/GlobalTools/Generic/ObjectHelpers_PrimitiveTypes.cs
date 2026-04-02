using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Globalization;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
	public static partial class ObjectHelpers {
		public static T GetItem<T>(this List<T> list, int index){
			index = Mathf.Clamp(index,
			                    0, list.Count);
			return list[index];
		}

		public static float InvariantConvertToSingle(object item) {
			return Convert.ToSingle(item, CultureInfo.InvariantCulture.NumberFormat);
        }
        public static IEnumerable<T> Rotate<T>(this IEnumerable<T> list, int offset) {
            return list.Skip(offset).Concat(list.Take(offset)).ToList();
        }


        private static void OnError(Delegate handler, Exception ex) {
            LogError(typeof(ObjectHelpers),$"Error invoking event handler {handler.Method.Name}: {ex.Message}");
        }
		public static void SafeInvoke(this Action callback, Action<Delegate, Exception> onError = null) {
			if (callback == null) return;
			if (onError == null) onError = OnError;

			foreach (var handler in callback.GetInvocationList()) {
				try {
					((Action)handler).Invoke();
				}
				catch (Exception ex) {
					onError?.Invoke(handler, ex);
					// Log, but continue to next subscriber
				}
			}
		}
		public static void SafeInvoke<T>(this Action<T> callback, T arg, Action<Delegate, Exception> onError = null) {
			if (callback == null) return;
			if (onError == null) onError = OnError;

			foreach (var handler in callback.GetInvocationList()) {
				try { 
					((Action<T>)handler).Invoke(arg);
				}
				catch (Exception ex) { 
					onError?.Invoke(handler, ex);
					// Log, but continue to next subscriber
				}
			}
		}
    }
}