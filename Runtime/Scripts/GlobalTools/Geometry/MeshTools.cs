using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public static partial class MeshTools {
        public static Mesh JoinMeshes(Transform[] origins, Mesh[] sources) {
            Mesh mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>(), currentVertices = new List<Vector3>(),
                normals = new List<Vector3>(), currentNormals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>(), currentUVs = new List<Vector2>();
            List<int> indices = new List<int>(), currentIndices = new List<int>(); ;

            int currentOffset = 0;
            for (int i = 0; i < sources.Length; i++) {
                currentVertices.Clear();
                sources[i].GetVertices(currentVertices);
                currentVertices.ForEach(vertex => vertices.Add(origins[i].TransformPoint(vertex)));

                currentNormals.Clear();
                sources[i].GetNormals(currentNormals);
                currentNormals.ForEach(normal => normals.Add(origins[i].TransformDirection(normal)));

                // TODO: Add support for multiple channels
                //for (int c = 0; c < 8; c++)
                int c = 0;
                {
                    currentUVs.Clear();
                    sources[i].GetUVs(c, currentUVs);
                    if (currentUVs.Count == 0)
                        currentUVs = (new Vector2[sources[i].vertexCount]).ToList();
                    uvs.AddRange(currentUVs);
                }

                for (int sm = 0; sm < sources[i].subMeshCount; sm++) {
                    currentIndices.Clear();
                    sources[i].GetTriangles(currentIndices, sm);
                    currentIndices.ForEach(index => indices.Add(index + currentOffset));
                }

                currentOffset += sources[i].vertexCount;
            }

            if (vertices.Count >= 65536)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            if (normals.Count == 0)
                mesh.RecalculateNormals();
            else
                mesh.SetNormals(normals);
            //for (int c = 0; c < 8; c++)
            //if (uvs[c] != null && uvs[c].Count > 0)
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();

            return mesh;
        }

        // Flat regular grid mesh

        // Generates a square regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static (MeshData meshData, Mesh mesh) FlatSquareGridMesh(int resolution, float size,
            Matrix4x4 modifier = default(Matrix4x4),
            bool orientUpwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            return FlatGridMesh(resolution, resolution, size, size,
                modifier, orientUpwards, computeUVs, computeNormals, computeBounds, isMeshFinal);
        }
        // Generates a regular grid mesh with the specified resolution and size.
        // By default in XY plane, centered at origin, but has Matrix parameter if needs to be modified.
        public static (MeshData meshData, Mesh mesh) FlatGridMesh(int resolutionX, int resolutionY, float sizeX, float sizeY,
            Matrix4x4 modifier = default(Matrix4x4),
            bool orientUpwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = FlatGridMeshData(resolutionX, resolutionY, sizeX, sizeY,
                modifier, orientUpwards, computeUVs, computeNormals);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static MeshData FlatGridMeshData(int resolutionX, int resolutionY, float sizeX, float sizeY,
            Matrix4x4 modifier = default(Matrix4x4),
            bool orientUpwards = true, bool computeUVs = true, bool computeNormals = true) {

            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;

            int vertexCount = (resolutionX + 1) * (resolutionY + 1);
            int triangleCount = resolutionX * resolutionY * 2;
            MeshData meshData = new MeshData(vertexCount, triangleCount * 3, computeUVs);

            Vector3 bottomLeft = new Vector3(-sizeX / 2, 0, -sizeY / 2);
            for (int y = 0; y <= resolutionY; y++) {
                for (int x = 0; x <= resolutionX; x++) {
                    int i = x + y * (resolutionX + 1);
                    Vector2 uv = new Vector2(x / (float)resolutionX, y / (float)resolutionY);
                    meshData.SetVertex(i, modifier.MultiplyPoint3x4(bottomLeft +
                        new Vector3(uv.x * sizeX, 0, uv.y * sizeY)), (computeUVs) ? uv : null);
                }
            }
            for (int y = 0; y < resolutionY; y++) {
                for (int x = 0; x < resolutionX; x++) {
                    int i = (x + y * resolutionX) * 6;
                    int a = x + y * (resolutionX + 1),
                        b = x + (y + 1) * (resolutionX + 1),
                        c = x + 1 + y * (resolutionX + 1),
                        d = x + 1 + (y + 1) * (resolutionX + 1);
                    meshData.SetTriangle(i, a, (orientUpwards) ? b : c, (orientUpwards) ? c : b);
                    meshData.SetTriangle(i + 3, c, (orientUpwards) ? b : d, (orientUpwards) ? d : b);
                }
            }
            return meshData;
        }

        // Spherical Mesh Segment
        public static (MeshData meshData, Mesh mesh) SphericalSquareGridMesh(int resolution,
            Vector2 angleYawRange, Vector2 anglePitchRange, float radius,
            Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            return SphericalGridMesh(resolution, resolution,
                angleYawRange, anglePitchRange, radius,
                center, directionModifier, modifier, orientOutwards, computeUVs, computeNormals, computeBounds, isMeshFinal);
        }
        public static (MeshData meshData, Mesh mesh) SphericalGridMesh(int resolutionX, int resolutionY,
            Vector2 angleYawRange, Vector2 anglePitchRange, float radius,
            Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = SphericalGridMeshData(resolutionX, resolutionY,
                angleYawRange, anglePitchRange, radius,
                center, directionModifier, modifier, orientOutwards, computeUVs, computeNormals);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static MeshData SphericalGridMeshData(int resolutionX, int resolutionY,
            Vector2 angleYawRange, Vector2 anglePitchRange, float radius,
            Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true) {

            if (center == default(Vector3)) center = Vector3.zero;
            if (directionModifier == default(Matrix4x4)) directionModifier = Matrix4x4.identity;
            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;

            int vertexCount = (resolutionX + 1) * (resolutionY + 1);
            int triangleCount = resolutionX * resolutionY * 2;
            MeshData meshData = new MeshData(vertexCount, triangleCount * 3, computeUVs);

            Vector2 bottomLeft = new Vector2(angleYawRange.x, anglePitchRange.x);
            Vector2 range = new Vector2(angleYawRange.y - angleYawRange.x,
                anglePitchRange.y - anglePitchRange.x);
            for (int y = 0; y <= resolutionY; y++) {
                for (int x = 0; x <= resolutionX; x++) {
                    int i = x + y * (resolutionX + 1);
                    Vector2 uv = new Vector2(x / (float)resolutionX, y / (float)resolutionY);
                    Vector2 angleYawPitch = bottomLeft
                        + Vector2.right * range.x * uv.x
                        + Vector2.up * range.y * uv.y;
                    Vector3 vertex = center +
                        directionModifier.MultiplyVector(
                            Quaternion.AngleAxis(-angleYawPitch.x, Vector3.up) *
                            (
                                Quaternion.AngleAxis(angleYawPitch.y, Vector3.right)
                                * -Vector3.forward
                            )
                        ) * radius;
                    meshData.SetVertex(i, modifier.MultiplyPoint3x4(vertex), (computeUVs) ? uv : null);
                }
            }
            for (int y = 0; y < resolutionY; y++) {
                for (int x = 0; x < resolutionX; x++) {
                    int i = (x + y * resolutionX) * 6;
                    int a = x + y * (resolutionX + 1),
                        b = x + (y + 1) * (resolutionX + 1),
                        c = x + 1 + y * (resolutionX + 1),
                        d = x + 1 + (y + 1) * (resolutionX + 1);
                    meshData.SetTriangle(i, a, (orientOutwards) ? b : c, (orientOutwards) ? c : b);
                    meshData.SetTriangle(i + 3, c, (orientOutwards) ? b : d, (orientOutwards) ? d : b);
                }
            }
            return meshData;
        }

        // Alter mesh heights
        private static float[] SampleGrayscaleHeightMap(this Texture2D heightMap, int resolutionX, int resolutionY,
            Vector2 heightRange) {
            int sampleCount = (resolutionX + 1) * (resolutionY + 1);
            Color[] heightColors = heightMap.SampleTexture(resolutionX, resolutionY);
            float[] heights = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
                heights[i] = Mathf.Lerp(heightRange.x, heightRange.y, heightColors[i].grayscale);
            return heights;
        }
        // - Flat
        public static void SetFlatSquareGridMeshHeights(this Mesh mesh,
                int meshResolution, Texture2D heightMap,
                Vector2 heightRange,
                Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            mesh.SetFlatGridMeshHeights(meshResolution, meshResolution, heightMap, heightRange,
                modifier,
                computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetFlatGridMeshHeights(this Mesh mesh,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange,
                Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            mesh.SetFlatGridMeshHeights(heights, modifier, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetFlatGridMeshHeights(this Mesh mesh, float[] heights,
                Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            MeshData meshData = new MeshData(mesh, allocateNormals: computeNormals);
            meshData.SetFlatGridMeshDataHeights(heights, modifier);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetFlatSquareGridMeshDataHeights(this MeshData meshData,
                int meshResolution, Texture2D heightMap,
                Vector2 heightRange,
                Matrix4x4 modifier = default(Matrix4x4)) {
            meshData.SetFlatGridMeshDataHeights(meshResolution, meshResolution, heightMap, heightRange, modifier);
        }
        public static void SetFlatGridMeshDataHeights(this MeshData meshData,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange,
                Matrix4x4 modifier = default(Matrix4x4)) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            meshData.SetFlatGridMeshDataHeights(heights, modifier);
        }
        public static void SetFlatGridMeshDataHeights(this MeshData meshData, float[] heights,
                Matrix4x4 modifier = default(Matrix4x4)) {
            if (meshData.VertexCount == 0)
                throw new Exception("Mesh topology is invalid.");
            if (meshData.VertexCount != heights.Length)
                throw new Exception("Mesh and heights must have the same number of vertices.");

            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;

            for (int i = 0; i < meshData.VertexCount; i++)
                meshData.SetVertex(i, modifier.MultiplyPoint3x4(meshData.GetVertex(i).XZ().ToXZ(heights[i])));
        }


        // - Spherical
        public static void SetSphericalSquareGridMeshHeights(this Mesh mesh,
                int meshResolution, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            mesh.SetSphericalGridMeshHeights(meshResolution, meshResolution, heightMap, heightRange, radius,
                center, directionModifier, modifier, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalGridMeshHeights(this Mesh mesh,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            mesh.SetSphericalGridMeshHeights(heights, radius,
                center, directionModifier, modifier, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalGridMeshHeights(this Mesh mesh, float[] heights, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            MeshData meshData = new MeshData(mesh, allocateNormals: computeNormals);
            meshData.SetSphericalGridMeshDataHeights(heights, radius, center, directionModifier, modifier);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalSquareGridMeshDataHeights(this MeshData meshData,
                int meshResolution, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4)) {
            meshData.SetSphericalGridMeshDataHeights(meshResolution, meshResolution, heightMap, heightRange, radius, center, directionModifier, modifier);
        }
        public static void SetSphericalGridMeshDataHeights(this MeshData meshData,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4)) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            meshData.SetSphericalGridMeshDataHeights(heights, radius, center, directionModifier, modifier);
        }
        public static void SetSphericalGridMeshDataHeights(this MeshData meshData, float[] heights, float radius,
                Vector3 center = default(Vector3), Matrix4x4 directionModifier = default(Matrix4x4), Matrix4x4 modifier = default(Matrix4x4)) {
            if (meshData.VertexCount == 0)
                throw new Exception("Mesh topology is invalid.");
            if (meshData.VertexCount != heights.Length)
                throw new Exception("Mesh and heights must have the same number of vertices.");

            if (directionModifier == default(Matrix4x4)) directionModifier = Matrix4x4.identity;
            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;

            for (int i = 0; i < meshData.VertexCount; i++)
                meshData.SetVertex(i, modifier.MultiplyPoint3x4(
                    center +
                    directionModifier.MultiplyVector(
                        (meshData.GetVertex(i) - center).normalized
                    ) * (radius + heights[i])
                ));
        }
    }
}