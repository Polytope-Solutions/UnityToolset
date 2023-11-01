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

        public static Vector2 ToXY(this Vector3 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 ToYX(this Vector3 vector) => new Vector2(vector.y, vector.x);
        public static Vector2 ToXZ(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 ToZX(this Vector3 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 ToYZ(this Vector3 vector) => new Vector2(vector.y, vector.z);
        public static Vector2 ToZY(this Vector3 vector) => new Vector2(vector.z, vector.y);

        public static Vector3 ToXY(this Vector2 vector, float extraValue = 0) => new Vector3(vector.x, vector.y, extraValue);
        public static Vector3 ToYX(this Vector2 vector, float extraValue = 0) => new Vector3(vector.y, vector.x, extraValue);
        public static Vector3 ToXZ(this Vector2 vector, float extraValue = 0) => new Vector3(vector.x, extraValue, vector.y);
        public static Vector3 ToZX(this Vector2 vector, float extraValue = 0) => new Vector3(vector.y, extraValue, vector.x);
        public static Vector3 ToYZ(this Vector2 vector, float extraValue = 0) => new Vector3(extraValue, vector.x, vector.y);
        public static Vector3 ToZY(this Vector2 vector, float extraValue = 0) => new Vector3(extraValue, vector.y, vector.x);
    }
}