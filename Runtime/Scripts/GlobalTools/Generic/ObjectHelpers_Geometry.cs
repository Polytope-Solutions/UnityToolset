using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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

        public static Vector2Int FloorToInt(this Vector2 vector) => new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
        public static Vector2Int CeilToInt(this Vector2 vector) => new Vector2Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
        public static Vector2Int RoundToInt(this Vector2 vector) => new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
        public static Vector3Int FloorToInt(this Vector3 vector) => new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
        public static Vector3Int CeilToInt(this Vector3 vector) => new Vector3Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y), Mathf.FloorToInt(vector.z));
        public static Vector3Int RoundToInt(this Vector3 vector) => new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.FloorToInt(vector.z));
        public static Vector3Double FloorToInt(this Vector3Double vector) => new Vector3Double(Math.Floor(vector.x), Math.Floor(vector.y), Math.Floor(vector.z));
        public static Vector3Double CeilToInt(this Vector3Double vector) => new Vector3Double(Math.Ceiling(vector.x), Math.Ceiling(vector.y), Math.Ceiling(vector.z));
        public static Vector3Double RoundToInt(this Vector3Double vector) => new Vector3Double(Math.Round(vector.x), Math.Round(vector.y), Math.Round(vector.z));

        public static Vector2 XY(this Vector3 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 YX(this Vector3 vector) => new Vector2(vector.y, vector.x);
        public static Vector2 XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 ZX(this Vector3 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 YZ(this Vector3 vector) => new Vector2(vector.y, vector.z);
        public static Vector2 ZY(this Vector3 vector) => new Vector2(vector.z, vector.y);

        public static Vector3 ToXY(this Vector2 vector, float extraValue = 0) => new Vector3(vector.x, vector.y, extraValue);
        public static Vector3 ToYX(this Vector2 vector, float extraValue = 0) => new Vector3(vector.y, vector.x, extraValue);
        public static Vector3 ToXZ(this Vector2 vector, float extraValue = 0) => new Vector3(vector.x, extraValue, vector.y);
        public static Vector3 ToZX(this Vector2 vector, float extraValue = 0) => new Vector3(vector.y, extraValue, vector.x);
        public static Vector3 ToYZ(this Vector2 vector, float extraValue = 0) => new Vector3(extraValue, vector.x, vector.y);
        public static Vector3 ToZY(this Vector2 vector, float extraValue = 0) => new Vector3(extraValue, vector.y, vector.x);

        public static Vector4 ToVector4(this Quaternion quternion) =>
            new Vector4(quternion.x, quternion.y, quternion.z, quternion.w);

    }
    [System.Serializable]
    public struct Vector3Double {
        public double x;
        public double y;
        public double z;

        public float xf => (float)this.x;
        public float yf => (float)this.y;
        public float zf => (float)this.z;

        public double sqrMagnitude => this.x * this.x + this.y * this.y + this.z * this.z;
        public double magnitude => Math.Sqrt(this.sqrMagnitude);

        public Vector3Double normalized => this / this.magnitude;

        public static Vector3Double zero => new Vector3Double(0, 0, 0);
        public static Vector3Double one => new Vector3Double(1, 1, 1);
        public static Vector3Double right => new Vector3Double(1, 0, 0);
        public static Vector3Double up => new Vector3Double(0, 1, 0);
        public static Vector3Double forward => new Vector3Double(0, 0, 1);

        public Vector3Double(double _x = 0, double _y = 0, double _z = 0) {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
        public Vector3Double(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public Vector3 ToVector3() {
            return new Vector3((float)this.x, (float)this.y, (float)this.z);
        }

        public static Vector3Double operator +(Vector3Double v1, Vector3Double v2) => new Vector3Double(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        public static Vector3Double operator -(Vector3Double v1, Vector3Double v2) => new Vector3Double(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        public static Vector3Double operator *(Vector3Double v, double s) => new Vector3Double(v.x * s, v.y * s, v.z * s);
        public static Vector3Double operator /(Vector3Double v, double s) => new Vector3Double(v.x / s, v.y / s, v.z / s);
    }
}