using System;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public struct SpatialRange {
        private Vector2 rangeX, rangeY, rangeZ;
        private int hashcode;
        public float MinX => this.rangeX.x;
        public float MaxX => this.rangeX.y;
        public float MinY => this.rangeY.x;
        public float MaxY => this.rangeY.y;
        public float MinZ => this.rangeZ.x;
        public float MaxZ => this.rangeZ.y;
        public float HalfX => (this.rangeX.x + this.rangeX.y) * .5f;
        public float HalfY => (this.rangeY.x + this.rangeY.y) * .5f;
        public float HalfZ => (this.rangeZ.x + this.rangeZ.y) * .5f;

        public SpatialRange(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) 
            : this(new Vector2(minX, maxX), new Vector2 (minY, maxY), new Vector2 (minZ, maxZ)) { }
        public SpatialRange(Vector2 rangeX, Vector2 rangeY, Vector2 rangeZ) {
            this.rangeX = rangeX;
            this.rangeY = rangeY;
            this.rangeZ = rangeZ;
            this.hashcode = HashCode.Combine(
                this.rangeX.x, this.rangeX.y,
                this.rangeY.x, this.rangeY.y,
                this.rangeZ.x, this.rangeZ.y);
        }
        public static SpatialRange FromBounds(Bounds bounds, SpatialRange boundingRange) {
            return new SpatialRange(
                Mathf.InverseLerp(boundingRange.MinX, boundingRange.MaxX, bounds.min.x),
                Mathf.InverseLerp(boundingRange.MinX, boundingRange.MaxX, bounds.max.x),
                Mathf.InverseLerp(boundingRange.MinY, boundingRange.MaxZ, bounds.min.y),
                Mathf.InverseLerp(boundingRange.MinY, boundingRange.MaxY, bounds.max.y),
                Mathf.InverseLerp(boundingRange.MinZ, boundingRange.MaxZ, bounds.min.z),
                Mathf.InverseLerp(boundingRange.MinZ, boundingRange.MaxZ, bounds.max.z)
            );
        }

        public bool Intesects(SpatialRange other) {
            return  (
                (
                       (this.rangeX.x >= other.rangeX.x && this.rangeX.x <= other.rangeX.y)
                    || (this.rangeX.y >= other.rangeX.x && this.rangeX.y <= other.rangeX.y)
                    || (this.rangeX.x <= other.rangeX.x && this.rangeX.y >= other.rangeX.y)
                ) // in X range
                && (
                       (this.rangeY.x >= other.rangeY.x && this.rangeY.x <= other.rangeY.y)
                    || (this.rangeY.y >= other.rangeY.x && this.rangeY.y <= other.rangeY.y)
                    || (this.rangeY.x <= other.rangeY.x && this.rangeY.y >= other.rangeY.y)
                ) // in Y range
                && (
                       (this.rangeZ.x >= other.rangeZ.x && this.rangeZ.x <= other.rangeZ.y)
                    || (this.rangeZ.y >= other.rangeZ.x && this.rangeZ.y <= other.rangeZ.y)
                    || (this.rangeZ.x <= other.rangeZ.x && this.rangeZ.y >= other.rangeZ.y)
                ) // in Z range
            );
        }
        public override int GetHashCode()
            => this.hashcode;

        public override string ToString() {
            return this.ToString("F3");
        }
        public string ToString(string format = "F3") {
            return $"[{this.rangeX.ToString(format)},{this.rangeY.ToString(format)},{this.rangeZ.ToString(format)}]";
        }
    }
}
