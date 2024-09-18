using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.Geometry {
    public static partial class MeshTools {
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
                if (i < 0 || i + 2 >= this.IndexCount) return;
                this.indices[i + 0] = a;
                this.indices[i + 1] = b;
                this.indices[i + 2] = c;
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
                }
                else if (this.uvs == null && other.uvs != null) {
                    this.uvs = new Vector2[currentVertexCount + other.VertexCount];
                    Array.Copy(other.uvs, 0, this.uvs, currentVertexCount, other.VertexCount);
                }
                else if (this.uvs != null && other.uvs == null) {
                    Array.Resize(ref this.uvs, currentVertexCount + other.VertexCount);
                }
                // handle Normals
                if (this.normals != null && other.normals != null) {
                    Array.Resize(ref this.normals, currentVertexCount + other.VertexCount);
                    Array.Copy(other.normals, 0, this.normals, currentVertexCount, other.VertexCount);
                }
                else if (this.normals == null && other.normals != null) {
                    this.normals = new Vector3[currentVertexCount + other.VertexCount];
                    Array.Copy(other.normals, 0, this.normals, currentVertexCount, other.VertexCount);
                }
                else if (this.normals != null && other.normals == null) {
                    Array.Resize(ref this.normals, currentVertexCount + other.VertexCount);
                }
            }
        }
    }
}