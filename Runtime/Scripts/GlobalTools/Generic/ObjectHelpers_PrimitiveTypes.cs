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
    }
}