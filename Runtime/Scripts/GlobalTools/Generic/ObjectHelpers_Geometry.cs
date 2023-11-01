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

        // Generates a square regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static Mesh RegularSquareGridMesh(int resolution, float size,
            Matrix4x4 modifier = default(Matrix4x4),
            bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            return RegularGridMesh(resolution, resolution, size, size,
                modifier, computeUVs, computeNormals, computeBounds, isMeshFinal);
        }
        // Generates a regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static Mesh RegularGridMesh(int resolutionX, int resolutionY, float sizeX, float sizeY, 
            Matrix4x4 modifier = default(Matrix4x4), 
            bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;
            Mesh mesh = new Mesh();

            int vertexCount = (resolutionX + 1) * (resolutionY + 1);
            int triangleCount = resolutionX * resolutionY * 2;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[triangleCount * 3];
            Vector3 bottomLeft = new Vector3(-sizeX / 2, 0, -sizeY / 2);

            for(int y = 0; y <= resolutionY; y++) {
                for (int x = 0; x <= resolutionX; x++) {
                    int i = x + y * (resolutionX + 1);
                    Vector2 uv = new Vector2(x / (float)resolutionX, y / (float)resolutionY);
                    vertices[i] = modifier.MultiplyPoint3x4(bottomLeft + 
                        new Vector3(uv.x*sizeX, 0, uv.y*sizeY));
                    if (computeUVs) 
                        uvs[i] = uv;
                }
            }
            for (int y = 0; y < resolutionY; y++) {
                for (int x = 0; x < resolutionX; x++) {
                    int i = (x + y * resolutionX) * 6;
                    triangles[i + 0] = x + y * (resolutionX + 1);
                    triangles[i + 1] = x + (y + 1) * (resolutionX + 1);
                    triangles[i + 2] = x + 1 + y * (resolutionX + 1);
                    triangles[i + 3] = x + 1 + y * (resolutionX + 1);
                    triangles[i + 4] = x + (y + 1) * (resolutionX + 1);
                    triangles[i + 5] = x + 1 + (y + 1) * (resolutionX + 1);
                }
            }
            mesh.indexFormat = (vertexCount > 65535) ? 
                UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            if (computeUVs) mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            if (computeNormals) mesh.RecalculateNormals();
            if (computeBounds) mesh.RecalculateBounds();
            mesh.UploadMeshData(isMeshFinal);
            return mesh;
        }
        public static void SetHeights(this Mesh mesh,
            int meshResolution, Texture2D heightMap,
            Vector2 heightRange,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            mesh.SetHeights(meshResolution, meshResolution, heightMap, heightRange, 
                computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetHeights(this Mesh mesh, 
            int meshResolutionX, int meshResolutionY, Texture2D heightMap, 
            Vector2 heightRange,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) { 
            int vertexCount = (meshResolutionX + 1) * (meshResolutionY + 1);
            if (mesh.vertexCount != vertexCount)
                throw new Exception("Mesh resolution doesn't match mesh topology.");
            Color[] heightColors = heightMap.SampleTexture(meshResolutionX, meshResolutionY);
            float[] heights = new float[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                heights[i] = Mathf.Lerp(heightRange.x, heightRange.y, heightColors[i].grayscale);
            mesh.SetHeights(heights, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetHeights(this Mesh mesh, float[] heights,
            bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            if (vertices.Count == 0) 
                throw new Exception("Mesh topology is invalid.");
            if (vertices.Count != heights.Length)
                throw new Exception("Mesh and heights must have the same number of vertices.");

            for (int i = 0; i < vertices.Count; i++) {
                vertices[i] = vertices[i].XZ().ToXZ(heights[i]);
            }
            mesh.SetVertices(vertices);
            if (computeNormals) mesh.RecalculateNormals();
            if (computeBounds) mesh.RecalculateBounds();
            mesh.MarkModified();
            mesh.UploadMeshData(isMeshFinal);
        }
    }
}