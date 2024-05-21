using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
	public static class MeshTools {
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

        public struct MeshData {
            private Vector3[] vertices;
            private int[] indices;
            private Vector2[] uvs;
            private Vector3[] normals;
            private Vector3 total;

            public int VertexCount => (this.vertices != null) ? this.vertices.Length : 0;
            public int IndexCount => (this.indices != null) ? this.indices.Length : 0;
            public Vector3 Average => this.total / this.VertexCount;
            public Vector3 Total => this.total;

            public UnityEngine.Rendering.IndexFormat IndexFormat => (this.VertexCount > 65535) ?
                UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

            public MeshData(int vertexCount, int indexCount, 
                bool allocateUVs = false, bool allocateNormals = false) {
                this.vertices = new Vector3[vertexCount];
                this.total = Vector3.zero;
                this.indices = new int[indexCount];
                if (allocateUVs)
                    this.uvs = new Vector2[vertexCount];
                else
                    this.uvs = null;
                if (allocateNormals)
                    this.normals = new Vector3[vertexCount];
                else
                    this.normals = null;
            }
            public MeshData(Mesh mesh,
                bool allocateUVs = false, bool allocateNormals = false) {
                this.vertices = mesh.vertices;
                this.total = Vector3.zero;
                this.indices = mesh.triangles;
                if (allocateUVs && mesh.uv != null && mesh.uv.Length == mesh.vertexCount)
                    this.uvs = mesh.uv;
                else
                    this.uvs = null;
                if (allocateNormals && mesh.normals != null && mesh.normals.Length == mesh.vertexCount)
                    this.normals = mesh.normals;
                else
                    this.normals = null;
            }

            public void Resize(int vertexCount, int indexCount) { 
                if (this.vertices == null)
                    this.vertices = new Vector3[vertexCount];
                else
                    Array.Resize(ref this.vertices, vertexCount);
                if (this.uvs == null)
                    this.uvs = new Vector2[vertexCount];
                else
                    Array.Resize(ref this.uvs, vertexCount);
                if (this.normals == null)
                    this.normals = new Vector3[vertexCount];
                else
                    Array.Resize(ref this.normals, vertexCount);
                if (this.indices == null)
                    this.indices = new int[indexCount];
                else
                    Array.Resize(ref this.indices, indexCount);
            }

            public void SetVertex(int i, Vector3 vertex, Vector2? uv = null, Vector3? normal = null) {
                if (i < 0 || i >= VertexCount) return;
                this.vertices[i] = vertex;
                this.total += vertex;
                if (uv != null) this.uvs[i] = uv.Value;
                if (normal != null) this.normals[i] = normal.Value;
            }
            public Vector3 GetVertex(int i) {
                if (i < 0 || i >= VertexCount) throw new Exception($"InvalidIndex {i}");
                return this.vertices[i];
            }
            public void SetTriangle(int i, int a, int b, int c) { 
                if (i < 0 || i+2 >= this.IndexCount) return;
                this.indices[i+0] = a;
                this.indices[i+1] = b;
                this.indices[i+2] = c;
            }
            public override string ToString() {
                return "triangles: " + string.Join(",", this.indices);
            }

            public void PassData2Mesh(ref Mesh mesh, 
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
                mesh.indexFormat = this.IndexFormat;
                mesh.SetVertices(this.vertices);
                if (this.uvs != null) mesh.SetUVs(0, this.uvs);
                mesh.SetTriangles(this.indices, 0);
                if (this.normals != null && computeNormals)
                    mesh.SetNormals(this.normals);
                else if (this.normals == null && computeNormals) 
                    mesh.RecalculateNormals();
                if (computeBounds) mesh.RecalculateBounds();
                mesh.MarkModified();
                mesh.UploadMeshData(isMeshFinal);
            }
            public void Merge(MeshData other) { 
                // TODO: add option to weld vertices
                int currentVertexCount = this.VertexCount, currentIndexCount = this.IndexCount;
                // Handle Vertices
                Array.Resize(ref this.vertices, currentVertexCount + other.VertexCount);
                Array.Copy(other.vertices, 0, this.vertices, currentVertexCount, other.VertexCount);
                this.total += other.total;
                // Handle Indices
                Array.Resize(ref this.indices, currentIndexCount + other.IndexCount);
                for (int i = 0; i < other.IndexCount; i++)
                    this.indices[i + currentIndexCount] = other.indices[i] + currentVertexCount;
                // handle UVs
                if (this.uvs != null && other.uvs != null) {
                    Array.Resize(ref this.uvs, currentVertexCount + other.VertexCount);
                    Array.Copy(other.uvs, 0, this.uvs, currentVertexCount, other.VertexCount);
                } else if (this.uvs == null && other.uvs != null) {
                    this.uvs = new Vector2[currentVertexCount + other.VertexCount];
                    Array.Copy(other.uvs, 0, this.uvs, currentVertexCount, other.VertexCount);
                } else if (this.uvs != null && other.uvs == null) {
                    Array.Resize(ref this.uvs, currentVertexCount + other.VertexCount);
                }
                // handle Normals
                if (this.normals != null && other.normals != null) {
                    Array.Resize(ref this.normals, currentVertexCount + other.VertexCount);
                    Array.Copy(other.normals, 0, this.normals, currentVertexCount, other.VertexCount);
                } else if (this.normals == null && other.normals != null) {
                    this.normals = new Vector3[currentVertexCount + other.VertexCount];
                    Array.Copy(other.normals, 0, this.normals, currentVertexCount, other.VertexCount);
                } else if (this.normals != null && other.normals == null) {
                    Array.Resize(ref this.normals, currentVertexCount + other.VertexCount);
                }
            }
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
            MeshData meshData = new MeshData(vertexCount, triangleCount*3, computeUVs);

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
            Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4),
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            return SphericalGridMesh(resolution, resolution, 
                angleYawRange, anglePitchRange, radius,
                center, modifier, orientOutwards, computeUVs, computeNormals, computeBounds, isMeshFinal);
        }
        public static (MeshData meshData, Mesh mesh) SphericalGridMesh(int resolutionX, int resolutionY,
            Vector2 angleYawRange, Vector2 anglePitchRange, float radius,
            Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4), 
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = SphericalGridMeshData(resolutionX, resolutionY, 
                angleYawRange, anglePitchRange, radius,
                center, modifier, orientOutwards, computeUVs, computeNormals);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static MeshData SphericalGridMeshData(int resolutionX, int resolutionY,
            Vector2 angleYawRange, Vector2 anglePitchRange, float radius,
            Vector3 center=default(Vector3), Matrix4x4 modifier = default(Matrix4x4),
            bool orientOutwards = true, bool computeUVs = true, bool computeNormals = true) {

            if (center == default(Vector3)) center = Vector3.zero;
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
                    Vector3 vertex = center + Quaternion.AngleAxis(-angleYawPitch.x, Vector3.up) *
                        (Quaternion.AngleAxis(angleYawPitch.y, Vector3.right)
                        * -Vector3.forward * radius);
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
            meshData.PassData2Mesh(ref mesh, computeNormals,computeBounds,isMeshFinal);
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
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            mesh.SetSphericalGridMeshHeights(meshResolution, meshResolution, heightMap, heightRange, radius,
                center, modifier, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalGridMeshHeights(this Mesh mesh,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            mesh.SetSphericalGridMeshHeights(heights, radius,
                center, modifier, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalGridMeshHeights(this Mesh mesh, float[] heights, float radius,
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4),
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = true) {
            MeshData meshData = new MeshData(mesh, allocateNormals: computeNormals);
            meshData.SetSphericalGridMeshDataHeights(heights, radius, center, modifier);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
        }
        public static void SetSphericalSquareGridMeshDataHeights(this MeshData meshData,
                int meshResolution, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4)) {
            meshData.SetSphericalGridMeshDataHeights(meshResolution, meshResolution, heightMap, heightRange, radius, center, modifier);
        }
        public static void SetSphericalGridMeshDataHeights(this MeshData meshData,
                int meshResolutionX, int meshResolutionY, Texture2D heightMap,
                Vector2 heightRange, float radius,
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4)) {
            float[] heights = heightMap.SampleGrayscaleHeightMap(meshResolutionX, meshResolutionY, heightRange);
            meshData.SetSphericalGridMeshDataHeights(heights, radius, center, modifier);
        }
        public static void SetSphericalGridMeshDataHeights(this MeshData meshData, float[] heights, float radius,
                Vector3 center = default(Vector3), Matrix4x4 modifier = default(Matrix4x4)) {
            if (meshData.VertexCount == 0)
                throw new Exception("Mesh topology is invalid.");
            if (meshData.VertexCount != heights.Length)
                throw new Exception("Mesh and heights must have the same number of vertices.");

            if (modifier == default(Matrix4x4)) modifier = Matrix4x4.identity;

            for (int i = 0; i < meshData.VertexCount; i++)
                meshData.SetVertex(i, modifier.MultiplyPoint3x4(
                    center + (meshData.GetVertex(i) - center).normalized * (radius + heights[i])));
        }

        public static (MeshData meshData, Mesh mesh) ExtrudeCurve(List<Vector3> points,
                float height, Vector3 upDirection, bool capTop,
                List<List<Vector3>> innerCurves = null,
                bool computeNormals = true, bool computeBounds = true, bool isMeshFinal = false) {
            Mesh mesh = new Mesh();
            MeshData meshData = ExtrudeCurve(points, height, upDirection, capTop, innerCurves);
            meshData.PassData2Mesh(ref mesh, computeNormals, computeBounds, isMeshFinal);
            return (meshData, mesh);
        }
        public static bool EvaluateCurveWindingInPlane(List<Vector3> curve, Vector3 normal) {
            Matrix4x4 correction = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(normal, Vector3.up), Vector3.one);
            float winding = 0;
            for (int i = 0; i < curve.Count; i++) {
                Vector3 planarVertex = correction * curve[i], planarNextVertex = curve[(i + 1) % curve.Count];
                winding += (planarVertex.x + planarNextVertex.x) * (planarVertex.z - planarNextVertex.z);
            }
            return winding > 0;
        }
        // NB! expexts curves in order without duplicate point on the end to close loop (repeats start point for better UV mapping)
        // Applies uv in equal distance per side over v (in bottom half if capping top) with a gap between sides
        // Applies normals out from centers or in to centers for internal curves.
        // TODO: add unweld sides option to have more realistic normals and lighting.
        public static MeshData ExtrudeCurve(List<Vector3> outerCurve, 
                float height, Vector3 upDirection, 
                bool capTop,
                List<List<Vector3>> innerCurves = null) {
            bool hasInnerCurves = (innerCurves != null && innerCurves.Count > 0);
            List<Vector3> topCurve = new List<Vector3>();
            List<List<Vector3>> topInnerCurves = new List<List<Vector3>>();
            List<Vector3> innerCurveCenters = new List<Vector3>();

            // Compute outer curve center for normals
            Vector3 outerCurveCenter = Vector3.zero;
            foreach (Vector3 vertex in outerCurve)
                outerCurveCenter += vertex;
            outerCurveCenter /= outerCurve.Count;

            int vertexCount = (outerCurve.Count + 1) * 2;
            int triangleCount = (outerCurve.Count + 1) * 2;
            if (hasInnerCurves)
                foreach (List<Vector3> innerCurve in innerCurves) {
                    vertexCount += (innerCurve.Count + 1) * 2;
                    triangleCount += (innerCurve.Count + 1) * 2;
                    // Compute center for normals
                    Vector3 innerCurveCenter = Vector3.zero;
                    foreach (Vector3 vertex in innerCurve)
                        innerCurveCenter += vertex;
                    innerCurveCenter /= innerCurve.Count;
                    innerCurveCenters.Add(innerCurveCenter);
                }
            MeshData meshData = new MeshData(vertexCount, triangleCount*3, true, true);
            

            // Add outer curve vertices, accounting for clockwise or counter-clockwise winding.
            {
                bool clockwise = EvaluateCurveWindingInPlane(outerCurve, upDirection);
                for (int j = 0; j < outerCurve.Count+1; j++) {
                    Vector3 vertex = outerCurve[(clockwise ? j : 2*outerCurve.Count - 1 - j) % outerCurve.Count],
                        topVertex = vertex + upDirection * height,
                        normal = (vertex - outerCurveCenter).normalized;
                    meshData.SetVertex(j, vertex,
                        new Vector2(j / ((float)vertexCount / 2), 0),
                        normal);
                    meshData.SetVertex(j + outerCurve.Count + 1, topVertex,
                        new Vector2(j / ((float)vertexCount / 2), (!capTop)?1 : 0.5f),
                        normal);
                    if (j < outerCurve.Count)
                        topCurve.Add(topVertex);
                }
            }
            // Add inner curve vertices, accounting for clockwise or counter-clockwise winding.
            if (hasInnerCurves) {
                int startIndex = (outerCurve.Count + 1) * 2;
                for (int i = 0; i < innerCurves.Count; i++) {
                    List<Vector3> innerCurve = innerCurves[i],
                        topInnerCurve = new List<Vector3>();
                    bool clockwise = EvaluateCurveWindingInPlane(innerCurve, upDirection);
                    for (int j = 0; j < innerCurve.Count + 1; j++) {
                        Vector3 vertex = innerCurve[(clockwise ? j : (2*innerCurve.Count - 1 - j)) % innerCurve.Count],
                            topVertex = vertex + upDirection * height,
                            normal = -(vertex - innerCurveCenters[i]).normalized;
                        meshData.SetVertex(startIndex + j, vertex,
                            new Vector2((startIndex/2 + j) / ((float)vertexCount / 2), 0),
                            normal);
                        meshData.SetVertex(startIndex + j + innerCurve.Count + 1, topVertex,
                            new Vector2((startIndex/2 + j) / ((float)vertexCount / 2), (!capTop) ? 1 : 0.5f), 
                            normal);
                        if (j < innerCurve.Count)
                            topInnerCurve.Add(topVertex);
                    }
                    topInnerCurves.Add(topInnerCurve);
                    startIndex += (innerCurve.Count + 1) * 2;
                }
            }

            // Add triangles for outer walls
            for (int i = 0; i < outerCurve.Count; i++) {
                int ti = i * 6;
                int a = i, b = i + 1,
                    c = i + outerCurve.Count + 1, d = i + 1 + outerCurve.Count + 1;
                    meshData.SetTriangle(ti,
                        a, b, c);
                    meshData.SetTriangle(ti + 3,
                        b, d, c);
            }
            // Add triangles for inner walls (in counterClockwise manner as indices are in clockwise but triangles shoud face inwards).
            if (hasInnerCurves) { 
                int startIndex = (outerCurve.Count + 1) * 2, triangleStartIndex = outerCurve.Count * 6;
                foreach (List<Vector3> innerCurve in innerCurves) {
                    for (int i = 0; i < innerCurve.Count; i++) {
                        int ti = i * 6 + triangleStartIndex;
                        int a = i + startIndex, b = i + 1 + startIndex,
                            c = i + innerCurve.Count + 1 + startIndex, d = i + 1 + innerCurve.Count + 1 + startIndex;
                        meshData.SetTriangle(ti,
                            a, c, b);
                        meshData.SetTriangle(ti + 3,
                            b, c, d);
                    }
                    startIndex += (innerCurve.Count + 1) * 2;
                    triangleStartIndex += innerCurve.Count * 6;
                }
            }

            if (capTop && outerCurve.Count > 2) {
                MeshData topMeshData;
                //topMeshData = CapTopSimple(topCurve, upDirection);
                topMeshData = CapTop(topCurve, upDirection, topInnerCurves);
                meshData.Merge(topMeshData);
            }
            return meshData;
        }
        public static MeshData CapTopSimple(List<Vector3> outerCurve, 
                Vector3 upDirection) {
            MeshData meshData = new MeshData(outerCurve.Count, (outerCurve.Count-2) * 3, true, true);
            //List<int> indices = Enumerable.Range(2, points.Count).ToList();

            for (int i = 0; i < outerCurve.Count; i++) {
                Vector3 vertex = outerCurve[i];
                // TODO: should have other UVs
                meshData.SetVertex(i, vertex,
                    new Vector2(i / (float)(outerCurve.Count - 1), 1), upDirection);
            }

            int startIndex = 0;
            int a, b, c, d;
            float dotC, dotD;
            Vector3 currentSide, sideC, sideD, inplaneCurrentSideNormal;
            a = 0;
            b = 1;
            do {
                // Find next and previous points.
                c = b + 1;
                d = (a - 1 + outerCurve.Count) % outerCurve.Count;
                // Find vectors for the current and two possible next sides.
                currentSide = outerCurve[b] - outerCurve[a];
                sideC = outerCurve[c] - outerCurve[b];
                sideD = outerCurve[d] - outerCurve[a];
                // Compute in plane normal to check if on the correct side.
                inplaneCurrentSideNormal = //(clockwise) ?
                    Vector3.Cross(upDirection, currentSide).normalized;
                    //: Vector3.Cross(currentSide, upDirection).normalized;
                // Compute how alligned are the two next possible sides with the normal.
                dotC = Vector3.Dot(inplaneCurrentSideNormal, sideC);
                dotD = Vector3.Dot(inplaneCurrentSideNormal, sideD);
                // Pick a side and change indices for the next iteration.
                if (dotC > dotD) {
                    meshData.SetTriangle(startIndex,
                        a + outerCurve.Count, b + outerCurve.Count, c + outerCurve.Count);
                        //((clockwise) ? b : c) + outerCurve.Count, ((clockwise) ? c : b) + outerCurve.Count);
                    b = c;
                    startIndex += 3;
                }
                else {
                    meshData.SetTriangle(startIndex,
                        a + outerCurve.Count, b + outerCurve.Count, d + outerCurve.Count);
                        //((clockwise) ? b : d) + outerCurve.Count, ((clockwise) ? d : b) + outerCurve.Count);
                    a = d;
                    startIndex += 3;
                }
                // Repeat until after both c and d are the same (finished processing all points).
            } while (c != d);
            return meshData;
        }

        // NB! Expects curves organized in clockwise order to face correctly in the up direction.
        // Applies flat prorjection to plane defined by normal in top half of UV.
        // Applies normal along up direction.
        public static MeshData CapTop(List<Vector3> outerCurve, 
                Vector3 upDirection, 
                List<List<Vector3>> innerCurves=null) {
            List<Vector3> totalPoints = new List<Vector3>();
            Matrix4x4 reorient = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(upDirection, Vector3.up), Vector3.one);
            Vector2 minMaxU = new Vector2(float.MaxValue, float.MinValue),
                minMaxV = new Vector2(float.MaxValue, float.MinValue);
            outerCurve.ForEach(vertex => {
                totalPoints.Add(vertex);
                Vector3 reorientedPoint = reorient.MultiplyPoint3x4(vertex);
                minMaxU.x = Mathf.Min(minMaxU.x, reorientedPoint.x);
                minMaxU.y = Mathf.Max(minMaxU.y, reorientedPoint.x);
                minMaxV.x = Mathf.Min(minMaxV.x, reorientedPoint.z);
                minMaxV.y = Mathf.Max(minMaxV.y, reorientedPoint.z);
            });
            List<int> triangles = new List<int>();

            List<Vector3> innerCurveCenters = new List<Vector3>();
            List<List<int>> innerIndices = new List<List<int>>();
            Vector3 temp;
            int start = outerCurve.Count;
            if (innerCurves != null)
                foreach (List<Vector3> innerCurve in innerCurves) {
                    innerCurve.ForEach(item => totalPoints.Add(item));

                    temp = Vector3.zero;
                    innerCurve.ForEach(item => temp += item);
                    temp /= innerCurve.Count;
                    innerCurveCenters.Add(temp);
                    innerIndices.Add(Enumerable.Range(start, innerCurve.Count).ToList());
                    start += innerCurve.Count;
                }

            List<List<int>> availableOuterCurves = new List<List<int>>();
            List<int> currentOuterCurveIndices = Enumerable.Range(0, outerCurve.Count).ToList();
            availableOuterCurves.Add(currentOuterCurveIndices);
            while (availableOuterCurves.Count > 0) {
                currentOuterCurveIndices = availableOuterCurves[0];
                // select two points
                int a = 0, b = 1;
                Vector3 side = totalPoints[currentOuterCurveIndices[b]] - totalPoints[currentOuterCurveIndices[a]];
                Vector3 sideCenter = (totalPoints[currentOuterCurveIndices[b]] + totalPoints[currentOuterCurveIndices[a]]) / 2;
                Vector3 sideInPlaneNormal = Vector3.Cross(upDirection, side);

                bool found = false;
                int selectedInnerCurve = -1;
                int selectedInnerPointIndex = -1;
                if (innerIndices.Count > 0) {
                    // loop through inner curves
                    selectedInnerCurve = 0;
                    float allignmentTemp;
                    float minDistance = float.MaxValue, currentDistance;
                    for (int i = 0; i < innerIndices.Count; i++) {
                        for (int j = 0; j < innerIndices[selectedInnerCurve].Count; j++) {
                            currentDistance = (totalPoints[innerIndices[selectedInnerCurve][j]]
                                - sideCenter)
                                .sqrMagnitude;
                            allignmentTemp = Vector3.Dot(totalPoints[innerIndices[selectedInnerCurve][j]]
                                - sideCenter, sideInPlaneNormal);
                            if (allignmentTemp > 0 && currentDistance < minDistance) {
                                selectedInnerCurve = i;
                                minDistance = currentDistance;
                                selectedInnerPointIndex = innerIndices[selectedInnerCurve][j];
                            }
                        }
                    }
                    found = selectedInnerPointIndex >= 0;
                }

                // select next and previous
                int c = a, d = b;
                Vector3 nextSide=Vector3.zero, previousSide=Vector3.zero;
                float allignmentNext = float.MinValue, allignmentPrevious = float.MinValue;
                List<int> previousLoopIndices = new List<int>();
                List<int> nextLoopIndices = new List<int>();
                previousLoopIndices.Add(currentOuterCurveIndices[a]);
                nextLoopIndices.Add(currentOuterCurveIndices[b]);
                Vector3 nextNextSide, previousNextSide;
                do {
                    if (c == a || allignmentPrevious < 0) { 
                        c = (c - 1 + currentOuterCurveIndices.Count) % currentOuterCurveIndices.Count;
                        previousLoopIndices.Add(currentOuterCurveIndices[c]);
                        previousSide = totalPoints[currentOuterCurveIndices[a]] - totalPoints[currentOuterCurveIndices[c]];
                        allignmentPrevious = -Vector3.Dot(previousSide, sideInPlaneNormal);
                    }
                    if (d == b || allignmentNext < 0) { 
                        d = d + 1;
                        nextLoopIndices.Add(currentOuterCurveIndices[d]);
                        nextSide = totalPoints[currentOuterCurveIndices[d]] - totalPoints[currentOuterCurveIndices[b]];
                        allignmentNext = Vector3.Dot(nextSide, sideInPlaneNormal);
                    }
                    // are they on the correct side?
                    // - no - select next ansd repeat
                    // - yes - continue
                } while ((allignmentNext < 0 || allignmentPrevious < 0) && (c > d));
                nextNextSide = totalPoints[currentOuterCurveIndices[d]] - totalPoints[currentOuterCurveIndices[a]];
                previousNextSide = totalPoints[currentOuterCurveIndices[c]] - totalPoints[currentOuterCurveIndices[b]];
                Vector3 nextNextNormalInPlane = Vector3.Cross(nextNextSide, upDirection),
                    previousNextNormalInPlane = Vector3.Cross(upDirection, previousNextSide);

                // Check if inner point was selected, is it inside one of the next triangles
                if (found) {
                    Vector3 innerPointA = totalPoints[selectedInnerPointIndex] - totalPoints[currentOuterCurveIndices[a]],
                        innerPointB = totalPoints[selectedInnerPointIndex] - totalPoints[currentOuterCurveIndices[b]];
                    Vector3 nextNormal = Vector3.Cross(upDirection, nextSide);
                    bool insideNext =
                        Vector3.Dot(nextNormal, innerPointB) > 0
                        && Vector3.Dot(nextNextNormalInPlane, innerPointA) > 0;
                    found = insideNext;
                    if (!found) { 
                        Vector3 previousNormal = Vector3.Cross(upDirection, previousSide);
                        bool insidePrevious = Vector3.Dot(previousNormal, innerPointA) > 0
                            && Vector3.Dot(previousNextNormalInPlane, innerPointB) > 0;
                        found = insidePrevious;
                    } 
                }

                // if adding merging inner point
                if (found) {
                    triangles.Add(currentOuterCurveIndices[a]);
                    triangles.Add(currentOuterCurveIndices[b]);
                    triangles.Add(selectedInnerPointIndex);

                    // reorganize the curves:
                    List<int> newPoints = new List<int>();
                    int curveStartIndex = innerIndices[selectedInnerCurve][0];
                    for (int i = 0; i < innerIndices[selectedInnerCurve].Count; i++) {
                        int j =
                            curveStartIndex
                            + (selectedInnerPointIndex - curveStartIndex + i)
                                % innerIndices[selectedInnerCurve].Count;
                        newPoints.Add(j);
                    }
                    newPoints.Add(selectedInnerPointIndex);
                    newPoints.Reverse();
                    currentOuterCurveIndices.InsertRange(1, newPoints);

                    innerIndices.RemoveAt(selectedInnerCurve);
                    innerCurveCenters.RemoveAt(selectedInnerCurve);
                }
                else {
                    bool nextInPrevious = c != d && Vector3.Dot(previousNextNormalInPlane, nextSide) >= 0;
                    float distanceNext = (totalPoints[currentOuterCurveIndices[d]] - sideCenter).sqrMagnitude,
                        distancePrevious = (totalPoints[currentOuterCurveIndices[c]] - sideCenter).sqrMagnitude;
                    if ((nextInPrevious && allignmentNext > 0) || (allignmentNext > 0 && distanceNext < distancePrevious)) {
                        triangles.Add(currentOuterCurveIndices[a]);
                        triangles.Add(currentOuterCurveIndices[b]);
                        triangles.Add(currentOuterCurveIndices[d]);
                        if (nextLoopIndices.Count > 2) {
                            for (int i = 0; i < nextLoopIndices.Count - 1; i++) {
                                currentOuterCurveIndices.RemoveAt(b);
                            }
                            availableOuterCurves.Add(nextLoopIndices);
                        }
                        else
                            currentOuterCurveIndices.RemoveAt(b);
                    }
                    else {
                        triangles.Add(currentOuterCurveIndices[c]);
                        triangles.Add(currentOuterCurveIndices[a]);
                        triangles.Add(currentOuterCurveIndices[b]);
                        if (previousLoopIndices.Count > 2) {
                            int index = a;
                            for (int i = 0; i < previousLoopIndices.Count - 1; i++) {
                                currentOuterCurveIndices.RemoveAt(index);
                                index = (index - 1 + currentOuterCurveIndices.Count) % currentOuterCurveIndices.Count;
                            }
                            previousLoopIndices.Reverse();
                            availableOuterCurves.Add(previousLoopIndices);
                        }
                        else
                            currentOuterCurveIndices.RemoveAt(a);
                    }
                }
                if (currentOuterCurveIndices.Count == 2) 
                    availableOuterCurves.RemoveAt(0);
            }
            MeshData meshData = new MeshData(totalPoints.Count, triangles.Count, true, true);

            for (int i = 0; i < totalPoints.Count; i++) { 
                Vector3 reorientedPoint = reorient.MultiplyPoint3x4(totalPoints[i]);
                Vector2 uv = new Vector2(
                    Mathf.InverseLerp(minMaxU.x, minMaxU.y, reorientedPoint.x),
                    Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(minMaxV.x, minMaxV.y, reorientedPoint.z))
                );
                meshData.SetVertex(i, totalPoints[i],
                    uv, upDirection);
            }
            for (int i = 0; i < triangles.Count / 3; i++) { 
                meshData.SetTriangle(i*3, 
                    triangles[i * 3], triangles[i * 3 + 1], triangles[i * 3 + 2]
                );
            }
            return meshData;
        }
    }
}