using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Generic {
    public static partial class ObjectHelpers {
        public static int[] ToIntArray(this Vector3Int vector) { 
            return new int[] { vector.x, vector.y, vector.z };
        }
        public static int[] ToIntArray(this Vector2Int vector) { 
            return new int[] { vector.x, vector.y };
        }
        public static Vector3Int ToVector3Int(this int[] array) {
            if (array.Length == 3) {
                return new Vector3Int(array[0], array[1], array[2]);
            }
            return Vector3Int.zero;
        }
    }
}